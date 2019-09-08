using System;
using IO = System.IO;

namespace PSql
{
    internal class TemporaryFile : IDisposable
    {
        public TemporaryFile()
        {
            Path = IO.Path.GetTempFileName();
        }

        public string Path { get; }

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
            IO.File.Delete(Path);
        }
    }
}
