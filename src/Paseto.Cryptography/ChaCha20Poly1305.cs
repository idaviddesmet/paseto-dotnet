﻿namespace Paseto.Cryptography
{
    using Internal;
    using Internal.ChaCha;

    /// <summary>
    /// ChaCha20-Poly1305 AEAD construction, compatible with <a href="https://tools.ietf.org/html/rfc7539#section-2.8">RFC 7539, section 2.8</a>.
    /// </summary>
    /// <seealso cref="Paseto.Cryptography.Internal.SnufflePoly1305" />
    public class ChaCha20Poly1305 : SnufflePoly1305
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChaCha20Poly1305"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public ChaCha20Poly1305(byte[] key) : base(key) { }

        /// <summary>
        /// Creates the snuffle instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="initialCounter">The initial counter.</param>
        /// <returns>Snuffle.</returns>
        protected override Snuffle CreateSnuffleInstance(byte[] key, int initialCounter) => new ChaCha20(key, initialCounter);
    }
}