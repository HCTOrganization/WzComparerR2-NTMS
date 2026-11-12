//"v410_260106_1_A1F3C9E2"
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static WzComparerR2.WzLib.Utilities.MathHelper;

namespace WzComparerR2.WzLib.Compatibility
{
    public static class WzVersionHasher
    {
        public static uint ComputePkg1HashVersion(int wzVersion)
        {
            uint sum = 0;
#if NET6_0_OR_GREATER
            Span<char> versionStr = stackalloc char[11];
            wzVersion.TryFormat(versionStr, out int charsWritten, provider: System.Globalization.CultureInfo.InvariantCulture);
            versionStr = versionStr.Slice(0, charsWritten);
#else
            string versionStr = wzVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);
#endif
            for (int j = 0; j < versionStr.Length; j++)
            {
                sum <<= 5;
                sum += (uint)versionStr[j] + 1;
            }
            
            return sum;
        }

        public static uint ComputePkg2HashVersion(string wzVersion)
        {
            ReadOnlySpan<byte> strBytes = MemoryMarshal.Cast<char, byte>(wzVersion.AsSpan());
            uint hash = 0x811C9DC5;
            foreach (var c in strBytes)
            {
                hash = (hash ^ c) * 0x1000193;
            }
            hash = 0x85EBCA6B * (hash ^ (hash >> 13));
            return hash ^ (hash >> 16);
        }

        public static ulong ComputePkg2HashVersion1202(string wzVersion)
        {
            ulong hash = 0x6D4C3B2A91E10DA5UL;
            for (int i = 0; i < wzVersion.Length; i++)
            {
                ushort c = wzVersion[i];
                if (i < 4)
                {
                    hash ^= (ulong)c << (i * 2);
                }
                else 
                {
                    hash = (i % 2 != 0) ? ROL(hash, c & 0x3F) : ROR(hash, c & 0x3F);
                }
            }
            return hash;
        }

        public static ulong ComputePkg2HashVersion1206(string wzVersion)
        {
            byte[] hash2 = ComputePkg2HashVersionBlocks1206(wzVersion);
            return BitConverter.ToUInt64(hash2, 0);
        }

        public static ulong ComputePkg2HashVersion1208(string wzVersion)
        {
            byte[] hash2 = ComputePkg2HashVersionBlocks1208(wzVersion);
            ulong lo = BitConverter.ToUInt64(hash2, 0);
            ulong hi = BitConverter.ToUInt64(hash2, 8);
            return lo ^ ROL(hi, 29);
        }

        private static byte[] ComputePkg2HashVersionBlocks1206(string wzVersion)
        {
            byte[] strBytes = Encoding.Unicode.GetBytes(wzVersion);
            byte[] key1 =
            {
                0xDE, 0x17, 0x00, 0xB9, 0x5C, 0x7C, 0xF2, 0xFE,
                0x10, 0x11, 0x5B, 0x6E, 0xE3, 0xE0, 0xE9, 0x84,
                0xA8, 0x35, 0x3C, 0xF4, 0x21, 0x87, 0xD8, 0x52,
                0x52, 0xCA, 0x5E, 0x0E, 0x71, 0x3C, 0x25, 0x87,
            };
            byte[] key2 =
            {
                0xEC, 0xEB, 0x71, 0xCE, 0x04, 0xCD, 0x3B, 0xB3,
                0xEE, 0xAD, 0x10, 0x78, 0x0E, 0x2F, 0x58, 0x0A,
                0x01,
            };
            return ComputeDoubleHmac(strBytes, key1, key2);
        }

        private static byte[] ComputePkg2HashVersionBlocks1208(string wzVersion)
        {
            byte[] strBytes = Encoding.Unicode.GetBytes(wzVersion);
            byte[] key1 =
            {
                0x34, 0xD0, 0xF4, 0x29, 0x59, 0x3B, 0xE8, 0xAF,
                0xA4, 0x96, 0x9F, 0x14, 0xCF, 0x4E, 0x66, 0xF0,
                0x25, 0x76, 0x6D, 0x24, 0xE0, 0x2C, 0x54, 0x2F,
                0xCE, 0x40, 0x0E, 0xDC, 0x18, 0xB3, 0xB3, 0x55,
            };
            byte[] key2 =
            {
                0x95, 0xB3, 0xDD, 0x88, 0x4A, 0xE3, 0xE4, 0x7A,
                0x3B, 0xD6, 0x4A, 0xE5, 0xB0, 0x38, 0xB4, 0x97,
                0x01,
            };
            return ComputeDoubleHmac(strBytes, key1, key2);
        }

        private static byte[] ComputeDoubleHmac(byte[] message, byte[] key1, byte[] key2)
        {
            byte[] ComputeHmacSha256(byte[] key, byte[] data)
            {
                using (var hmac = new HMACSHA256(key))
                {
                    return hmac.ComputeHash(data);
                }
            }

            byte[] hash1 = ComputeHmacSha256(key1, message);
            return ComputeHmacSha256(hash1, key2);
        }
    }
}
