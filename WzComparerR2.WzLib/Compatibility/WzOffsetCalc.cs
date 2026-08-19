using System;
using WzComparerR2.WzLib.Utilities;
using static WzComparerR2.WzLib.Utilities.MathHelper;

namespace WzComparerR2.WzLib.Compatibility
{
    /// <summary>
    /// Calculates the offset of a wz image/directory entry from its hashed value.
    /// </summary>
    public interface IWzImageOffsetCalc
    {
        uint CalcOffset(uint filePos, uint hashedOffset);
    }

    /// <summary>
    /// Extended offset calculator for PKG2.
    /// </summary>
    public interface IPkg2ImageOffsetCalc : IWzImageOffsetCalc
    {
    }

    /// <summary>
    /// Extended offset calculator for PKG2, also handles entry count decryption.
    /// </summary>
    public interface IPkg2ImageOffsetCalc<TEncryptedEntryCount> : IPkg2ImageOffsetCalc
    {
        int DecryptEntryCount(TEncryptedEntryCount encryptedEntryCount);
    }

    /// <summary>
    /// Optional PKG2 capability for formats that encrypt image length/checksum fields.
    /// </summary>
    public interface IPkg2ImageLengthCalc
    {
        int CalcLength(uint filePos, int encryptedValue);
    }

    internal static class Pkg2ImageOffsetCalcHelper
    {
        public static int DecryptEntryCount(IPkg2ImageOffsetCalc calc, long encryptedEntryCount)
        {
            if (calc is IPkg2ImageOffsetCalc<int> calc32)
                return calc32.DecryptEntryCount(checked((int)encryptedEntryCount));
            if (calc is IPkg2ImageOffsetCalc<long> calc64)
                return calc64.DecryptEntryCount(encryptedEntryCount);
            throw new NotSupportedException($"Unsupported PKG2 offset calculator type: {calc.GetType().FullName}");
        }
    }

    /// <summary>
    /// PKG2 offset calculation algorithm version.
    /// </summary>
    public enum Pkg2OffsetVersion
    {
        /// <summary>KMST 1196-1197</summary>
        KMST1196 = 1,
        /// <summary>KMST 1198</summary>
        KMST1198 = 2,
        /// <summary>KMST 1199</summary>
        KMST1199 = 3,
        /// <summary>KMST 1202</summary>
        KMST1202 = 4,
        /// <summary>KMST 1205</summary>
        KMST1205 = 5,
    }

    /// <summary>
    /// PKG1 offset calculation (original format).
    /// </summary>
    public sealed class Pkg1OffsetCalc : IWzImageOffsetCalc
    {
        public Pkg1OffsetCalc(uint headerLen, uint hashVersion)
        {
            this.headerLen = headerLen;
            this.hashVersion = hashVersion;
        }

        private readonly uint headerLen;
        private readonly uint hashVersion;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = filePos - this.headerLen;
            offset = ~offset;
            offset *= this.hashVersion;
            offset -= 0x581C3F6D;
            int distance = (int)offset & 0x1F;
            offset = ROL(offset, distance);
            offset ^= hashedOffset;
            offset += this.headerLen * 2;
            return offset;
        }
    }

    /// <summary>
    /// PKG2 offset calculation for KMST 1196-1197 (V1).
    /// </summary>
    public sealed class Pkg2OffsetCalcV1 : IPkg2ImageOffsetCalc<int>
    {
        public Pkg2OffsetCalcV1(uint headerLen, uint hashVersion, uint hash1)
        {
            this.headerLen = headerLen;
            this.hashVersion = hashVersion;
            this.hash1 = hash1;
        }

        private readonly uint headerLen;
        private readonly uint hashVersion;
        private readonly uint hash1;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = filePos - this.headerLen;
            offset = ~offset;
            offset *= this.hashVersion;
            offset -= 0x581C3F6D;
            offset ^= this.hash1 * 0x01010101;
            int distance = (byte)((this.hashVersion ^ this.hash1) & 0x1F);
            offset = ROL(offset, distance);
            offset ^= hashedOffset;
            offset += this.headerLen;
            return offset;
        }

        public int DecryptEntryCount(int encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 24) + (0x7F4A7C15 * this.hashVersion)));
        }
    }

    /// <summary>
    /// PKG2 offset calculation for KMST 1198 (V2).
    /// </summary>
    public sealed class Pkg2OffsetCalcV2 : IPkg2ImageOffsetCalc<int>
    {
        public Pkg2OffsetCalcV2(uint headerLen, uint hashVersion, uint hash1)
        {
            this.headerLen = headerLen;
            this.hashVersion = hashVersion;
            this.hash1 = hash1;
        }

        private readonly uint headerLen;
        private readonly uint hashVersion;
        private readonly uint hash1;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = filePos - this.headerLen;
            offset = ~offset;
            offset *= this.hashVersion ^ this.hash1;
            offset -= 0x581C3F6D;
            offset ^= this.hash1 * 0x01010101;
            int distance = (byte)((this.hashVersion ^ this.hash1) & 0x1F);
            offset = ROL(offset, distance);
            offset ^= ~hashedOffset;
            offset += this.headerLen;
            return offset;
        }

        public int DecryptEntryCount(int encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 16) - (0x21524111 * this.hashVersion)));
        }
    }

    /// <summary>
    /// PKG2 offset calculation for KMST 1199 (V3).
    /// </summary>
    public sealed class Pkg2OffsetCalcV3 : IPkg2ImageOffsetCalc<int>
    {
        public Pkg2OffsetCalcV3(uint headerLen, uint hashVersion, uint hash1)
        {
            this.headerLen = headerLen;
            this.hashVersion = hashVersion;
            this.hash1 = hash1;

            uint preHash = hash1 ^ hashVersion;
            this.preHash = preHash;
            this.mixedHash = Mix(preHash ^ 0x6D4C3B2A) ^ 0x91E10DA5;
        }

        private readonly uint headerLen;
        private readonly uint hashVersion;
        private readonly uint hash1;
        private readonly uint preHash;
        private readonly uint mixedHash;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = filePos - this.headerLen;
            offset = ~offset;
            offset *= this.preHash + (this.mixedHash ^ 0xA7E3C093);
            offset -= 0x581C3F6D;
            offset ^= this.hash1 * 0x01010101;
            offset ^= this.mixedHash * 0x9E3779B9;
            int distance = (byte)((this.preHash ^ this.mixedHash) & 0x1F);
            offset = ROL(offset, distance);
            offset ^= ~hashedOffset;
            offset += this.headerLen;
            return offset;
        }

        public int DecryptEntryCount(int encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 16) + (this.mixedHash & 0x7fffffff) - (0x21524111 * this.hashVersion)));
        }
    }

    /// <summary>
    /// 64-bit PKG2 offset calculation for KMST 1202.
    /// </summary>
    public sealed class Pkg2OffsetCalc64V1 : IPkg2ImageOffsetCalc<long>, IPkg2ImageLengthCalc
    {
        public Pkg2OffsetCalc64V1(uint headerLen, ulong hash1, ulong hashVersion)
        {
            // client only use low 32bits.
            this.headerLen = headerLen;
            this.hash1 = hash1;
            this.hashVersionFull = hashVersion;
            this.preHash = (uint)hash1 ^ (uint)hashVersion;
            this.mixedHash = this.preHash ^ 0x33BBBB33;
        }

        private readonly uint headerLen;
        private readonly ulong hash1;
        private readonly ulong hashVersionFull;
        private readonly uint preHash;
        private readonly uint mixedHash;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = this.CalcSharedKey(filePos);
            offset ^= ~hashedOffset;
            offset += this.headerLen;
            return offset;
        }

        public int CalcLength(uint filePos, int encryptedValue)
        {
            return encryptedValue ^ unchecked((int)this.CalcSharedKey(filePos));
        }

        private uint CalcSharedKey(uint filePos)
        {
            uint key = filePos - this.headerLen;
            key = ~key;
            key *= (this.preHash + (this.mixedHash ^ 0xA7E3C093));
            key -= 0x581C3F6D;
            key ^= (uint)this.hash1 * 0x01010101;
            key ^= this.mixedHash * 0x9E3779B9;
            return ROL(key, 19);
        }

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            ulong dirCount = ((ulong)encryptedEntryCount ^ this.hash1 ^ this.hashVersionFull ^ 0x550EC4DD02C468ECUL) >> 16;
            if (dirCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(encryptedEntryCount), "64-bit PKG2 dir count exceeds supported range.");
            }
            return (int)dirCount;
        }
    }

    /// <summary>
    /// 64-bit PKG2 offset calculation for KMST 1205.
    /// </summary>
    public sealed class Pkg2OffsetCalc64V2 : IPkg2ImageOffsetCalc<long>, IPkg2ImageLengthCalc
    {
        public Pkg2OffsetCalc64V2(uint headerLen, ulong hash1, ulong hashVersion)
        {
            // client only use low 32bits.
            this.headerLen = headerLen;
            this.hash1 = hash1;
            this.hashVersionFull = hashVersion;
        }

        private readonly uint headerLen;
        private readonly ulong hash1;
        private readonly ulong hashVersionFull;

        private static ulong Sub1529FA450(ulong hash_ver, ulong hash1)
        {
            // v2 = __ROL8__(hash1 ^ 0x81B4A01224AAB10C, 31)
            ulong v2 = ROL8(hash1 ^ 0x81B4A01224AAB10CUL, 31);

            // MurmurHash3 fmix64 pass 1
            ulong t1 = 0xFF51AFD7ED558CCDUL * (v2 ^ (v2 >> 33));
            ulong v3 = 0xC4CEB9FE1A85EC53UL * (t1 ^ (t1 >> 29));
            v3 ^= v3 >> 32;

            // v4 = __ROR8__(0xBF58476D1CE4E5B9 * ((hash_ver - 0x2E4AB5CD2E6D12FD) ^ 0x84CAA73B2BB70682 ^ (... >> 30)), 27)
            ulong a = hash_ver - 0x2E4AB5CD2E6D12FDUL;
            ulong t2 = a ^ 0x84CAA73B2BB70682UL;
            ulong v4 = ROR8(0xBF58476D1CE4E5B9UL * (t2 ^ (t2 >> 30)), 27);

            // v5 = (0x94D049BB133111EB * (v4 ^ (v4 >> 27))) ^ ((...) >> 31)
            ulong v5x = 0x94D049BB133111EBUL * (v4 ^ (v4 >> 27));
            ulong v5 = v5x ^ (v5x >> 31);

            // v6 = v5 + 0x510E527FADE682D1;  v7 = __ROL8__(v5, 17)
            ulong v6 = v5 + 0x510E527FADE682D1UL;
            ulong v7 = ROL8(v5, 17);

            // v8 = 0x9FB21C651E98DF25 * ((v3+v6) ^ __ROR8__(v3+v6,25) ^ __ROR8__(v3+v6,47))
            ulong tmp = v3 + v6;
            ulong v8 = 0x9FB21C651E98DF25UL * (tmp ^ ROR8(tmp, 25) ^ ROR8(tmp, 47));

            // 最终 mix: 0x94D049BB133111EB * (v3 ^ v8 ^ v7 ^ (v8>>28) ^ (..>>29)), 再 ^ (..>>32)
            ulong f = v3 ^ v8 ^ v7 ^ (v8 >> 28);
            ulong fx = 0x94D049BB133111EBUL * (f ^ (f >> 29));
            return fx ^ (fx >> 32);
        }

        // sub_1529FA750(hash_ver, hash1, a3) → uint
        // 先调用 Sub1529FA450 混合，再做 MurmurHash3 32-bit finalizer
        public static uint Sub1529FA750(ulong hash_ver, ulong hash1, uint a3)
        {
            // v4 = (int)(uint)hash1 的低32位;  v5 = (int)(uint)hash_ver 的低32位
            uint v4 = (uint)hash1;
            uint v5 = (uint)hash_ver;

            // v6 = sub_1529FA450(hash_ver, hash1)
            ulong v6 = Sub1529FA450(hash_ver, hash1);

            // v7 = v6 ^ HIDWORD(v6)  → 压缩到 32 位
            uint v7 = (uint)v6 ^ (uint)(v6 >> 32);

            // inner = v7 + 0x2545F491 * (a3 ^ (a3 >> 15)) + (v5 ^ v4)
            uint inner = v7 + 0x2545F491u * (a3 ^ (a3 >> 15)) + (v5 ^ v4);

            // ROL4(inner, (v7 ^ v4) & 0x1F)
            uint rolled = ROL(inner, (int)((v7 ^ v4) & 0x1F));

            // LODWORD(v6) = 0x85EBCA77 * rolled
            uint v6lo = 0x85EBCA77u * rolled;

            // t = (v6lo ^ (v6lo >> 13)) - 0x3D4D51C3 * v7
            uint t = (v6lo ^ (v6lo >> 13)) - 0x3D4D51C3u * v7;

            // return t ^ ROL4(t, 16)
            return t ^ ROL(t, 16);
        }

        private uint CalcSharedKey(uint filePos)
        {
            uint key = filePos - this.headerLen;
            return Sub1529FA750(hashVersionFull, hash1, key);
        }

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint v9 = (uint)Sub1529FA450(hashVersionFull, hash1);
            uint lo_h1 = (uint)hash1;
            uint lo_hv = (uint)hashVersionFull;

            // key（与正向完全相同）
            uint part1 = 0xC2B2AE3Du * v9;
            uint part2 = 0x85EBCA77u * lo_h1;
            uint part3 = ((lo_h1 ^ lo_hv) + (v9 ^ 0x2545F491u)) * ~((uint)(filePos - (uint)headerLen));
            uint key = part1 ^ part2 ^ (part3 + 0x2545F491u);

            uint rotAmt = (v9 ^ lo_h1 ^ lo_hv) & 0x1F;

            // 逆向：v11 = ~enc_val;  val = (v11 ^ ROL4(key, rotAmt)) + header_size
            uint v11 = ~hashedOffset;
            return (uint)(v11 ^ ROL(key, (int)rotAmt)) + headerLen;
        }

        public int CalcLength(uint filePos, int encryptedValue)
        {
            return encryptedValue ^ unchecked((int)this.CalcSharedKey(filePos));
        }

        private static ulong Sub1529FA560(ulong hashVersion, ulong hash1)
        {
            // v3 = ROR8(0xBF58476D1CE4E5B9 * (hash1 ^ 0x84CAA73B2BB70682 ^ ((hash1 ^ 0x84CAA73B2BB70682) >> 30)), 27)
            ulong t1 = hash1 ^ 0x84CAA73B2BB70682UL;
            ulong v3 = ROR8(0xBF58476D1CE4E5B9UL * (t1 ^ (t1 >> 30)), 27);

            // v4 = 0x9FB21C651E98DF25 * ((v8 + 0x510E527FADE682D1) ^ ROR8(v8 + 0x510E527FADE682D1, 25) ^ ROR8(v8 + 0x510E527FADE682D1, 47))
            ulong sum1 = hashVersion + 0x510E527FADE682D1UL;
            ulong v4 = 0x9FB21C651E98DF25UL * (sum1 ^ ROR8(sum1, 25) ^ ROR8(sum1, 47));

            // tmp2 = 0x94D049BB133111EB * (v3 ^ (v3 >> 27))
            // v5 = v4 ^ ((v4 ^ (tmp2 >> 3)) >> 28)
            //    = v4 ^ (v4 >> 28) ^ (tmp2 >> 31)
            ulong tmp2 = 0x94D049BB133111EBUL * (v3 ^ (v3 >> 27));
            ulong v5 = v4 ^ ((v4 ^ (tmp2 >> 3)) >> 28);

            // v6 = 0x2545F4914F6CDD1D * ((v8 + hash1) ^ 0x6A09E667F3BCC908
            //     ^ ROL8((v8 + hash1) ^ 0x6A09E667F3BCC908, 23)
            //     ^ ROL8((v8 + hash1) ^ 0x6A09E667F3BCC908, 41))
            ulong t4 = (hashVersion + hash1) ^ 0x6A09E667F3BCC908UL;
            ulong v6 = 0x2545F4914F6CDD1DUL * (t4 ^ ROL8(t4, 23) ^ ROL8(t4, 41));

            // v7 = (0xBF58476D1CE4E5B9 * (v6 ^ HIDWORD(v6)))
            //    ^ ((0xBF58476D1CE4E5B9 * (v6 ^ HIDWORD(v6))) >> 29)
            //    ^ ((tmp2 ^ v5) + ROL8(tmp2 ^ v5, 23))
            ulong tmp3 = 0xBF58476D1CE4E5B9UL * (v6 ^ (uint)(v6 >> 32));
            ulong x = tmp2 ^ v5;  // 等价于 v17（Compute 中的）
            ulong v7 = (tmp3 ^ (tmp3 >> 29)) ^ (x + ROL8(x, 23));

            return v7 ^ (v7 >> 31);
        }

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            ulong dirCount = ((ulong)encryptedEntryCount ^ Sub1529FA560(hashVersionFull, hash1)) >> 16;
            if (dirCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(encryptedEntryCount), "64-bit PKG2 dir count exceeds supported range.");
            }
            return (int)dirCount;
        }
    }
}