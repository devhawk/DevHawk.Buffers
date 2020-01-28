[![Build Status](https://devhawk.visualstudio.com/Public/_apis/build/status/devhawk.DevHawk.Buffers?branchName=master)](https://devhawk.visualstudio.com/Public/_build/latest?definitionId=21&branchName=master)

# DevHawk.Buffers

This is a .NET Standard library for reading and writing data from types in the
[System.Buffers](https://docs.microsoft.com/en-us/dotnet/api/system.buffers)
namespace, with a focus on performance and minimal or zero heap allocations.

So far, it includes three types:

* BufferReader\<T> - Provides methods for reading binary and text data from a
  [ReadOnlySpan\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1)
  or a [ReadOnlySequence\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.readonlysequence-1).
  This type is conceptually similar to [SequenceReader\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.sequencereader-1),
  but abstracts away the underlying data source.
* BufferWriter\<T> - Provides high-performance methods for writing binary and text to an
  [IBufferWriter\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.ibufferwriter-1) instance.
  Heavily inspired by the BufferWriter\<T> implementation from the
  [MessagePack for C# project](https://github.com/neuecc/MessagePack-CSharp).
* MemoryPoolBufferWriter\<T> - an implementation of [IBufferWriter\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.ibufferwriter-1)
  that allocates buffers via [MemoryPool\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1)
  instead of plain .NET arrays.
