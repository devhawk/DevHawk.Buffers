using System;
using System.Buffers;

namespace DevHawk.Buffers
{
    public static class SpanReaderExtensions
    {
        internal static bool TryRead<T>(ref this SpanReader<byte> reader, out T value)
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public static bool TryRead(ref this SpanReader<byte> reader, out sbyte value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out short value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out ushort value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out int value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out uint value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out long value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out ulong value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out float value)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadBigEndian(ref this SpanReader<byte> reader, out double value)
        {
            throw new NotImplementedException();
        }
    }
}