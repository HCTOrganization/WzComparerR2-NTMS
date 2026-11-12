using System;
using System.Collections.Generic;
using System.IO;
using WzComparerR2.WzLib.Utilities;

namespace WzComparerR2.WzLib.Compatibility
{
    /// <summary>
    /// Pre-reads the fully-encrypted KMST1208 PKG2 directory tree.
    /// Unlike older formats, no raw scanning is possible here: every field is decrypted
    /// with the keys derived from the header hashes.
    /// </summary>
    internal sealed class Pkg2PreReader1208 : IWzPreReader
    {
        public const int HeaderSize = 383;
        private const int MaxEntryCount = 1 << 24;
        private const int MaxDepth = 64;

        public bool TryPreRead(Wz_File wzFile, out WzPreReadResult result)
        {
            result = null;
            if (wzFile.Header is not Wz_Header.WzPkg2Header64 header)
                return false;
            if (header.HeaderSize != HeaderSize)
                return false;

            ulong hashVersion = WzVersionHasher.ComputePkg2HashVersion1208(Pkg2Kmst1208Hash.VersionString);
            ulong hash1 = header.Hash1;
            ulong c = Pkg2Kmst1208Hash.ComputeC(hashVersion, hash1);
            if (Pkg2Kmst1208Hash.ComputeD(c) != header.Hash2)
                return false;

            try
            {
                wzFile.FileStream.Position = header.DirStartPosition;
                var reader = new WzBinaryReader(wzFile.FileStream, false);
                long dirStartPosition = wzFile.FileStream.Position;
                var state = new PreReadState
                {
                    Calc = new Pkg2OffsetCalc64V3((uint)header.HeaderSize, hash1, hashVersion),
                };

                var res = new WzPreReadResult(WzFileFormat.Pkg2Kmst1208,
                    new List<WzPreReadNodeInfo>(), dirStartPosition, 0)
                {
                    Pkg2DirEntryCounts = new List<Pkg2DirEntryCount>(),
                };

                ReadDirectory(reader, res, state, 0);
                res.DirEndPosition = reader.BaseStream.Position;
                result = res;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static void ReadDirectory(WzBinaryReader reader, WzPreReadResult result, PreReadState state, int depth)
        {
            if (depth > MaxDepth)
                throw new InvalidDataException("PKG2 directory tree is too deep.");

            long encryptedEntryCount = reader.ReadCompressedInt64();
            int entryCount = state.Calc.DecryptEntryCount(encryptedEntryCount);
            if (entryCount < 0 || entryCount > MaxEntryCount)
                throw new InvalidDataException($"Invalid PKG2 directory entry count: {entryCount}.");

            var entries = new PreReadEntry[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                long typePosition = reader.BaseStream.Position;
                byte rawNodeType = reader.ReadByte();
                byte nodeType = state.Calc.DecryptNodeType((uint)typePosition, rawNodeType);
                if (nodeType != 0x03 && nodeType != 0x04)
                    throw new InvalidDataException($"Invalid PKG2 node type {nodeType}.");

                reader.ReadCompressedInt32(); // checksum; not needed for structural pre-read
                long namePosition = reader.BaseStream.Position;
                short nameLength = reader.ReadInt16();
                if (nameLength > 0)
                    throw new InvalidDataException("Unexpected positive name length in PKG2 dir tree.");
                long nameBytes = (long)(-(int)nameLength) * 2;
                if (namePosition + 2 + nameBytes > reader.BaseStream.Length)
                    throw new EndOfStreamException();
                reader.BaseStream.Position += nameBytes;

                long sizePosition = reader.BaseStream.Position;
                int size = state.Calc.CalcLength((uint)sizePosition, reader.ReadCompressedInt32());

                entries[i] = new PreReadEntry
                {
                    NodeType = nodeType,
                    DataLength = size,
                    DataLengthPosition = (uint)sizePosition,
                };
            }

            result.Pkg2DirEntryCounts.Add(new Pkg2DirEntryCount
            {
                EncryptedEntryCount = encryptedEntryCount,
                ActualEntryCount = entryCount,
            });

            var offsetPositions = new uint[entryCount];
            var hashedOffsets = new uint[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                offsetPositions[i] = (uint)reader.BaseStream.Position;
                hashedOffsets[i] = reader.ReadUInt32();
            }

            for (int i = 0; i < entryCount; i++)
            {
                // Offsets are stored in reverse entry order.
                int sourceIndex = entryCount - 1 - i;
                result.Nodes.Add(new WzPreReadNodeInfo
                {
                    NodeType = entries[i].NodeType,
                    DataLength = entries[i].DataLength,
                    DataLengthPosition = entries[i].DataLengthPosition,
                    IsDataLengthEncrypted = false,
                    HashedOffsetPosition = offsetPositions[sourceIndex],
                    HashedOffset = hashedOffsets[sourceIndex],
                });
            }

            for (int i = 0; i < entryCount; i++)
            {
                if (entries[i].NodeType == 0x03)
                {
                    ReadDirectory(reader, result, state, depth + 1);
                }
            }
        }

        private struct PreReadState
        {
            public Pkg2OffsetCalc64V3 Calc;
        }

        private struct PreReadEntry
        {
            public byte NodeType;
            public int DataLength;
            public uint DataLengthPosition;
        }
    }
}
