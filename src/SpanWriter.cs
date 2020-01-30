// Copyright (c) Harry Pierson. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;

namespace DevHawk.Buffers
{
    public ref struct SpanWriter<T>
    {
        public Span<T> Span { get; private set; }

        public SpanWriter(Span<T> span)
        {
            Span = span;
        }

        public void Advance(int count)
        {
            if (count > Span.Length) throw new InvalidOperationException();
            Span = Span.Slice(count);
        }

        public void Write(ReadOnlySpan<T> source)
        {
            if (Span.Length < source.Length) throw new InvalidOperationException();
            source.CopyTo(Span);
            Advance(source.Length);
        }
    }
}