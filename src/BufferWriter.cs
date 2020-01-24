// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;

namespace DevHawk.Buffers
{
    public ref struct BufferWriter<T> where T : unmanaged
    {
        public IBufferWriter<T> UnderlyingWriter { get; }
        public Span<T> Span { get; private set; }
        public int BytesCommitted { get; private set; }
        private int buffered;

        private static Memory<T> GetMemoryCheckResult(IBufferWriter<T> output, int size = 0)
        {
            var memory = output.GetMemory(size);
            if (memory.IsEmpty)
            {
                throw new InvalidOperationException("The underlying IBufferWriter<byte>.GetMemory(int) method returned an empty memory block, which is not allowed. This is a bug in " + output.GetType().FullName);
            }

            return memory;
        }

        public BufferWriter(IBufferWriter<T> output)
        {
            UnderlyingWriter = output;
            buffered = 0;
            BytesCommitted = 0;
            Span = GetMemoryCheckResult(output).Span;
        }

        public void Commit()
        {
            if (buffered > 0)
            {
                var temp = buffered;
                BytesCommitted += temp;
                buffered = 0;
                UnderlyingWriter.Advance(temp);
            }
        }

        public void Advance(int count)
        {
            buffered += count;
            Span = Span.Slice(count);
            Ensure();
        }

        public void Write(ReadOnlySpan<T> source)
        {
            if (Span.Length >= source.Length)
            {
                source.CopyTo(Span);
                Advance(source.Length);
            }
            else
            {
                WriteMultiBuffer(source);
            }
        }

        private void WriteMultiBuffer(ReadOnlySpan<T> source)
        {
            int copiedBytes = 0;
            int bytesLeftToCopy = source.Length;
            while (bytesLeftToCopy > 0)
            {
                if (Span.Length == 0)
                {
                    EnsureMore(bytesLeftToCopy);
                }

                var writable = Math.Min(bytesLeftToCopy, Span.Length);
                source.Slice(copiedBytes, writable).CopyTo(Span);
                copiedBytes += writable;
                bytesLeftToCopy -= writable;
                Advance(writable);
            }
        }

        public void Ensure(int count = 0)
        {
            if (Span.Length < count || Span.Length == 0)
            {
                EnsureMore(count);
            }
        }

        private void EnsureMore(int count = 0)
        {
            if (buffered > 0)
            {
                Commit();
            }

            Span = GetMemoryCheckResult(UnderlyingWriter, count).Span;
        }
    }
}