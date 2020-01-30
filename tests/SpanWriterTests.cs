// Copyright (c) Harry Pierson. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Buffers.Binary;
using DevHawk.Buffers;
using Xunit;

namespace DevHawk.BuffersTest
{
    public class SpanWriterTests
    {
        [Fact]
        public void write_advance_to_end()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,4));

            var array = new byte[] { 0, 1, 2, 3 };
            writer.Write(array);

            Assert.True(writer.Span.IsEmpty);
        }

        [Fact]
        public void write_advance()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));

            var array = new byte[] { 0, 1, 2, 3 };
            writer.Write(array);

            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(6, writer.Span.Length);
        }

        [Fact]
        public void write_byte()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));

            writer.Write((byte)0xde);

            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(9, writer.Span.Length);
            Assert.Equal(0xde, owner.Memory.Span[0]);
        }

        [Fact]
        public void write_little_endian()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));

            uint value = 0x12345678;

            writer.WriteLittleEndian(value);

            var actual = BinaryPrimitives.ReadUInt32LittleEndian(owner.Memory.Span);

            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(10 - sizeof(uint), writer.Span.Length);
            Assert.Equal(value, actual);
        }

        [Fact]
        public void write_big_endian()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));

            uint value = 0x12345678;

            writer.WriteBigEndian(value);

            var actual = BinaryPrimitives.ReadUInt32BigEndian(owner.Memory.Span);

            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(10 - sizeof(uint), writer.Span.Length);
            Assert.Equal(value, actual);
        }

        [Fact]
        public void advance_too_far_throws()
        {
            static void doTest()
            {
                using var owner = MemoryPool<byte>.Shared.Rent();
                var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));
                writer.Advance(20);
            }

            Assert.Throws<InvalidOperationException>(() => doTest());
        }

        [Fact]
        public void write_too_much_throws()
        {
            static void doTest()
            {
                using var owner = MemoryPool<byte>.Shared.Rent();
                var writer = new SpanWriter<byte>(owner.Memory.Span.Slice(0,10));
                writer.WriteLittleEndian(0x12345678);
                writer.WriteLittleEndian(0x12345678);
                writer.WriteLittleEndian(0x12345678);
            }

            Assert.Throws<InvalidOperationException>(() => doTest());
        }

        


    }
}
