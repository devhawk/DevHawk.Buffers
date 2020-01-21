using System;
using Xunit;
using FluentAssertions;
using DevHawk.Buffers;
using Nerdbank.Streams;
using System.Buffers;
using System.Linq;

namespace DevHawk.BuffersTest
{
    internal class BufferSegment<T> : ReadOnlySequenceSegment<T>
    {
        public BufferSegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public BufferSegment<T> Append(ReadOnlyMemory<T> memory)
        {
            var segment = new BufferSegment<T>(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }
    }

    public class BufferReaderTests
    {

        ReadOnlySequence<byte> GetSequence(params Memory<byte>[] memories)
        {
            if (memories.Length < 2) throw new ArgumentException(nameof(memories));

            var first = new BufferSegment<byte>(memories[0]);
            var last = first;
            foreach (var memory in memories.Skip(1))
            {
                last = last.Append(memory);
            }

            return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
        }

        private void Test_Source(BufferReader<byte> reader, int length)
        {
            reader.Length.Should().Be(length);

            for (int i = 0; i < reader.Length; i++)
            {
                reader.End.Should().BeFalse();
                reader.TryRead(out byte value).Should().BeTrue();
                i.Should().Be(value);
            }
            reader.End.Should().BeTrue();
            reader.TryRead(out byte _).Should().BeFalse();
        }

        [Fact]
        public void BufferReader_Span_Source()
        {
            Span<byte> span = stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var reader = new BufferReader<byte>(span);
            Test_Source(reader, span.Length);
        }

        [Fact]
        public void BufferReader_Sequence_Source()
        {
            var sequence = GetSequence(new byte[] { 0, 1, 2 }, new byte[] { 3, 4, 5, 6, 7 }, new byte[] { 8, 9 });
            sequence.Length.Should().BeLessThan(int.MaxValue);

            var reader = new BufferReader<byte>(sequence);
            Test_Source(reader, (int)sequence.Length);
        }

        private void Test_Advance(BufferReader<byte> reader, int advance = 5)
        {
            reader.Advance(advance);

            for (int i = advance; i < reader.Length; i++)
            {
                reader.End.Should().BeFalse();
                reader.TryRead(out byte value).Should().BeTrue();
                i.Should().Be(value);
            }
            reader.End.Should().BeTrue();
            reader.TryRead(out byte _).Should().BeFalse();
        }

        [Fact]
        public void BufferReader_Span_Advance()
        {
            Span<byte> span = stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var reader = new BufferReader<byte>(span);
            Test_Advance(reader);
        }

        [Fact]
        public void BufferReader_Sequence_Advance()
        {
            var sequence = GetSequence(new byte[] { 0, 1, 2 }, new byte[] { 3, 4, 5, 6, 7 }, new byte[] { 8, 9 });

            var reader = new BufferReader<byte>(sequence);
            Test_Advance(reader);
        }

        private void Test_Rewind(BufferReader<byte> reader)
        {
            var count = reader.Length - 1;

            for (int i = 0; i < count; i++)
            {
                reader.End.Should().BeFalse();
                reader.TryRead(out byte value).Should().BeTrue();
                i.Should().Be(value);
            }

            reader.Rewind(count);

            for (int i = 0; i < reader.Length; i++)
            {
                reader.End.Should().BeFalse();
                reader.TryRead(out byte value).Should().BeTrue();
                i.Should().Be(value);
            }

            reader.End.Should().BeTrue();
            reader.TryRead(out byte _).Should().BeFalse();
        }

        [Fact]
        public void BufferReader_Span_Rewind()
        {
            Span<byte> span = stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var reader = new BufferReader<byte>(span);
            Test_Rewind(reader);
        }

        [Fact]
        public void BufferReader_Sequence_Rewind()
        {
            var sequence = GetSequence(new byte[] { 0, 1, 2 }, new byte[] { 3, 4, 5, 6, 7 }, new byte[] { 8, 9 });

            var reader = new BufferReader<byte>(sequence);
            Test_Rewind(reader);
        }

        private void Test_TryCopyTo(BufferReader<byte> reader, ReadOnlySpan<byte> expected)
        {
            reader.Length.Should().Be(expected.Length);

            Span<byte> actual = stackalloc byte[expected.Length];
            reader.TryCopyTo(actual).Should().BeTrue();
            actual.SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public void BufferReader_Span_TryCopyTo()
        {
            Span<byte> span = stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var reader = new BufferReader<byte>(span);
            Test_TryCopyTo(reader, span);
        }

        [Fact]
        public void BufferReader_Sequence_TryCopyTo()
        {
            Span<byte> expected = stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sequence = GetSequence(new byte[] { 0, 1, 2 }, new byte[] { 3, 4, 5, 6, 7 }, new byte[] { 8, 9 });

            var reader = new BufferReader<byte>(sequence);
            Test_TryCopyTo(reader, expected);
        }

    }
}
