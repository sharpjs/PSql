/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Text;
using IO = System.IO;

namespace PSql.Tests;

internal class TemporaryFile : IDisposable
{
    private static readonly Encoding Utf8
        = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes:             true
        );

    public TemporaryFile()
    {
        Path = IO.Path.GetTempFileName();
    }

    public string Path { get; }

    public bool IsDisposed { get; private set; }

    public void Write(string text)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(TemporaryFile));

        File.WriteAllText(Path, text, Utf8);
    }

    ~TemporaryFile()
    {
        Dispose(managed: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool managed)
    {
        if (IsDisposed)
            return;

        File.Delete(Path);
        IsDisposed = true;
    }
}
