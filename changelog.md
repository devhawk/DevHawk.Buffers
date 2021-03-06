# DevHawk.Buffers Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

This project uses [NerdBank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning)
to manage version numbers. This tool automatically sets the Semantic Versioning Patch
value based on the [Git height](https://github.com/AArnott/Nerdbank.GitVersioning#what-is-git-height)
of the commit that generated the build. As such, released versions of this extension
will not have contiguous patch numbers. Initial major and minor releases will be documented
in this file without a patch number. Patch version will be included for bug fix releases, but
may not exactly match a publicly released version.

## [1.1] - Unreleased

### Added

- `MemoryPoolBufferWriter<T>`, an
  [`IBufferWriter<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.ibufferwriter-1)
  implementation using buffers allocated from [`MemoryPool<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1).

## [1.0] - 2020-01-25

Initial Public Release
