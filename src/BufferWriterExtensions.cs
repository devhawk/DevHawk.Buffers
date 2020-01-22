// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DevHawk.Buffers
{
    public static class BufferWriterExtensions
    {
        private unsafe static void Write<T>(ref this BufferWriter<byte> writer, T value, bool reverse)
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

        public static void Write(ref this BufferWriter<byte> writer, byte value)
        {
            writer.Ensure(1);
            writer.Span[0] = value;
            writer.Advance(1);
        }

        public static void Write(ref this BufferWriter<byte> writer, sbyte value)
        {
            Write(ref writer, unchecked((byte)value));
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, short value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, short value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, ushort value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, ushort value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, int value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, int value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, uint value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, uint value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, long value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, long value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

        public static void WriteLittleEndian(ref this BufferWriter<byte> writer, ulong value)
        {
            Write(ref writer, value, !BitConverter.IsLittleEndian);
        }

        public static void WriteBigEndian(ref this BufferWriter<byte> writer, ulong value)
        {
            Write(ref writer, value, BitConverter.IsLittleEndian);
        }

    }
}