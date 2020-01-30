// Copyright (c) Harry Pierson. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace DevHawk.Buffers
{
    public static class SpanWriterExtensions
    {
        private unsafe static void Write<T>(ref this SpanWriter<byte> writer, T value, bool reverse)
            where T : unmanaged
        {
            var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            var byteSpan = MemoryMarshal.AsBytes(valueSpan);

            if (reverse)
            {
                var array = ArrayPool<byte>.Shared.Rent(sizeof(T));
                var span = array.AsSpan().Slice(0, sizeof(T));
                byteSpan.CopyTo(span);
                span.Reverse();
                writer.Write(span);
                ArrayPool<byte>.Shared.Return(array);
            }
            else
            {
                writer.Write(byteSpan);
            }
        }

        public static void Write(ref this SpanWriter<byte> writer, byte value)
        {
            if (writer.Span.IsEmpty) throw new InvalidOperationException();
            writer.Span[0] = value;
            writer.Advance(1);
        }

        public static void Write(ref this SpanWriter<byte> writer, sbyte value)
        {
            Write(ref writer, unchecked((byte)value));
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, short value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, short value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, ushort value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, ushort value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, int value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, int value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, uint value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, uint value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, long value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, long value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this SpanWriter<byte> writer, ulong value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this SpanWriter<byte> writer, ulong value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }
    }
}