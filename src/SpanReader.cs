using System;
using System.Buffers;
using System.Diagnostics;

namespace DevHawk.Buffers
{
    public ref struct SpanReader<T> where T : unmanaged
    {
        private readonly ReadOnlySpan<T> span;
        private readonly ReadOnlySequence<T> sequence;
        private SequencePosition currentPosition;
        private SequencePosition nextPosition;

        public SpanReader(ReadOnlySpan<T> span)
        {
            this.span = span;
            this.sequence = default;

            this.CurrentSpanIndex = 0;
            this.Consumed = 0;

            CurrentSpan = span;
            Length = span.Length;

            this.currentPosition = default;
            this.nextPosition = default; 
        }

        private SpanReader(in ReadOnlySequence<T> sequence)
        {
            Debug.Assert(!sequence.IsSingleSegment);

            this.span = default;
            this.sequence = sequence;

            this.CurrentSpanIndex = 0;
            this.Consumed = 0;

            CurrentSpan = sequence.First.Span;
            Length = sequence.Length;

            this.currentPosition = sequence.Start;
            this.nextPosition = sequence.GetPosition(CurrentSpan.Length); 
        }

        public static SpanReader<T> Create(in ReadOnlySequence<T> sequence)
        {
            return sequence.IsSingleSegment
                ? new SpanReader<T>(sequence.FirstSpan)
                : new SpanReader<T>(sequence);
        }

        public bool End => Length > Consumed;

        public ReadOnlySpan<T> CurrentSpan { get; private set; }

        public int CurrentSpanIndex { get; private set; }

        public ReadOnlySpan<T> UnreadSpan => CurrentSpan.Slice(CurrentSpanIndex);

        public long Consumed { get; private set; }

        public long Remaining => this.Length - this.Consumed;

        public long Length { get; private set; }

        public bool TryPeek(out T value)
        {
            if (End)
            {
                value = default;
                return false;
            }
            else
            {
                value = CurrentSpan[CurrentSpanIndex];
                return true;
            }
        }
 
        public bool TryRead(out T value)
        {
            if (TryPeek(out value))
            {
                Advance(1);
                return true;
            }

            return false;
        }

        // public void Rewind(long count)
        // {
        //     throw new NotImplementedException();
        // }

        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            if ((count & TooBigOrNegative) == 0 && this.CurrentSpan.Length - this.CurrentSpanIndex > (int)count)
            {
                this.CurrentSpanIndex += (int)count;
                this.Consumed += count;
            }
            else if (!this.sequence.IsEmpty)
            {
                AdvanceToNextSpan(count);
            }
            else if (this.CurrentSpan.Length - this.CurrentSpanIndex == (int)count)
            {
                this.CurrentSpanIndex += (int)count;
                this.Consumed += count;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private void AdvanceToNextSpan(long count)
        {
            // Debug.Assert(this.usingSequence, "usingSequence");
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            this.Consumed += count;
            while (!this.End)
            {
                int remaining = this.CurrentSpan.Length - this.CurrentSpanIndex;

                if (remaining > count)
                {
                    this.CurrentSpanIndex += (int)count;
                    count = 0;
                    break;
                }

                // As there may not be any further segments we need to
                // push the current index to the end of the span.
                this.CurrentSpanIndex += remaining;
                count -= remaining;
                Debug.Assert(count >= 0, "count >= 0");

                this.GetNextSpan();

                if (count == 0)
                {
                    break;
                }
            }

            if (count != 0)
            {
                // Not enough data left- adjust for where we actually ended and throw
                this.Consumed -= count;
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private void GetNextSpan()
        {
            // Debug.Assert(this.usingSequence, "usingSequence");
            if (!this.sequence.IsSingleSegment)
            {
                SequencePosition previousNextPosition = this.nextPosition;
                while (this.sequence.TryGet(ref this.nextPosition, out ReadOnlyMemory<T> memory, advance: true))
                {
                    this.currentPosition = previousNextPosition;
                    if (memory.Length > 0)
                    {
                        this.CurrentSpan = memory.Span;
                        this.CurrentSpanIndex = 0;
                        return;
                    }
                    else
                    {
                        this.CurrentSpan = default;
                        this.CurrentSpanIndex = 0;
                        previousNextPosition = this.nextPosition;
                    }
                }
            }

            // this.moreData = false;
        }

        public bool TryAdvance(long count)
        {
            if (this.Remaining < count)
            {
                return false;
            }

            this.Advance(count);
            return true;
        }

        public bool TryCopyTo(Span<T> destination)
        {
            var unreadSpan = this.UnreadSpan; 
            if (unreadSpan.Length >= destination.Length)
            {
                unreadSpan
                    .Slice(0, destination.Length)
                    .CopyTo(destination);
                return true;
            }

            return TryCopyMultisegment(destination);
        }

        private bool TryCopyMultisegment(Span<T> destination)
        {
            if (Remaining < destination.Length)
            {
                return false;
            }

            var unreadSpan = this.UnreadSpan; 
            Debug.Assert(unreadSpan.Length < destination.Length);
            unreadSpan.CopyTo(destination);
            int copied = unreadSpan.Length;

            SequencePosition next = this.nextPosition;
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
