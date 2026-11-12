using static WzComparerR2.WzLib.Utilities.MathHelper;

namespace WzComparerR2.WzLib.Compatibility
{
    /// <summary>
    /// Cryptographic primitives recovered from the KMST1208 client (NameSpace.dll).
    /// All positions are relative to the end of the file header (i.e. data-relative).
    /// </summary>
    internal static class Pkg2Kmst1208Hash
    {
        /// <summary>
        /// Version string baked into the KMST1208 client. Used to derive the version hash.
        /// </summary>
        public const string VersionString = "v410_260106_1_A1F3C9E2";

        /// <summary>
        /// C(hashVersion, hash1). The value is derived once per file and reused by the
        /// directory count/field/name/offset keys.
        /// </summary>
        public static ulong ComputeC(ulong hashVersion, ulong hash1)
        {
            ulong v2 = hash1 - 0x716D61B446224A6DUL;
            ulong v3 = ROR(v2, 27);
            ulong v4 = ROR(v2, 44);
            ulong v5 = ROL(hashVersion ^ 0x1E1F8E71B938A7E1UL, 28);
            ulong v6 = 0x348E9CFE0D833B4BUL * (v2 ^ v3 ^ v4);
            v6 ^= v6 >> 28;

            ulong t1 = 0x03D871A903B6DBB7UL * (v5 ^ (v5 >> 34));
            ulong v7 = 0xA5521FD3FCB4057BUL * (t1 ^ (t1 >> 32));
            ulong v8 = v7 ^ (v7 >> 31);

            ulong v9 = v6 ^ v8;
            ulong v10 = ROR(v6, 19);
            ulong t2 = v9 ^ 0x35EEB10256AA6BD7UL;
            ulong v11 = ROR(0x5C8E961F7B7BA523UL * (t2 ^ (t2 >> 31)), 27);
            ulong v12 = 0x8B5ADDDCC28F4A1FUL * (v11 ^ (v11 >> 31));

            ulong b = v8 ^ v12 ^ v10 ^ (v12 >> 30);
            ulong v13 = 0x71E6EFA7890B49D7UL * (b ^ (b >> 28));
            return v13 ^ (v13 >> 34);
        }

        /// <summary>
        /// D(value). Header validation relation: D(C(hashVersion, hash1)) == hash2.
        /// </summary>
        public static ulong ComputeD(ulong value)
        {
            ulong t = value ^ 0xC565BDCD03A98872UL;
            ulong v1 = 0xBF2F2B4EFECC7BE1UL * (t ^ ROL(t, 18) ^ ROL(t, 41));
            ulong v2 = ROL(value ^ 0x00207B38132A02ECUL, 28);

            ulong u = 0x03D871A903B6DBB7UL * (v2 ^ (v2 >> 34));
            ulong w = 0xA5521FD3FCB4057BUL * (u ^ (u >> 32));
            ulong x = w ^ (w >> 31);
            ulong y = 0x0D64EE103D36E75BUL * (v1 ^ (v1 >> 34));
            return y ^ ROL(x, 29) ^ (y >> 27);
        }

        /// <summary>
        /// Key used to decrypt a directory's entry count.
        /// </summary>
        public static ulong ComputeDirectoryCountKey(ulong hashVersion, ulong hash1)
        {
            ulong t0 = hash1 ^ 0xDADB824F046C2EFFUL;
            ulong v2 = 0xBF2F2B4EFECC7BE1UL * (t0 ^ ROL(t0, 18) ^ ROL(t0, 41));
            ulong v3 = ROL(hashVersion ^ 0x00207B38132A02ECUL, 28);

            ulong t1 = 0x03D871A903B6DBB7UL * (v3 ^ (v3 >> 34));
            ulong v4 = 0xA5521FD3FCB4057BUL * (t1 ^ (t1 >> 32));
            ulong m2 = 0x0D64EE103D36E75BUL * (v2 ^ (v2 >> 34));
            ulong v5 = m2 ^ v4 ^ ((m2 ^ (v4 >> 4)) >> 27);

            ulong s = (hash1 ^ hashVersion ^ 0x067F5742CA97D7F9UL) + 0x77979B713DBB074AUL;
            ulong v6 = 0x348E9CFE0D833B4BUL * (s ^ ROR(s, 27) ^ ROR(s, 44));
            ulong v7 = v6 ^ (v6 >> 28) ^ (v5 + ROR(v5, 28));
            return v7 ^ (v7 >> 28);
        }

        /// <summary>
        /// State used by the per-character directory entry name cipher.
        /// </summary>
        public static ulong ComputeDirEntryNameKey(ulong c, uint position)
        {
            ulong v2 = 0xE4150772966F4E81UL * position;
            v2 ^= ROR(v2, 25);
            ulong v3 = v2 + c;
            ulong v4 = c ^ ROL(v2, 32);

            ulong q = v3 ^ 0x35EEB10256AA6BD7UL ^ ((v3 ^ 0x35EEB10256AA6BD7UL) >> 31);
            ulong v5 = ROR(0x5C8E961F7B7BA523UL * q, 27);
            ulong v6 = 0x8B5ADDDCC28F4A1FUL * (v5 ^ (v5 >> 31));

            v4 ^= 0xDADB824F046C2EFFUL;
            ulong t = v4 ^ ROL(v4, 18) ^ ROL(v4, 41);
            ulong u = 0xBF2F2B4EFECC7BE1UL * t;
            ulong v9 = 0x0D64EE103D36E75BUL * (u ^ (u >> 34));

            ulong r = 0x56E95F804A4CD699UL * (v6 ^ v9 ^ ((v9 ^ (v6 >> 3)) >> 27));
            return r ^ (r >> 31);
        }

        /// <summary>
        /// Key used to decrypt per-entry fields (checksum, size) and the node type.
        /// </summary>
        public static uint ComputeEntryFieldKey(ulong hashVersion, ulong hash1, ulong c, int position)
        {
            uint v4 = (uint)((c >> 7) ^ (c >> 32));
            uint p = unchecked((uint)(294617447 * position));
            uint v5 = ROR(unchecked(v4 + (uint)(hash1 ^ (hashVersion >> 32)) + (p ^ (p >> 12))),
                (int)(((uint)(hash1 >> 32) ^ (v4 >> 5)) & 0x1F));
            uint acc = unchecked(277150939 * v4
                + (unchecked(1021519063 * v5) ^ (unchecked(1021519063 * v5) >> 13)));
            return acc ^ ROL(acc, 16);
        }

        /// <summary>
        /// Decrypts a directory entry node type (0x03 = directory, 0x04 = image).
        /// </summary>
        public static byte DecryptNodeType(ulong hashVersion, ulong hash1, ulong c, int position, byte encryptedNodeType)
        {
            uint key = ComputeEntryFieldKey(hashVersion, hash1, c, position);
            return (byte)(encryptedNodeType ^ ((key >> 11) & 0xFF));
        }

        /// <summary>
        /// Decrypts an image/directory offset (pointing at an absolute file position).
        /// </summary>
        public static uint ComputeImageOffset(uint headerLen, ulong hashVersion, ulong hash1, ulong c,
            int position, uint encryptedOffset)
        {
            uint v14 = ROR(encryptedOffset, 11);
            uint r = unchecked(1442009253u * ROL(
                unchecked((uint)(c >> 17) + (uint)(hashVersion >> 16))
                    ^ unchecked((uint)(-1918831151 * (uint)position)),
                (int)(((uint)(c >> 17) ^ (uint)((hash1 >> 40) & 0xFF)) & 0x1F)));
            return unchecked(headerLen + (v14 ^ r ^ (r >> 15) ^ (uint)(1030949161UL * (uint)hash1)));
        }
    }
}
