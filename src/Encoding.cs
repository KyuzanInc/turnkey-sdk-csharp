// 1:1 logical port of @turnkey/encoding@0.6.0
//
// Upstream snapshot:
//   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
//
// Files covered:
//   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
//                                HexToAscii / NormalizePadding
//   ts-source/base64.ts       -> StringToBase64UrlString /
//                                HexStringToBase64Url /
//                                Base64StringToBase64UrlEncodedString /
//                                Base64UrlToBase64 /
//                                DecodeBase64UrlToString
//   ts-source/encode.ts       -> PointEncode
//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
//   ts-source/index.ts        -> DEFAULT_JWK_MEMBER_BYTE_LENGTH constant
//
// Adaptations:
//   Uint8Array          -> byte[]
//   String.fromCharCode(b) reduction -> direct byte buffer + Convert.ToBase64String
//   (the upstream "btoa(s)" implementation produces wire-identical bytes to
//    Convert.ToBase64String when the input is 0-255-bounded, which it always
//    is after hex -> Uint8Array conversion. The custom btoa only exists in
//    upstream to support React Native; it does not affect wire bytes.)
//   throw new Error(msg) -> throw new ArgumentException(msg)
//   regex match         -> System.Text.RegularExpressions.Regex
//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
//                           (same algorithm; produces the same bytes for valid
//                            input as the bs58 npm package)
//
// Unity-specific helpers kept for Crypto.cs consumption (also present in the
// peak Unity port):
//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
//                         array literals; this helper centralizes the pattern)

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Math;

namespace Turnkey
{
    /// <summary>
    /// Encoding utilities for the Turnkey API.
    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
    /// </summary>
    public static class Encoding
    {
        /// <summary>
        /// Internal constants exported by <c>@turnkey/encoding</c>.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// JWK member byte length used by the upstream <c>index.ts</c>.
            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
            /// </summary>
            public const int DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;

            /// <summary>
            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
            /// </summary>
            public const string BASE58_ALPHABET =
                "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            /// <summary>
            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
            /// </summary>
            internal const string BASE64_KEYSTR =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        }

        private static readonly Regex HexRegex = new Regex(
            "^[0-9A-Fa-f]+$",
            RegexOptions.Compiled);

        // ============================================================
        // hex.ts
        // ============================================================

        /// <summary>
        /// Converts a byte array into a lower-case hex string.
        /// Upstream: <c>hex.ts uint8ArrayToHexString</c>.
        /// </summary>
        public static string Uint8ArrayToHexString(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Upstream uses Array.prototype.reduce starting at "". For an empty
            // array the result is "". Match that explicitly.
            if (input.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(input.Length * 2);
            for (int i = 0; i < input.Length; i++)
            {
                sb.Append(input[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a byte array from a hex string.
        /// Upstream: <c>hex.ts uint8ArrayFromHexString</c>.
        /// </summary>
        /// <param name="hexString">Hex string. Must be even-length and contain only [0-9A-Fa-f].</param>
        /// <param name="length">Optional target length. When given, the result is left-padded with leading zero bytes or an exception is thrown if the value does not fit.</param>
        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
        {
            if (string.IsNullOrEmpty(hexString)
                || hexString.Length % 2 != 0
                || !HexRegex.IsMatch(hexString))
            {
                throw new ArgumentException(
                    "cannot create uint8array from invalid hex string: \"" + hexString + "\"");
            }

            var buffer = new byte[hexString.Length / 2];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            // Upstream uses `if (!length)`, which treats `0` as falsy/omitted.
            // Match that exactly so the wire-level behavior is identical.
            if (!length.HasValue || length.Value == 0)
            {
                return buffer;
            }

            if (hexString.Length / 2 > length.Value)
            {
                throw new ArgumentException(
                    "hex value cannot fit in a buffer of " + length.Value + " byte(s)");
            }

            var paddedBuffer = new byte[length.Value];
            // Left-pad: zeros at start, original bytes at the end.
            Array.Copy(buffer, 0, paddedBuffer, length.Value - buffer.Length, buffer.Length);
            return paddedBuffer;
        }

        /// <summary>
        /// Converts a hex string to an ASCII string.
        /// Upstream: <c>hex.ts hexToAscii</c>.
        /// </summary>
        /// <remarks>
        /// Upstream JS uses <c>parseInt(s.substr(i, 2), 16)</c> for each pair.
        /// JS <c>String.prototype.substr(i, 2)</c> can return 1 char on the final
        /// odd index, and <c>parseInt</c> can return <c>NaN</c> for unparseable
        /// pairs; <c>String.fromCharCode(NaN)</c> emits <c>U+0000</c>. This port
        /// preserves both behaviors so wire bytes match upstream for any input.
        /// </remarks>
        public static string HexToAscii(string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException(nameof(hexString));
            }

            var sb = new StringBuilder(hexString.Length / 2);
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // substr(i, 2): may yield 1 char on the trailing odd index.
                int take = Math.Min(2, hexString.Length - i);
                string pair = hexString.Substring(i, take);

                // JS parseInt(s, 16):
                //   - skips leading whitespace
                //   - parses as many leading [0-9A-Fa-f] characters as possible
                //   - returns NaN if no characters parse
                int parsed = JsParseIntBase16(pair);
                sb.Append((char)parsed);
            }
            return sb.ToString();
        }

        /// <summary>
        /// JS-compatible <c>parseInt(s, 16)</c>: parses the leading hex prefix of
        /// <paramref name="s"/>. Returns 0 for inputs that JS would map to <c>NaN</c>
        /// (no hex digits parseable) so that <c>String.fromCharCode(NaN)</c>
        /// behavior is matched (<c>NaN → U+0000</c>).
        /// </summary>
        private static int JsParseIntBase16(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }
            int value = 0;
            int parsedDigits = 0;
            foreach (char c in s)
            {
                int digit;
                if (c >= '0' && c <= '9') digit = c - '0';
                else if (c >= 'a' && c <= 'f') digit = 10 + (c - 'a');
                else if (c >= 'A' && c <= 'F') digit = 10 + (c - 'A');
                else break;
                value = value * 16 + digit;
                parsedDigits++;
            }
            // JS parseInt returns NaN when no digits parsed; String.fromCharCode(NaN) = 0
            return parsedDigits == 0 ? 0 : value;
        }

        /// <summary>
        /// Normalizes the padding of a byte array to a target length by either
        /// left-padding with zero bytes or stripping leading zero bytes.
        /// Throws if too many non-zero bytes would have to be removed.
        /// Upstream: <c>hex.ts normalizePadding</c>.
        /// </summary>
        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
        {
            if (byteArray == null)
            {
                throw new ArgumentNullException(nameof(byteArray));
            }

            int paddingLength = targetLength - byteArray.Length;

            // Add leading zeros
            if (paddingLength > 0)
            {
                var result = new byte[targetLength];
                Array.Copy(byteArray, 0, result, paddingLength, byteArray.Length);
                return result;
            }

            // Strip leading zeros
            if (paddingLength < 0)
            {
                int expectedZeroCount = -paddingLength;
                int zeroCount = 0;
                for (int i = 0; i < expectedZeroCount && i < byteArray.Length; i++)
                {
                    if (byteArray[i] == 0)
                    {
                        zeroCount++;
                    }
                }
                if (zeroCount != expectedZeroCount)
                {
                    throw new ArgumentException(
                        "invalid number of starting zeroes. Expected number of zeroes: "
                        + expectedZeroCount + ". Found: " + zeroCount + ".");
                }

                var result = new byte[targetLength];
                Array.Copy(byteArray, expectedZeroCount, result, 0, targetLength);
                return result;
            }

            return byteArray;
        }

        // ============================================================
        // base64.ts
        // ============================================================

        /// <summary>
        /// Converts a plain string into a base64url-encoded string.
        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
        /// </summary>
        /// <remarks>
        /// Upstream uses a pure-JS <c>btoa</c> for React Native compatibility and
        /// throws on code points greater than 0xFF. This port preserves that
        /// behavior by checking each code point against 0xFF before delegating to
        /// <c>System.Convert.ToBase64String</c>, which is wire-identical for the
        /// 0-255 byte range.
        /// </remarks>
        public static string StringToBase64UrlString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var bytes = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                int cp = input[i];
                if (cp > 0xFF)
                {
                    throw new ArgumentException(
                        "InvalidCharacterError: found code point greater than 255:"
                        + cp + " at position " + i);
                }
                bytes[i] = (byte)cp;
            }

            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
        }

        /// <summary>
        /// Converts a hex string into a base64url-encoded string.
        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
        /// </summary>
        public static string HexStringToBase64Url(string input, int? length = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Add a leading 0 if needed to get an even-length hex string.
            // padStart(Math.ceil(input.length / 2) * 2, "0")
            int targetHexLen = ((input.Length + 1) / 2) * 2;
            string hexString = input.Length < targetHexLen
                ? input.PadLeft(targetHexLen, '0')
                : input;

            var buffer = Uint8ArrayFromHexString(hexString, length);

            // Upstream then folds the buffer into a per-byte string via
            // String.fromCharCode and calls btoa on that. Convert.ToBase64String
            // on the raw bytes produces the same wire bytes (validated by tests).
            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(buffer));
        }

        /// <summary>
        /// Converts a standard base64 string into a base64url-encoded string by
        /// replacing <c>+</c> with <c>-</c>, <c>/</c> with <c>_</c>, and stripping <c>=</c>.
        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
        /// </summary>
        public static string Base64StringToBase64UrlEncodedString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            return input.Replace('+', '-').Replace('/', '_').Replace("=", string.Empty);
        }

        /// <summary>
        /// Converts a base64url string into a standard base64 string (with
        /// <c>=</c> padding restored).
        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
        /// </summary>
        public static string Base64UrlToBase64(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            string b64 = input.Replace('-', '+').Replace('_', '/');
            int padLen = (4 - (b64.Length % 4)) % 4;
            return b64 + new string('=', padLen);
        }

        /// <summary>
        /// Decodes a base64url-encoded string into a plain string by first
        /// restoring base64 padding and then base64-decoding into a byte buffer.
        /// Each output byte is then reinterpreted as a code point.
        /// Upstream: <c>base64.ts decodeBase64urlToString</c> + <c>atob</c>.
        /// </summary>
        /// <remarks>
        /// Upstream <c>atob</c> silently strips any character not in
        /// <c>[A-Za-z0-9+/=]</c> before decoding, and throws only when the
        /// remaining length mod 4 is 1. This port reproduces that lenient
        /// behavior so callers get the same string upstream JS would return.
        /// </remarks>
        public static string DecodeBase64UrlToString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            string b64 = Base64UrlToBase64(input);

            // Reproduce upstream atob: strip invalid characters first.
            var filtered = new StringBuilder(b64.Length);
            foreach (char c in b64)
            {
                if ((c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z')
                    || (c >= '0' && c <= '9')
                    || c == '+' || c == '/' || c == '=')
                {
                    filtered.Append(c);
                }
            }
            string sanitized = filtered.ToString();

            // Upstream atob explicitly throws when (length % 4) == 1.
            if (sanitized.Length % 4 == 1)
            {
                throw new ArgumentException(
                    "InvalidCharacterError: The string to be decoded is not correctly encoded.");
            }

            // Upstream atob then walks the string and shifts in 6-bit groups,
            // emitting full 8-bit bytes. Reproduce that bit-shifting decoder
            // explicitly because it is more permissive than Convert.FromBase64String
            // (it tolerates missing or partial padding the same way upstream does).
            const string keyStr =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            var sb = new StringBuilder();
            int buffer = 0;
            int bits = 0;
            for (int i = 0; i < sanitized.Length; i++)
            {
                char ch = sanitized[i];
                int index = keyStr.IndexOf(ch);
                if (index < 0 || index > 64)
                {
                    continue;
                }
                if (ch == '=')
                {
                    bits = 0;
                }
                else
                {
                    buffer = (buffer << 6) | index;
                    bits += 6;
                }
                while (bits >= 8)
                {
                    bits -= 8;
                    sb.Append((char)((buffer >> bits) & 0xff));
                }
            }
            return sb.ToString();
        }

        // ============================================================
        // encode.ts
        // ============================================================

        /// <summary>
        /// Compresses an uncompressed P-256 public key into its 33-byte compressed form.
        /// Upstream: <c>encode.ts pointEncode</c>.
        /// </summary>
        /// <param name="raw">65-byte uncompressed key starting with 0x04.</param>
        /// <returns>33-byte compressed key starting with 0x02 or 0x03.</returns>
        public static byte[] PointEncode(byte[] raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException(nameof(raw));
            }
            if (raw.Length != 65 || raw[0] != 0x04)
            {
                throw new ArgumentException("Invalid uncompressed P-256 key");
            }

            // x = raw[1..33], y = raw[33..65]; lengths are 32 and 32 by construction.
            byte yLastByte = raw[64];
            byte prefix = (yLastByte & 1) == 0 ? (byte)0x02 : (byte)0x03;

            var compressed = new byte[33];
            compressed[0] = prefix;
            Array.Copy(raw, 1, compressed, 1, 32);
            return compressed;
        }

        // ============================================================
        // bs58.ts (and bs58check.ts)
        //
        // Upstream imports the `bs58` and `bs58check` npm packages. This port
        // implements the same Bitcoin base58 algorithm in C# using BouncyCastle's
        // BigInteger. The algorithm is standard and produces wire-identical bytes
        // to the upstream packages for valid input.
        // ============================================================

        /// <summary>
        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
        /// Upstream: <c>bs58.ts bs58.encode</c>.
        /// </summary>
        public static string Base58Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length == 0)
            {
                return string.Empty;
            }

            var intData = new BigInteger(1, data);
            var sb = new StringBuilder();
            var fiftyEight = BigInteger.ValueOf(58);

            while (intData.CompareTo(BigInteger.Zero) > 0)
            {
                var divmod = intData.DivideAndRemainder(fiftyEight);
                intData = divmod[0];
                sb.Insert(0, Constants.BASE58_ALPHABET[divmod[1].IntValue]);
            }

            // Preserve leading zero bytes as '1' characters (Base58 convention).
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                sb.Insert(0, '1');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
        /// Upstream: <c>bs58.ts bs58.decode</c>.
        /// </summary>
        public static byte[] Base58Decode(string encoded)
        {
            if (encoded == null)
            {
                throw new ArgumentNullException(nameof(encoded));
            }
            if (encoded.Length == 0)
            {
                return new byte[0];
            }

            var decoded = BigInteger.Zero;
            var multi = BigInteger.One;
            var fiftyEight = BigInteger.ValueOf(58);

            for (int i = encoded.Length - 1; i >= 0; i--)
            {
                int digit = Constants.BASE58_ALPHABET.IndexOf(encoded[i]);
                if (digit < 0)
                {
                    throw new ArgumentException(
                        "Invalid character '" + encoded[i] + "' in base58 string");
                }
                decoded = decoded.Add(multi.Multiply(BigInteger.ValueOf(digit)));
                multi = multi.Multiply(fiftyEight);
            }

            byte[] bytes = decoded.Equals(BigInteger.Zero)
                ? new byte[0]
                : decoded.ToByteArrayUnsigned();

            int leadingZeros = 0;
            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
            {
                leadingZeros++;
            }
            if (leadingZeros > 0)
            {
                var result = new byte[leadingZeros + bytes.Length];
                Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);
                return result;
            }
            return bytes;
        }

        /// <summary>
        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
        /// </summary>
        public static string Base58CheckEncode(byte[] payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }
            using (var sha = SHA256.Create())
            {
                var h1 = sha.ComputeHash(payload);
                var h2 = sha.ComputeHash(h1);
                var checksum = new byte[4];
                Array.Copy(h2, 0, checksum, 0, 4);
                var concat = new byte[payload.Length + 4];
                Array.Copy(payload, 0, concat, 0, payload.Length);
                Array.Copy(checksum, 0, concat, payload.Length, 4);
                return Base58Encode(concat);
            }
        }

        /// <summary>
        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
        /// SHA256(SHA256(payload)) checksum.
        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
        /// </summary>
        public static byte[] Base58CheckDecode(string encoded)
        {
            byte[] decoded = Base58Decode(encoded);
            if (decoded.Length < 4)
            {
                throw new ArgumentException("Invalid Base58Check string - too short");
            }

            var data = new byte[decoded.Length - 4];
            Array.Copy(decoded, 0, data, 0, data.Length);
            var checksum = new byte[4];
            Array.Copy(decoded, decoded.Length - 4, checksum, 0, 4);

            using (var sha = SHA256.Create())
            {
                var h1 = sha.ComputeHash(data);
                var h2 = sha.ComputeHash(h1);
                if (h2[0] != checksum[0] || h2[1] != checksum[1]
                    || h2[2] != checksum[2] || h2[3] != checksum[3])
                {
                    throw new ArgumentException("Invalid Base58Check checksum");
                }
            }

            return data;
        }

        // ============================================================
        // Unity-port-derived helpers (NOT in @turnkey/encoding)
        //
        // The upstream library reaches for these patterns inline (TextDecoder,
        // spread). The C# port keeps them here as public helpers because
        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.
        // ============================================================

        /// <summary>
        /// UTF-8 decodes a byte array into a string.
        /// </summary>
        /// <remarks>
        /// Upstream uses <c>new TextDecoder().decode(bytes)</c> inline. This
        /// helper centralizes the equivalent in the .NET port. It is
        /// wire-irrelevant — it is invoked after wire bytes have already been
        /// decided.
        /// </remarks>
        public static string Uint8ArrayToString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Concatenates byte arrays.
        /// </summary>
        /// <remarks>
        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
        /// This helper centralizes the equivalent. It is wire-irrelevant — it
        /// just glues bytes that were already computed.
        /// </remarks>
        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
        {
            if (arrays == null)
            {
                throw new ArgumentNullException(nameof(arrays));
            }
            int total = arrays.Sum(a => a?.Length ?? 0);
            var result = new byte[total];
            int offset = 0;
            foreach (var a in arrays)
            {
                if (a != null && a.Length > 0)
                {
                    Array.Copy(a, 0, result, offset, a.Length);
                    offset += a.Length;
                }
            }
            return result;
        }
    }
}
