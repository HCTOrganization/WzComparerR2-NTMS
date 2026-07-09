using System;
using WzComparerR2.WzLib.Utilities;
using static WzComparerR2.WzLib.Utilities.MathHelper;
using static WzComparerR2.WzLib.Wz_Header;

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
    /// Factory delegate for creating offset calculators during version detection.
    /// </summary>
    public delegate IWzImageOffsetCalc OffsetCalcFactory(Wz_File wzFile, ulong hashVersion);

    /// <summary>
    /// Extended offset calculator for PKG2, also handles entry count decryption.
    /// </summary>
    public interface IPkg2ImageOffsetCalc : IWzImageOffsetCalc
    {
        int DecryptEntryCount(long encryptedEntryCount);
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
    public sealed class Pkg2OffsetCalcV1 : IPkg2ImageOffsetCalc
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

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 24) + (0x7F4A7C15 * this.hashVersion)));
        }
    }

    /// <summary>
    /// PKG2 offset calculation for KMST 1198 (V2).
    /// </summary>
    public sealed class Pkg2OffsetCalcV2 : IPkg2ImageOffsetCalc
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

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 16) - (0x21524111 * this.hashVersion)));
        }
    }

    /// <summary>
    /// PKG2 offset calculation for KMST 1199 (V3).
    /// </summary>
    public sealed class Pkg2OffsetCalcV3 : IPkg2ImageOffsetCalc
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

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            return (int)(encryptedEntryCount ^ ((this.hash1 << 16) + (this.mixedHash & 0x7fffffff) - (0x21524111 * this.hashVersion)));
        }
    }
    public sealed class Pkg2OffsetCalcV4 : IPkg2ImageOffsetCalc
    {
        public Pkg2OffsetCalcV4(uint headerLen, ulong hashVersion, ulong hash1)
        {
            this.headerLen = headerLen;
            this.hashVersion = hashVersion;
            this.hash1 = hash1;
        }

        private readonly uint headerLen;
        private readonly ulong hashVersion;
        private readonly ulong hash1;

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            hashedOffset = ~hashedOffset;
            var hash1_32 = (uint)hash1;
            var hashver_32 = (uint)hashVersion;


            var v10 = (uint)((0x1010101 * hash1_32)
              ^ (0x9E3779B9 * (hashver_32 ^ hash1_32 ^ 0x33BBBB33))
              ^ (((hashver_32 ^ hash1_32) + (hashver_32 ^ hash1_32 ^ 0x94587BA0))
               * ~(filePos - headerLen)
               - 0x581C3F6D));

            var a5 = headerLen + (hashedOffset ^ ROL(v10,19));

            return a5;
        }

        public int DecryptEntryCount(long encryptedEntryCount)
        {
            return (int)(((ulong)encryptedEntryCount ^ hash1 ^ hashVersion ^ 0x550EC4DD02C468EC) >> 16);
        }
    }
}

