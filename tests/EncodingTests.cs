// EncodingTests.cs — xunit tests for src/Encoding.cs
//
// Vectors are derived from the upstream test file at:
//   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
//
// Plus additional edge cases for leading-zero handling, base58check
// roundtrip, and PointEncode round-trip.

using System;
using FluentAssertions;
using Xunit;

namespace Turnkey.Tests
{
    public class EncodingTests
    {
        // ========================================================
        // hex
        // ========================================================

        [Fact]
        public void Uint8ArrayToHexString_UpstreamVector()
        {
            // From index-test.ts test("uint8ArrayToHexString")
            byte[] input =
            {
                82, 52, 208, 143, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23,
                46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
            };
            const string expected =
                "5234d08ffa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";

            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
        }

        [Fact]
        public void Uint8ArrayToHexString_EmptyArray_ReturnsEmpty()
        {
            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
        }

        [Fact]
        public void Uint8ArrayToHexString_Null_Throws()
        {
            Action act = () => Encoding.Uint8ArrayToHexString(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Uint8ArrayToHexString_LeadingZeros_Preserved()
        {
            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
        }

        [Fact]
        public void Uint8ArrayFromHexString_UpstreamVector()
        {
            // From index-test.ts test("uint8ArrayFromHexString")
            const string hex =
                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
            byte[] expected =
            {
                82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23,
                46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
            };
            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
        }

        [Fact]
        public void Uint8ArrayFromHexString_ShortAscii()
        {
            // From index-test.ts: "627566666572" -> [98,117,102,102,101,114] (ASCII "buffer")
            byte[] expected = { 98, 117, 102, 102, 101, 114 };
            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
        }

        [Theory]
        [InlineData("")]    // empty
        [InlineData("123")] // odd length
        [InlineData("oops")] // bad chars
        public void Uint8ArrayFromHexString_Invalid_Throws(string hex)
        {
            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
            act.Should().Throw<ArgumentException>()
               .WithMessage("cannot create uint8array from invalid hex string*");
        }

        [Fact]
        public void Uint8ArrayFromHexString_WithLength_Pads()
        {
            // "01" with length 2 -> [0, 1]
            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
        }

        [Fact]
        public void Uint8ArrayFromHexString_WithoutLength_NoPadding()
        {
            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
        }

        [Fact]
        public void Uint8ArrayFromHexString_TooShortGetsPadded()
        {
            // From upstream "TOO SHORT" case
            const string hex =
                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc";
            byte[] expected =
            {
                0, 0, 82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138,
                40, 23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252,
            };
            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
        }

        [Fact]
        public void Uint8ArrayFromHexString_TooLong_Throws()
        {
            // From upstream "TOO LONG" case: 34 bytes into length=32 buffer
            const string hex =
                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fcfafbfcfd";
            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
            act.Should().Throw<ArgumentException>()
               .WithMessage("hex value cannot fit in a buffer of 32 byte(s)");
        }

        [Fact]
        public void Uint8ArrayFromHexString_OneByte_FitTwoByteRequest_Padding()
        {
            // Upstream: "0100" with length=1 throws "cannot fit in 1 byte(s)"
            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
            act.Should().Throw<ArgumentException>()
               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
        }

        [Fact]
        public void HexToAscii_Roundtrip()
        {
            const string asciiHex = "627566666572";
            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
        }

        [Fact]
        public void NormalizePadding_AddsLeadingZeros()
        {
            byte[] input = { 0x01, 0x02 };
            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
        }

        [Fact]
        public void NormalizePadding_StripsLeadingZeros()
        {
            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
        }

        [Fact]
        public void NormalizePadding_StripWouldRemoveNonZero_Throws()
        {
            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
            Action act = () => Encoding.NormalizePadding(input, 2);
            act.Should().Throw<ArgumentException>()
               .WithMessage("invalid number of starting zeroes*");
        }

        [Fact]
        public void NormalizePadding_EqualLength_Identity()
        {
            byte[] input = { 0x01, 0x02 };
            Encoding.NormalizePadding(input, 2).Should().Equal(input);
        }

        // ========================================================
        // base64url
        // ========================================================

        [Fact]
        public void StringToBase64UrlString_UpstreamHelloVector()
        {
            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
        }

        [Fact]
        public void StringToBase64UrlString_UpstreamPrivateKeyVector()
        {
            // From index-test.ts
            const string input =
                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
            const string expected =
                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
            Encoding.StringToBase64UrlString(input).Should().Be(expected);
        }

        [Fact]
        public void StringToBase64UrlString_UpstreamApiKeyStampVector()
        {
            const string input =
                "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\","
                + "\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\","
                + "\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}";
            const string expected =
                "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ";
            Encoding.StringToBase64UrlString(input).Should().Be(expected);
        }

        [Fact]
        public void StringToBase64UrlString_CodePointAbove255_Throws()
        {
            Action act = () => Encoding.StringToBase64UrlString("aĀb");
            act.Should().Throw<ArgumentException>()
               .WithMessage("InvalidCharacterError: found code point greater than 255:256 at position 1");
        }

        [Fact]
        public void Base64StringToBase64UrlEncodedString_UpstreamVectors()
        {
            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
                .Should().Be("aGVsbG8gd29ybGQ");
            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
                .Should().Be("U29tZSBzYW1wbGUgdGV4dA");
        }

        [Fact]
        public void HexStringToBase64Url_UpstreamVectors()
        {
            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
        }

        [Fact]
        public void HexStringToBase64Url_TooLong_Throws()
        {
            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
            act.Should().Throw<ArgumentException>()
               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
        }

        [Fact]
        public void Base64UrlToBase64_RestoresPadding()
        {
            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
        }

        [Fact]
        public void Base64UrlToBase64_NoChangeWhenAlreadyPadded()
        {
            // "AAAA" length 4, padding 0
            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
        }

        [Fact]
        public void DecodeBase64UrlToString_Roundtrip()
        {
            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
            // From upstream private-key encoding vector
            const string b64url =
                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1");
        }

        // ========================================================
        // PointEncode
        // ========================================================

        [Fact]
        public void PointEncode_EvenY_PrefixIs02()
        {
            var raw = new byte[65];
            raw[0] = 0x04;
            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
            raw[64] = 0xBE; // last byte even

            var compressed = Encoding.PointEncode(raw);

            compressed.Should().HaveCount(33);
            compressed[0].Should().Be(0x02);
            for (int i = 1; i < 33; i++) compressed[i].Should().Be(0xAA);
        }

        [Fact]
        public void PointEncode_OddY_PrefixIs03()
        {
            var raw = new byte[65];
            raw[0] = 0x04;
            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
            raw[64] = 0xBD; // last byte odd

            var compressed = Encoding.PointEncode(raw);

            compressed[0].Should().Be(0x03);
        }

        [Fact]
        public void PointEncode_InvalidPrefix_Throws()
        {
            var raw = new byte[65];
            raw[0] = 0x05;
            Action act = () => Encoding.PointEncode(raw);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid uncompressed P-256 key");
        }

        [Fact]
        public void PointEncode_WrongLength_Throws()
        {
            var raw = new byte[64];
            raw[0] = 0x04;
            Action act = () => Encoding.PointEncode(raw);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid uncompressed P-256 key");
        }

        // ========================================================
        // Base58 / Base58Check
        // ========================================================

        [Fact]
        public void Base58Encode_KnownVector()
        {
            // From "Hello World" -> hex "48656c6c6f20576f726c64" -> base58 "JxF12TrwUP45BMd"
            // (well-known Bitcoin reference vector)
            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
        }

        [Fact]
        public void Base58Decode_KnownVector()
        {
            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
        }

        [Fact]
        public void Base58_Roundtrip_PreservesLeadingZero()
        {
            byte[] data = { 0x00, 0x01, 0x02, 0x03 };
            string encoded = Encoding.Base58Encode(data);
            encoded.Should().StartWith("1"); // leading-zero convention
            byte[] decoded = Encoding.Base58Decode(encoded);
            decoded.Should().Equal(data);
        }

        [Fact]
        public void Base58_Empty()
        {
            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
        }

        [Fact]
        public void Base58Check_Roundtrip()
        {
            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
            string encoded = Encoding.Base58CheckEncode(payload);
            byte[] decoded = Encoding.Base58CheckDecode(encoded);
            decoded.Should().Equal(payload);
        }

        [Fact]
        public void Base58CheckDecode_TamperedChecksum_Throws()
        {
            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
            string encoded = Encoding.Base58CheckEncode(payload);
            // Flip one character to corrupt the checksum
            char[] chars = encoded.ToCharArray();
            chars[chars.Length - 1] = chars[chars.Length - 1] == 'A' ? 'B' : 'A';
            string tampered = new string(chars);

            Action act = () => Encoding.Base58CheckDecode(tampered);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Base58CheckDecode_TooShort_Throws()
        {
            // Empty payload encoded would still produce 4-byte checksum + something,
            // but we just feed a 3-byte raw decoded result by abusing a known short string.
            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid Base58Check string - too short");
        }

        // ========================================================
        // Helpers
        // ========================================================

        [Fact]
        public void Uint8ArrayToString_DecodesUtf8()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
        }

        [Fact]
        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
        {
            byte[] a = { 1, 2 };
            byte[] b = { 3 };
            byte[] c = { 4, 5, 6 };
            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
        }
    }
}
