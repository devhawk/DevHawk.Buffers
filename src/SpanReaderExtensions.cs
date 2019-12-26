// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevHawk.Buffers
{
    public static class SpanReaderExtensions
    {
        public static unsafe bool TryRead<T>(ref this SpanReader<byte> reader, out T value)
            where T : unmanaged
        {
            ReadOnlySpan<byte> span = reader.UnreadSpan;
            if (span.Length < sizeof(T))
            {
                return TryReadMultisegment(ref reader, out value);
            }

            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
            reader.Advance(sizeof(T));
            return true;
        }

        private static unsafe bool TryReadMultisegment<T>(ref SpanReader<byte> reader, out T value)
            where T : unmanaged
        {
            Debug.Assert(reader.UnreadSpan.Length < sizeof(T), "reader.UnreadSpan.Length < sizeof(T)");

            // Not enough data in the current segment, try to peek for the data we need.
            T buffer = default;
            Span<byte> tempSpan = new Span<byte>(&buffer, sizeof(T));

            if (!reader.TryCopyTo(tempSpan))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(tempSpan));
            reader.Advance(sizeof(T));
            return true;
        }

        public static bool TryRead(ref this SpanReader<byte> reader, out sbyte value)
        {
            if (TryRead(ref reader, out byte byteValue))
            {
                value = unchecked((sbyte)byteValue);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out short value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out ushort value)
        {
            if (TryReadBigEndian(ref reader, out short shortValue))
            {
                value = unchecked((ushort)shortValue);
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryReadReverseEndianness(ref SpanReader<byte> reader, out short value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out int value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out uint value)
        {
            if (TryReadBigEndian(ref reader, out int intValue))
            {
                value = unchecked((uint)intValue);
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryReadReverseEndianness(ref SpanReader<byte> reader, out int value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out long value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out ulong value)
        {
            if (TryReadBigEndian(ref reader, out long longValue))
            {
                value = unchecked((ulong)longValue);
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryReadReverseEndianness(ref SpanReader<byte> reader, out long value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        public static unsafe bool TryReadBigEndian(ref this SpanReader<byte> reader, out float value)
        {
            if (TryReadBigEndian(ref reader, out int intValue))
            {
                value = *(float*)&intValue;
                return true;
            }

            value = default;
            return false;
        }

        public static unsafe bool TryReadBigEndian(ref this SpanReader<byte> reader, out double value)
        {
            if (TryReadBigEndian(ref reader, out long longValue))
            {
                value = *(double*)&longValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}