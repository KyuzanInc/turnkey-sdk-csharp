// Implementation-specific constants for the BouncyCastle-backed C# port of
// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
// they exist because BouncyCastle requires explicit curve / parameter inputs
// where the upstream uses @noble/curves which hides them.
//
// Upstream snapshot:
//   tests/UpstreamSources/turnkey-crypto-2.8.8/
//
// Mapping to upstream:
//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
//     checks in turnkey.ts.
//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
//     `crypto.ts uncompressRawPublicKey`:
//       p = BigInt("0xffff...ffffffff")   (P-256 prime)
//       b = BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")
//       a = p - BigInt(3)
//     Pulled out into named constants here so the BouncyCastle wiring stays
//     readable and the values can be checked against the pinned source.
//
// These constants are wire-irrelevant by themselves; what matters is that
// the BouncyCastle code paths that consume them produce the same bytes as
// the upstream @noble/curves paths. Wire-byte parity is verified by the
// HPKE / bundle-decrypt / signature-verify test fixtures.

namespace Turnkey
{
    /// <summary>
    /// Constants required by the BouncyCastle-backed C# port of
    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
    /// </summary>
    public static class CryptoConstants
    {
        /// <summary>
        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
        /// Used with <c>ECNamedCurveTable.GetByName</c>.
        /// </summary>
        public const string CURVE_NAME = "secp256r1";

        /// <summary>
        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
        /// Equivalent to <c>turnkey/crypto</c> hard-coded
        /// <c>compressedKey.length === 33</c> checks.
        /// </summary>
        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;

        /// <summary>
        /// NIST P-256 prime field modulus (p), decimal form.
        /// </summary>
        /// <remarks>
        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
        /// hex literal
        /// <c>0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff</c>.
        /// The decimal form is given here so BouncyCastle's
        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
        /// constructor parses it directly.
        /// </remarks>
        public const string P256_P =
            "115792089210356248762697446949407573530086143415290314195533631308867097853951";

        /// <summary>
        /// NIST P-256 curve coefficient b, hex form. See FIPS 186-4 Appendix D.
        /// Upstream inlines this as
        /// <c>BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")</c>.
        /// </summary>
        public const string P256_B =
            "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";

        /// <summary>
        /// Offset used to derive the P-256 coefficient a as <c>p - 3</c>.
        /// Upstream inlines <c>const a = p - BigInt(3)</c>.
        /// </summary>
        public const string P256_A_OFFSET = "3";
    }
}
