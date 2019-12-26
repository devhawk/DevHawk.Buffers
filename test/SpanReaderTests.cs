using System;
using Xunit;
using FluentAssertions;
using DevHawk.Buffers;
using Nerdbank.Streams;
using System.Buffers;

namespace DevHawk.BuffersTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Span<byte> span = new byte[] {0,1,2,3,4,5,6,7,8,9};
            var reader = new SpanReader<byte>(span);

            reader.TryPeek(out var actual).Should().BeTrue();
            actual.Should().Be((byte)0);
        }

        [Fact]
        public void Test2()
        {
            Span<byte> span = new byte[] {0,1,2,3,4,5,6,7,8,9};
            var reader = new SpanReader<byte>(span);

            for (byte expected = 0; expected < 10; expected++)
            {
                reader.TryPeek(out var actual).Should().BeTrue();
                actual.Should().Be(expected);
                reader.Advance(1);
            }
            
        }

        [Fact]
        public void Test3()
        {
            Span<byte> span = new byte[] {0,1,2,3,4,5,6,7,8,9};
            var reader = new SpanReader<byte>(span);

            for (byte expected = 0; expected < 10; expected++)
            {
                reader.TryRead(out var actual).Should().BeTrue();
                actual.Should().Be(expected);
            }
        }

        static ReadOnlySequence<byte> GetTestReadOnlySequence()
        {
            var seq = new Sequence<byte>();

            var mem1 = seq.GetMemory(4);
            var span1 = mem1.Span;
            span1[0] = 0;
            span1[1] = 1;
            span1[2] = 2;
            span1[3] = 3;
            seq.Advance(4);

            var mem2 = seq.GetMemory(6);
            var span2 = mem2.Span;
            span2[0] = 4;
            span2[1] = 5;
            span2[2] = 6;
            span2[3] = 7;
            span2[4] = 8;
            span2[5] = 9;
            seq.Advance(6);

            return seq.AsReadOnlySequence;
        }

        [Fact]
        public void Test4()
        {
            var ros = GetTestReadOnlySequence();
            var reader = new SpanReader<byte>(ros);

            for (byte expected = 0; expected < 10; expected++)
            {
                reader.TryRead(out var actual).Should().BeTrue();
                actual.Should().Be(expected);
            }
        }

        [Fact]
        public void Test5()
        {
            Span<byte> expected = new byte[] {0,1,2,3,4,5,6,7,8,9};

            var ros = GetTestReadOnlySequence();
            var reader = new SpanReader<byte>(ros);

            Span<byte> actual = new byte[10];
            reader.TryCopyTo(actual).Should().BeTrue();

            actual.SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public void Test6()
        {
            Span<byte> span = new byte[] {0,1,2,3,4,5,6,7,8,9};
            var reader = new SpanReader<byte>(span);

            Span<byte> expected = new byte[] {0,1,2,3,4,5,6,7,8,9};

            Span<byte> actual = new byte[10];
            reader.TryCopyTo(actual).Should().BeTrue();
            actual.SequenceEqual(expected).Should().BeTrue();
        }
    }
}
