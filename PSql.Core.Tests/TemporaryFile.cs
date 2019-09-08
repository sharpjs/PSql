using System;
using System.IO;
using System.Text;
using IO = System.IO;

namespace PSql
{
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
}
