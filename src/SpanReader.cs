using System;
using System.Buffers;

namespace DevHawk.Buffers
{
    public ref struct SpanReader<T> where T : unmanaged
    {
        public SpanReader(ReadOnlySpan<T> span)
        {
            throw new NotImplementedException();
        }

        public SpanReader(in ReadOnlySequence<T> span)
        {
            throw new NotImplementedException();
        }

        public bool End => throw new NotImplementedException();

        public ReadOnlySpan<T> CurrentSpan => throw new NotImplementedException();

        public int CurrentSpanIndex => throw new NotImplementedException();

        public ReadOnlySpan<T> UnreadSpan => throw new NotImplementedException();

        public long Consumed => throw new NotImplementedException();

        public long Remaining => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public bool TryPeek(out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(out T value)
        {
            throw new NotImplementedException();
        }
        public void Rewind(long count)
        {
            throw new NotImplementedException();
        }

        public void Advance(long count)
        {
            throw new NotImplementedException();
        }

        public bool TryCopyTo(Span<T> destination)
        {
            throw new NotImplementedException();
        }
    }
}
