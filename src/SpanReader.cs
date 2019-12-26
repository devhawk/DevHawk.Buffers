﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DevHawk.Buffers
{
    public ref struct SpanReader<T> where T : unmanaged
    {
        private bool usingSequence;
        private readonly ReadOnlySequence<T> sequence;
        private SequencePosition currentPosition;
        private SequencePosition nextPosition;
        private bool moreData;
        private readonly long length;

        public SpanReader(ReadOnlySpan<T> span)
        {
            usingSequence = false;
            CurrentSpanIndex = 0;
            Consumed = 0;
            this.sequence = default;
            currentPosition = default;
            length = span.Length;

            CurrentSpan = span;
            nextPosition = default;
            moreData = span.Length > 0;
        }

        public SpanReader(in ReadOnlySequence<T> sequence)
        {
            usingSequence = true;
            CurrentSpanIndex = 0;
            Consumed = 0;
            this.sequence = sequence;
            currentPosition = sequence.Start;
            length = -1;

            var first = sequence.First.Span;
            CurrentSpan = first;
            nextPosition = sequence.GetPosition(first.Length);
            moreData = first.Length > 0;

            if (!moreData && !sequence.IsSingleSegment)
            {
                moreData = true;
                GetNextSpan();
            }
        }

        public readonly bool End => !moreData;

        public ReadOnlySpan<T> CurrentSpan { readonly get; private set; }

        public int CurrentSpanIndex { readonly get; private set; }

        public readonly ReadOnlySpan<T> UnreadSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentSpan.Slice(CurrentSpanIndex);
        }

        public long Consumed { readonly get; private set; }

        public readonly long Remaining => Length - Consumed;

        public readonly long Length
        {
            get
            {
                if (length < 0)
                {
                    Debug.Assert(usingSequence, "usingSequence");
                    // Cast-away readonly to initialize lazy field
                    Volatile.Write(ref Unsafe.AsRef(length), sequence.Length);
                }
                return length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out T value)
        {
            if (moreData)
            {
                value = CurrentSpan[CurrentSpanIndex];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out T value)
        {
            if (End)
            {
                value = default;
                return false;
            }

            value = CurrentSpan[CurrentSpanIndex];
            CurrentSpanIndex++;
            Consumed++;

            if (CurrentSpanIndex >= CurrentSpan.Length)
            {
                if (usingSequence)
                {
                    GetNextSpan();
                }
                else
                {
                    moreData = false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(long count)
        {
            if ((ulong)count > (ulong)Consumed)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Consumed -= count;

            if (CurrentSpanIndex >= count)
            {
                CurrentSpanIndex -= (int)count;
                moreData = true;
            }
            else if (usingSequence)
            {
                // Current segment doesn't have enough data, scan backward through segments
                RetreatToPreviousSpan(Consumed);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Rewind went past the start of the memory.", nameof(count));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetreatToPreviousSpan(long consumed)
        {
            Debug.Assert(usingSequence, "usingSequence");
            ResetReader();
            Advance(consumed);
        }

        private void ResetReader()
        {
            Debug.Assert(usingSequence, "usingSequence");
            CurrentSpanIndex = 0;
            Consumed = 0;
            currentPosition = sequence.Start;
            nextPosition = currentPosition;

            if (sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
            {
                moreData = true;

                if (memory.Length == 0)
                {
                    CurrentSpan = default;
                    // No data in the first span, move to one with data
                    GetNextSpan();
                }
                else
                {
                    CurrentSpan = memory.Span;
                }
            }
            else
            {
                // No data in any spans and at end of sequence
                moreData = false;
                CurrentSpan = default;
            }
        }

        private void GetNextSpan()
        {
            Debug.Assert(usingSequence, "usingSequence");
            if (!sequence.IsSingleSegment)
            {
                SequencePosition previousNextPosition = nextPosition;
                while (sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
                {
                    currentPosition = previousNextPosition;
                    if (memory.Length > 0)
                    {
                        CurrentSpan = memory.Span;
                        CurrentSpanIndex = 0;
                        return;
                    }
                    else
                    {
                        CurrentSpan = default;
                        CurrentSpanIndex = 0;
                        previousNextPosition = nextPosition;
                    }
                }
            }
            moreData = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            if ((count & TooBigOrNegative) == 0 && CurrentSpan.Length - CurrentSpanIndex > (int)count)
            {
                CurrentSpanIndex += (int)count;
                Consumed += count;
            }
            else if (usingSequence)
            {
                // Can't satisfy from the current span
                AdvanceToNextSpan(count);
            }
            else if (this.CurrentSpan.Length - this.CurrentSpanIndex == (int)count)
            {
                this.CurrentSpanIndex += (int)count;
                this.Consumed += count;
                this.moreData = false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private void AdvanceToNextSpan(long count)
        {
            Debug.Assert(this.usingSequence, "usingSequence");
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Consumed += count;
            while (moreData)
            {
                int remaining = CurrentSpan.Length - CurrentSpanIndex;

                if (remaining > count)
                {
                    CurrentSpanIndex += (int)count;
                    count = 0;
                    break;
                }

                // As there may not be any further segments we need to
                // push the current index to the end of the span.
                CurrentSpanIndex += remaining;
                count -= remaining;
                Debug.Assert(count >= 0);

                GetNextSpan();

                if (count == 0)
                {
                    break;
                }
            }

            if (count != 0)
            {
                // Not enough data left- adjust for where we actually ended and throw
                Consumed -= count;
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
        {
            // This API doesn't advance to facilitate conditional advancement based on the data returned.
            // We don't provide an advance option to allow easier utilizing of stack allocated destination spans.
            // (Because we can make this method readonly we can guarantee that we won't capture the span.)

            ReadOnlySpan<T> firstSpan = UnreadSpan;
            if (firstSpan.Length >= destination.Length)
            {
                firstSpan.Slice(0, destination.Length).CopyTo(destination);
                return true;
            }

            // Not enough in the current span to satisfy the request, fall through to the slow path
            return TryCopyMultisegment(destination);
        }

        internal readonly bool TryCopyMultisegment(Span<T> destination)
        {
            Debug.Assert(this.usingSequence, "usingSequence");

            // If we don't have enough to fill the requested buffer, return false
            if (Remaining < destination.Length)
                return false;

            ReadOnlySpan<T> firstSpan = UnreadSpan;
            Debug.Assert(firstSpan.Length < destination.Length);
            firstSpan.CopyTo(destination);
            int copied = firstSpan.Length;

            SequencePosition next = nextPosition;
            while (sequence.TryGet(ref next, out ReadOnlyMemory<T> nextSegment, true))
            {
                if (nextSegment.Length > 0)
                {
                    ReadOnlySpan<T> nextSpan = nextSegment.Span;
                    int toCopy = Math.Min(nextSpan.Length, destination.Length - copied);
                    nextSpan.Slice(0, toCopy).CopyTo(destination.Slice(copied));
                    copied += toCopy;
                    if (copied >= destination.Length)
                    {
                        break;
                    }
                }
            }

            return true;
        }
    }
}
