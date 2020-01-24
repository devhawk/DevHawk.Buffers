// Copyright (c) Harry Pierson. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Diagnostics;

namespace DevHawk.Buffers
{
    public sealed class MemoryPoolBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        private sealed class NullMemoryOwner : IMemoryOwner<T>
        {
            public static IMemoryOwner<T> Instance = new NullMemoryOwner();

            private NullMemoryOwner() {}

            public Memory<T> Memory => default;

            public void Dispose() { }
        }

        private IMemoryOwner<T> owner;
        private int index;

        private const int DefaultInitialBufferSize = 256;

        public MemoryPoolBufferWriter()
        {
            owner = NullMemoryOwner.Instance;
            index = 0;
        }

        public MemoryPoolBufferWriter(int initialCapacity)
        {
            if (initialCapacity <= 0)
                throw new ArgumentException(nameof(initialCapacity));

            owner = MemoryPool<T>.Shared.Rent(initialCapacity);
            index = 0;
        }

        public void Dispose()
        {
            owner.Dispose();
            owner = NullMemoryOwner.Instance;
        }

        public ReadOnlyMemory<T> WrittenMemory => owner.Memory.Slice(0, index);
        public ReadOnlySpan<T> WrittenSpan => owner.Memory.Span.Slice(0, index);
        public int WrittenCount => index;
        public int Capacity => owner.Memory.Length;
        public int FreeCapacity => owner.Memory.Length - index;

        public void Clear()
        {
            Debug.Assert(owner.Memory.Length >= index);
            owner.Memory.Span.Clear();
            index = 0;
        }

        public void Advance(int count)
        {
            if (count < 0) throw new ArgumentException(nameof(count));
            if (index > owner.Memory.Length - count) throw new InvalidOperationException("advanced to far");

            index += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(owner.Memory.Length > index);
            return owner.Memory.Slice(index);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(owner.Memory.Length > index);
            return owner.Memory.Span.Slice(index);
        }

        static void Resize(ref IMemoryOwner<T> owner, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException(nameof(newSize));

            var originalOwner = owner;
            if (originalOwner.Memory.IsEmpty)
            {
                owner = MemoryPool<T>.Shared.Rent(newSize);
                return;
            }

            if (originalOwner.Memory.Length != newSize)
            {
                var newOwner = MemoryPool<T>.Shared.Rent(newSize);
                originalOwner.Memory.CopyTo(newOwner.Memory);
                owner = newOwner;
                originalOwner.Dispose();
            }
        }

        void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint < 0) throw new ArgumentException(nameof(sizeHint));

            if (sizeHint == 0) 
            {
                sizeHint = 1;
            }

            if (sizeHint > owner.Memory.Length - index)
            {
                int growBy = Math.Max(sizeHint, owner.Memory.Length);

                if (owner.Memory.Length == 0)
                {
                    growBy = Math.Max(growBy, DefaultInitialBufferSize);
                }

                int newSize = checked(owner.Memory.Length + growBy);
                Resize(ref owner, newSize);
            }

            Debug.Assert(FreeCapacity > 0 && FreeCapacity >= sizeHint);
        }
    }
}