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
    public class BufferWriterTests
    {
        [Fact]
        public void span_advance_to_end()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new BufferWriter<byte>(owner.Memory.Span);

            writer.Advance(owner.Memory.Length);
            writer.Commit();

            Assert.True(writer.Span.IsEmpty);
        }

        [Fact]
        public void ibufwrite_advance_to_end()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);

            writer.Advance(buffer.Capacity);
            writer.Commit();

            Assert.False(writer.Span.IsEmpty);
        }

        
        private void test_write_advance(ref BufferWriter<byte> writer, int length)
        {
            var array = new byte[] { 0, 1, 2, 3 };
            writer.Write(array);

            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(length - 4, writer.Span.Length);
        }


        [Fact]
        public void span_write_advance()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new BufferWriter<byte>(owner.Memory.Span);

            test_write_advance(ref writer, owner.Memory.Length);
        }

        [Fact]
        public void ibw_write_advance()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);

            test_write_advance(ref writer, buffer.Capacity);
        }

        private void test_write_byte(ref BufferWriter<byte> writer, int length, byte value)
        {
            writer.Write(value);
            writer.Commit();
            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(length - 1, writer.Span.Length);
        }

        [Fact]
        public void span_write_byte()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new BufferWriter<byte>(owner.Memory.Span);
            byte expected = 0xd3;
            test_write_byte(ref writer, owner.Memory.Length, expected);

            Assert.Equal(expected, owner.Memory.Span[0]);
        }

        [Fact]
        public void ibw_write_byte()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);
            byte expected = 0xd3;
            test_write_byte(ref writer, buffer.Capacity, expected);

            Assert.Equal(expected, buffer.WrittenSpan[0]);
        }

        private void test_write_little_endian(ref BufferWriter<byte> writer, int length, uint value)
        {
            writer.WriteLittleEndian(value);
            writer.Commit();
            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(length - sizeof(uint), writer.Span.Length);
        }

        [Fact]
        public void span_write_little_endian()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new BufferWriter<byte>(owner.Memory.Span);
            uint expected = 0x12345678;
            test_write_little_endian(ref writer, owner.Memory.Length, expected);
            var actual = BinaryPrimitives.ReadUInt32LittleEndian(owner.Memory.Span);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ibw_write_little_endian()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);
            uint expected = 0x12345678;
            test_write_little_endian(ref writer, buffer.Capacity, expected);
            var actual = BinaryPrimitives.ReadUInt32LittleEndian(buffer.WrittenSpan);
            Assert.Equal(expected, actual);
        }

        private void test_write_big_endian(ref BufferWriter<byte> writer, int length, uint value)
        {
            writer.WriteBigEndian(value);
            writer.Commit();
            Assert.False(writer.Span.IsEmpty);
            Assert.Equal(length - sizeof(uint), writer.Span.Length);
        }

        [Fact]
        public void span_write_big_endian()
        {
            using var owner = MemoryPool<byte>.Shared.Rent();
            var writer = new BufferWriter<byte>(owner.Memory.Span);
            uint expected = 0x12345678;
            test_write_big_endian(ref writer, owner.Memory.Length, expected);
            var actual = BinaryPrimitives.ReadUInt32BigEndian(owner.Memory.Span);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ibw_write_big_endian()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);
            uint expected = 0x12345678;
            test_write_big_endian(ref writer, buffer.Capacity, expected);
            var actual = BinaryPrimitives.ReadUInt32BigEndian(buffer.WrittenSpan);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ibufwrite_advance_too_far_throws()
        {
            static void doTest()
            {
                var buffer = new ArrayBufferWriter<byte>();
                var writer = new BufferWriter<byte>(buffer);

                writer.Advance(buffer.FreeCapacity + 10);
            }

            Assert.Throws<InvalidOperationException>(() => doTest());
        }

        [Fact]
        public void span_advance_too_far_throws()
        {
            static void doTest()
            {
                using var owner = MemoryPool<byte>.Shared.Rent();
                var writer = new BufferWriter<byte>(owner.Memory.Span.Slice(0,10));
                writer.Advance(20);
            }

            Assert.Throws<InvalidOperationException>(() => doTest());
        }

        [Fact]
        public void span_write_too_much_throws()
        {
            static void doTest()
            {
                using var owner = MemoryPool<byte>.Shared.Rent();
                var writer = new BufferWriter<byte>(owner.Memory.Span.Slice(0,10));
                writer.WriteLittleEndian(0x12345678);
                writer.WriteLittleEndian(0x12345678);
                writer.WriteLittleEndian(0x12345678);
            }

            Assert.Throws<InvalidOperationException>(() => doTest());
        }

        [Fact]
        public void ibw_write_too_much_works()
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BufferWriter<byte>(buffer);

            var originalCapacity = buffer.Capacity;

            writer.Advance(buffer.FreeCapacity - 10);
            writer.WriteLittleEndian(0x12345678);
            writer.WriteLittleEndian(0x12345678);
            writer.WriteLittleEndian(0x12345678);

            Assert.True(originalCapacity < buffer.Capacity);
        }
    }
}
