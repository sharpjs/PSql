/*
    Copyright 2020 Jeffrey Sharp

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

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PSql
{
    internal static class IoHelper
    {
        private const int
            BufferSize = 64 * 1024;
            // NOTE: Must keep under 85000 bytes to prevent object from being
            // placed in .NET's large object heap.

        internal static string ReadText(string path)
        {
            using (var stream = Open(path))
            {
                var encoding = DetectEncoding(stream);

                using (var reader = new StreamReader(stream, encoding, /*detect:*/ false))
                    return reader.ReadToEnd();
            }
        }

        private static Encoding DetectEncoding(FileStream stream)
        {
            Encoding encoding;

            // Use .NET's encoding detection algorithm
            using (var reader = CreateBomReader(stream))
            {
                reader.Peek();
                encoding = reader.CurrentEncoding;
            }

            // Undo whatever reads were performed to detect the encoding
            stream.Seek(0, SeekOrigin.Begin);

            // Use the detected encoding if it has the desired options already
            if (encoding.EncoderFallback is EncoderExceptionFallback &&
                encoding.DecoderFallback is DecoderExceptionFallback)
                return encoding;

            // Otherwise, ensure the encoding is writable so the options can be changed
            if (encoding.IsReadOnly)
                encoding = (Encoding) encoding.Clone();

            // Set desired options
            encoding.EncoderFallback = new EncoderExceptionFallback();
            encoding.DecoderFallback = new DecoderExceptionFallback();

            return encoding;
        }

        private static FileStream Open(string path)
        {
            return new FileStream(
                ToPlatformPath(path),
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.SequentialScan
            );
        }

        private static StreamReader CreateBomReader(Stream stream)
        {
            return new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 8,  // only used to read the BOM
                leaveOpen: true // another reader will be opened for this stream
            );
        }

        private static string ToPlatformPath(string path)
        {
            return Regex.Replace(
                path,
                @"[\\/]",
                Path.DirectorySeparatorChar.ToString(),
                RegexOptions.CultureInvariant
            );
        }
    }
}
