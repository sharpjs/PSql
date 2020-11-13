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

namespace PSql
{
    /// <summary>
    ///   Modes for connection encryption and server identity verification.
    /// </summary>
    public enum EncryptionMode
    {
        /// <summary>
        ///   The default encrption mode.  Equivalent to <see cref="None"/> for
        ///   connections to the local machine, and <see cref="Full"/> for all
        ///   other connections.
        /// </summary>
        Default,

        /// <summary>
        ///   No connection encryption or server identity check.
        ///   Data sent over the connection is exposed to other network devices.
        ///   A malicious device could masquerade as a server.
        ///   This encryption mode is appropriate for same-machine connections only.
        /// </summary>
        None,

        /// <summary>
        ///   Connections are encrypted, but server identities are not verified.
        ///   A malicious device could masquerade as a server.
        ///   This encryption mode is appropriate only when the server uses a self-signed certificate.
        /// </summary>
        Unverified,

        /// <summary>
        ///   Connections are encrypted, and server identities are verified.
        ///   This is the most secure encryption mode.
        /// </summary>
        Full
    }
}
