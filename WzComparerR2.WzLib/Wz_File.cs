using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using WzComparerR2.WzLib.Utilities;

namespace WzComparerR2.WzLib
{
    public class Wz_File : IMapleStoryFile, IDisposable
    {
        public Wz_File(string fileName, Wz_Structure wz, string fallbackFileName = null)
        {
            this.imageCount = 0;
            this.wzStructure = wz;
            this.fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.loaded = this.GetHeader(fileName);
            this.directories = new List<Wz_Directory>();
        }

        private FileStream fileStream;
        private Wz_Structure wzStructure;
        private Wz_Header header;
        private Wz_Node node;
        private int imageCount;
        private bool loaded;
        private bool isSubDir;
        private Wz_Type type;
        private List<Wz_File> mergedWzFiles;
        private Wz_File ownerWzFile;
        private readonly List<Wz_Directory> directories;

        public Encoding TextEncoding { get; set; }
        public bool Forced { get; set; }
        public List<Tuple<long, long>> ret { get; set; } = new List<Tuple<long, long>>();
        public bool retInited { get; set; }
        public object ReadLock => this.fileStream;

        public FileStream FileStream
        {
            get { return fileStream; }
        }

        public Wz_Structure WzStructure
        {
            get { return wzStructure; }
            set { wzStructure = value; }
        }

        public Wz_Header Header
        {
            get { return header; }
            private set { header = value; }
        }

        public Wz_Node Node
        {
            get { return node; }
            set { node = value; }
        }

        public int ImageCount
        {
            get { return imageCount; }
        }

        public bool Loaded
        {
            get { return loaded; }
        }

        public bool IsSubDir
        {
            get { return this.isSubDir; }
        }

        public Wz_Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public IEnumerable<Wz_File> MergedWzFiles
        {
            get { return this.mergedWzFiles ?? Enumerable.Empty<Wz_File>(); }
        }

        public Wz_File OwnerWzFile
        {
            get { return this.ownerWzFile; }
        }

        Wz_Structure IMapleStoryFile.WzStructure => this.wzStructure;

        Stream IMapleStoryFile.FileStream => this.fileStream;

        object IMapleStoryFile.ReadLock => this.ReadLock;

        public void Close()
        {
            if (this.fileStream != null)
                this.fileStream.Close();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        private bool GetHeader(string fileName)
        {
            this.fileStream.Position = 0;
            var br = new WzBinaryReader(this.fileStream, false);

            long filesize = this.FileStream.Length;
            if (filesize < 4) { goto __failed; }

            string signature = new string(br.ReadChars(4));
            if (signature != Wz_Header.PKG1 && signature != Wz_Header.PKG2) { goto __failed; }

            long dataSize = br.ReadInt64();
            int headerSize = br.ReadInt32();
            string copyright = new string(br.ReadChars(headerSize - (int)this.FileStream.Position));

            if (signature == Wz_Header.PKG1)
            {
                // encver detecting:
                // Since KMST1132, wz removed the 2 bytes encver, and use a fixed wzver '777'.
                // Here we try to read the first 2 bytes from data part and guess if it looks like an encver.
                bool encverMissing = false;
                int encver = -1;
                if (dataSize >= 2)
                {
                    this.fileStream.Position = headerSize;
                    encver = br.ReadUInt16();
                    // encver always less than 256
                    if (encver > 0xff)
                    {
                        encverMissing = true;
                    }
                    else if (encver == 0x80)
                    {
                        // there's an exceptional case that the first field of data part is a compressed int which determined property count,
                        // if the value greater than 127 and also to be a multiple of 256, the first 5 bytes will become to
                        //   80 00 xx xx xx
                        // so we additional check the int value, at most time the child node count in a wz won't greater than 65536.
                        if (dataSize >= 5)
                        {
                            this.fileStream.Position = headerSize;
                            int propCount = br.ReadCompressedInt32();
                            if (propCount > 0 && (propCount & 0xff) == 0 && propCount <= 0xffff)
                            {
                                encverMissing = true;
                            }
                        }
                    }
                }
                else
                {
                    // Obviously, if data part have only 1 byte, encver must be deleted.
                    encverMissing = true;
                }

                int dataStartPos = headerSize + (encverMissing ? 0 : 2);
                this.Header = new Wz_Header(signature, copyright, fileName, headerSize, dataSize, filesize, dataStartPos);

                if (encverMissing)
                {
                    // not sure if nexon will change this magic version, just hard coded.
                    this.Header.SetWzVersion(777);
                    this.Header.VersionChecked = true;
                    this.Header.Capabilities |= Wz_Capabilities.EncverMissing;
                }
                else
                {
                    this.Header.SetOrdinalVersionDetector(encver);
                }
            }
            else if (signature == Wz_Header.PKG2)
            {
                uint hash1 = br.ReadUInt32();
                uint hash2 = br.ReadUInt32();
                int dataStartPos = (int)this.fileStream.Position;
                Wz_Header header = new(signature, copyright, fileName, headerSize, dataSize, filesize, dataStartPos);
                header.SetWzVersionPkg2(hash1, hash2);
                this.header = header;
            }
            else
            {
                goto __failed;
            }

            return true;

        __failed:
            this.header = new Wz_Header(null, null, fileName, 0, 0, filesize, 0);
            return false;
        }

        public uint CalcOffset(uint filePos, uint hashedOffset)
        {
            uint offset = (uint)(filePos - 0x3C) ^ 0xFFFFFFFF;
            int distance;

            offset *= this.Header.HashVersion;
            offset -= 0x581C3F6D;
            distance = (int)offset & 0x1F;
            offset = (offset << distance) | (offset >> (32 - distance));
            offset ^= hashedOffset;
            offset += 0x78;

            return offset;
        }

        // for KMST 1196-1197
        public uint CalcOffsetPkg2V1(uint filePos, uint hashedOffset)
        {
            uint headerLen = (uint)this.header.HeaderSize;
            uint hashVersion = this.header.HashVersion;
            uint hash1 = this.header.Pkg2Hash1;

            uint offset = filePos - headerLen;
            int distance;

            offset = ~offset;
            offset *= hashVersion;
            offset -= 0x581C3F6D;
            offset ^= hash1 * 0x01010101;
            distance = (byte)((hashVersion ^ hash1) & 0x1F);
            offset = (offset << distance) | (offset >> (32 - distance));
            offset ^= hashedOffset;
            offset += headerLen;

            return offset;
        }

        // for KMST 1198
        public uint CalcOffsetPkg2V2(uint filePos, uint hashedOffset)
        {
            uint headerLen = (uint)this.header.HeaderSize;
            uint hashVersion = this.header.HashVersion;
            uint hash1 = this.header.Pkg2Hash1;

            uint offset = filePos - headerLen;
            int distance;

            offset = ~offset;
            offset *= hashVersion ^ hash1;
            offset -= 0x581C3F6D;
            offset ^= hash1 * 0x01010101;
            distance = (byte)((hashVersion ^ hash1) & 0x1F);
            offset = (offset << distance) | (offset >> (32 - distance));
            offset ^= ~hashedOffset;
            offset += headerLen;

            return offset;
        }

        // for KMST 1196-1197
        public int DecryptPkg2EntryCountV1(int encryptedEntryCount)
        {
            uint hash1 = this.header.Pkg2Hash1;
            uint hashVersion = this.header.HashVersion;
            int entryCount = (int)(encryptedEntryCount ^ ((hash1 << 24) + (0x7F4A7C15 * hashVersion)));
            return entryCount;
        }

        // for KMST 1198
        public int DecryptPkg2EntryCountV2(int encryptedEntryCount)
        {
            uint hash1 = this.header.Pkg2Hash1;
            uint hashVersion = this.header.HashVersion;
            int entryCount = (int)(encryptedEntryCount ^ ((hash1 << 16) + (0x21524111 * hashVersion)));
            return entryCount;
        }

        public uint CalcHashVersionFromEntryCountV1(int encryptedEntryCount, int entryCount)
        {
            // calculate with modular inverse:
            uint hash1 = this.header.Pkg2Hash1;
            return ((uint)(entryCount ^ encryptedEntryCount) - (hash1 << 24)) * 0x9937733D;
        }

        public void GetDirTree(Wz_Node parent, bool useBaseWz = false, bool loadWzAsFolder = false, string fileName = null, string fallbackFileName = null)
        {
            var ps = new PartialStream(this.FileStream, this.header.DataStartPosition, this.fileStream.Length - this.header.DataStartPosition, true);
            ps.Position = 0;
            var reader = new WzBinaryReader(ps, false);
            this.GetDirTree(reader, parent, useBaseWz, loadWzAsFolder, fileName, fallbackFileName);
        }

        private void GetDirTree(WzBinaryReader reader, Wz_Node parent, bool useBaseWz = false, bool loadWzAsFolder = false, string fileName = null, string fallbackFileName = null)
        {
            List<string> dirs = new List<string>();

            if (this.header.Signature == Wz_Header.PKG1)
            {
                this.ReadDirTree(reader, parent, ref dirs);
            }
            else if (this.header.Signature == Wz_Header.PKG2)
            {
                this.ReadDirTreePkg2(reader, parent, ref dirs);
            }
            else
            {
                throw new Exception($"Unknown signature: {this.header.Signature}");
            }

            int dirCount = dirs.Count;
            bool willLoadBaseWz = useBaseWz ? parent.Text.Equals("base.wz", StringComparison.OrdinalIgnoreCase) : false;

            var baseFolder = Path.GetDirectoryName(fileName ?? this.header.FileName);
            var baseFolder2 = Path.GetDirectoryName(fallbackFileName);

            if (willLoadBaseWz && this.WzStructure.AutoDetectExtFiles)
            {
                for (int i = 0; i < dirCount; i++)
                {
                    //检测文件名
                    var m = Regex.Match(dirs[i], @"^([A-Za-z]+)$");
                    if (m.Success)
                    {
                        string wzTypeName = m.Result("$1");

                        //检测扩展wz文件
                        for (int fileID = 2; ; fileID++)
                        {
                            string extDirName = wzTypeName + fileID;
                            string extWzFile = Path.Combine(baseFolder, extDirName + ".wz");
                            if (File.Exists(extWzFile))
                            {
                                if (!dirs.Take(dirCount).Any(dir => extDirName.Equals(dir, StringComparison.OrdinalIgnoreCase)))
                                {
                                    dirs.Add(extDirName);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        //检测KMST1058的wz文件
                        for (int fileID = 1; ; fileID++)
                        {
                            string extDirName = wzTypeName + fileID.ToString("D3");
                            string extWzFile = Path.Combine(baseFolder, extDirName + ".wz");
                            if (File.Exists(extWzFile))
                            {
                                if (!dirs.Take(dirCount).Any(dir => extDirName.Equals(dir, StringComparison.OrdinalIgnoreCase)))
                                {
                                    dirs.Add(extDirName);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < dirs.Count; i++)
            {
                string dir = dirs[i];
                Wz_Node t = parent.Nodes.Add(dir);
                if (i < dirCount)
                {
                    this.GetDirTree(reader, t, false);
                }

                if (t.Nodes.Count == 0)
                {
                    this.WzStructure.has_basewz |= willLoadBaseWz;

                    try
                    {
                        if (loadWzAsFolder)
                        {
                            string wzFolder = willLoadBaseWz ? Path.Combine(Path.GetDirectoryName(baseFolder), dir) : Path.Combine(baseFolder, dir);
                            string wzFolder2 = baseFolder2 == null ? (string)null : (willLoadBaseWz ? Path.Combine(Path.GetDirectoryName(baseFolder2), dir) : Path.Combine(baseFolder2, dir));
                            if (!Directory.Exists(wzFolder))
                            {
                                if (!Directory.Exists(wzFolder2))
                                {
                                    continue;
                                }
                            }
                            this.wzStructure.LoadWzFolder(wzFolder, ref t, fallbackFolder: wzFolder2);
                            if (!willLoadBaseWz)
                            {
                                var dirWzFile = t.GetValue<Wz_File>();
                                dirWzFile.Type = Wz_Type.Unknown;
                                dirWzFile.isSubDir = true;
                            }
                        }
                        else if (willLoadBaseWz)
                        {
                            string filePath = Path.Combine(baseFolder, dir + ".wz");
                            if (File.Exists(filePath))
                            {
                                this.WzStructure.LoadFile(filePath, t, false, loadWzAsFolder);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            parent.Nodes.Trim();
        }

        private void ReadDirTree(WzBinaryReader reader, Wz_Node parent, ref List<string> dirs)
        {
            var cryptoKey = this.WzStructure.encryption.Pkg1Keys;
            int count = reader.ReadCompressedInt32();

            for (int i = 0; i < count; i++)
            {
                byte nodeType = reader.ReadByte();
                string name;
                switch (nodeType)
                {
                    case 0x02:
                        int stringOffAdd = this.Header.HasCapabilities(Wz_Capabilities.EncverMissing) ? 2 : -1;
                        name = reader.ReadStringAt(reader.ReadInt32() + stringOffAdd, cryptoKey);
                        break;
                    case 0x04:
                    case 0x03:
                        name = reader.ReadString(cryptoKey);
                        break;
                    default:
                        throw new Exception($"Unknown type {nodeType} in WzDirTree.");
                }

                int size = reader.ReadCompressedInt32();
                int cs32 = reader.ReadCompressedInt32();
                uint pos = (uint)this.fileStream.Position;
                uint hashOffset = reader.ReadUInt32();

                switch (nodeType)
                {
                    case 0x02:
                    case 0x04:
                        Wz_Image img = new Wz_Image(name, size, cs32, hashOffset, pos, this);
                        Wz_Node childNode = parent.Nodes.Add(name);
                        childNode.Value = img;
                        img.OwnerNode = childNode;
                        this.imageCount++;
                        break;

                    case 0x03:
                        this.directories.Add(new Wz_Directory(name, size, cs32, hashOffset, pos, this));
                        dirs.Add(name);
                        break;
                }
            }
        }

        private void ReadDirTreePkg2(WzBinaryReader reader, Wz_Node parent, ref List<string> dirs)
        {
            var encType = this.WzStructure.encryption.Pkg2EncType;
            var pkg1Keys = this.WzStructure.encryption.Pkg1Keys ?? this.WzStructure.encryption.GetKeys(Wz_CryptoKeyType.BMS);
            var pkg2Keys = this.WzStructure.encryption.Pkg2Keys;
            int encryptedEntryCount = reader.ReadCompressedInt32();

            List<Pkg2DirEntry> entries = new();
            while (true)
            {
                byte nodeType = reader.ReadByte();
                string name;
                if (nodeType == 0x03 || nodeType == 0x04)
                {
                    if (encType == Wz_CryptoKeyType.KMST1198)
                    {
                        name = entries.Count == 0 ? reader.ReadPkg2DirString(pkg2Keys) : reader.ReadString(pkg1Keys);
                    }
                    else
                    {
                        name = reader.ReadString(pkg2Keys);
                    }
                }
                else if (nodeType == 0x80 || (-127 <= encryptedEntryCount && encryptedEntryCount <= 127 && nodeType == encryptedEntryCount))
                {
                    // next byte is encryptedOffsetCount
                    reader.BaseStream.Position--;
                    break;
                }
                else
                {
                    throw new Exception($"Unknown type {nodeType} in WzDirTree.");
                }

                int size = reader.ReadCompressedInt32();
                int cs32 = reader.ReadCompressedInt32();
                entries.Add(new Pkg2DirEntry
                {
                    NodeType = nodeType,
                    Name = name,
                    DataLength = size,
                    Checksum = cs32
                });
            }

            int encryptedOffsetCount = reader.ReadCompressedInt32();
            if (encryptedOffsetCount == encryptedEntryCount && entries.Count > 0)
            {
                Span<Pkg2DirEntry> list = CollectionsMarshal.AsSpan(entries);
                for (int i = 0; i < list.Length; i++)
                {
                    uint pos = (uint)this.fileStream.Position;
                    uint hashOffset = reader.ReadUInt32();
                    ref Pkg2DirEntry entry = ref list[i];
                    switch (entry.NodeType)
                    {
                        case 0x04:
                            Wz_Image img = new Wz_Image(entry.Name, entry.DataLength, entry.Checksum, hashOffset, pos, this);
                            Wz_Node childNode = parent.Nodes.Add(entry.Name);
                            childNode.Value = img;
                            img.OwnerNode = childNode;
                            this.imageCount++;
                            break;

                        case 0x03:
                            this.directories.Add(new Wz_Directory(entry.Name, entry.DataLength, entry.Checksum, hashOffset, pos, this));
                            dirs.Add(entry.Name);
                            break;
                    }
                }
            }
        }

        public void ForceGetDirTree(
          Wz_Node parent,
          bool useBaseWz = false,
          bool loadWzAsFolder = false,
          string fileName = null,
          string fallbackFileName = null)
        {
            var partialStream = new PartialStream(
                this.FileStream,
                this.header.DataStartPosition,
                this.fileStream.Length - this.header.DataStartPosition,
                leaveOpen: true);

            partialStream.Position = 0;

            string name = $"{this.FileStream}_{this.header.DataStartPosition}";

            var reader = new WzBinaryReader(partialStream, false, name);

            this.ForceGetDirTree(reader, parent, useBaseWz, loadWzAsFolder, fileName, fallbackFileName);
        }


        private void ForceGetDirTree(WzBinaryReader reader, Wz_Node parent, bool useBaseWz = false, bool loadWzAsFolder = false, string fileName = null, string fallbackFileName = null)
        {
            List<string> dirs = new List<string>();
            if (!(this.header.Signature == "PKG2"))
                throw new Exception("Unknown signature: " + this.header.Signature);
            this.ForceReadDirTreePkg2(reader, parent, ref dirs);
            int count = dirs.Count;
            bool flag = useBaseWz && parent.Text.Equals("base.wz", StringComparison.OrdinalIgnoreCase);
            string directoryName1 = Path.GetDirectoryName(fileName ?? this.header.FileName);
            string directoryName2 = Path.GetDirectoryName(fallbackFileName);
            if (flag && this.WzStructure.AutoDetectExtFiles)
            {
                for (int i = 0; i < count; i++)
                {
                    var match = Regex.Match(dirs[i], "^([A-Za-z]+)$");
                    if (!match.Success)
                        continue;

                    string prefix = match.Groups[1].Value;
                    for (int n = 2; ; n++)
                    {
                        string extDir = prefix + n;
                        string path = Path.Combine(directoryName1, extDir + ".wz");

                        if (!File.Exists(path))
                            break;

                        if (!dirs.Take(count).Any(d => extDir.Equals(d, StringComparison.OrdinalIgnoreCase)))
                            dirs.Add(extDir);
                    }
                    for (int n = 1; ; n++)
                    {
                        string extDir = prefix + n.ToString("D3");
                        string path = Path.Combine(directoryName1, extDir + ".wz");

                        if (!File.Exists(path))
                            break;

                        if (!dirs.Take(count).Any(d => extDir.Equals(d, StringComparison.OrdinalIgnoreCase)))
                            dirs.Add(extDir);
                    }
                }
            }

            for (int index = 0; index < dirs.Count; ++index)
            {
                string str1 = dirs[index];
                Wz_Node node = parent.Nodes.Add(str1);
                if (index < count)
                    this.ForceGetDirTree(reader, node);
                if (node.Nodes.Count == 0)
                {
                    this.WzStructure.has_basewz |= flag;
                    try
                    {
                        if (loadWzAsFolder)
                        {
                            string str2 = flag ? Path.Combine(Path.GetDirectoryName(directoryName1), str1) : Path.Combine(directoryName1, str1);
                            string str3 = directoryName2 == null ? (string)null : (flag ? Path.Combine(Path.GetDirectoryName(directoryName2), str1) : Path.Combine(directoryName2, str1));
                            if (!Directory.Exists(str2))
                            {
                                if (!Directory.Exists(str3))
                                    continue;
                            }
                            this.wzStructure.LoadWzFolder(str2, ref node, fallbackFolder: str3, force: true);
                            if (!flag)
                            {
                                Wz_File wzFile = node.GetValue<Wz_File>();
                                wzFile.Type = Wz_Type.Unknown;
                                wzFile.isSubDir = true;
                            }
                        }
                        else if (flag)
                        {
                            string str4 = Path.Combine(directoryName1, str1 + ".wz");
                            if (File.Exists(str4))
                                this.WzStructure.LoadFile(str4, node, loadWzAsFolder: loadWzAsFolder, force: true);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            parent.Nodes.Trim();
        }

        private void ForceReadDirTreePkg2(WzBinaryReader reader, Wz_Node parent, ref List<string> dirs)
        {
            this.WzStructure.encryption.Pkg2EncType2 = Wz_CryptoKeyType.KMST1199;
            IWzDecrypter decrypter = this.WzStructure.encryption.Pkg1Keys ?? this.WzStructure.encryption.GetKeys(Wz_CryptoKeyType.BMS);
            IWzDecrypter pkg2Keys2 = this.WzStructure.encryption.Pkg2Keys2;
            int num1 = reader.ReadCompressedInt32();
            this.FindAllHits(this);
            int count = this.ret.Count;
            int index1 = 0;
            List<Wz_File.Pkg2DirEntry> list = new List<Wz_File.Pkg2DirEntry>();
            byte num2;
            while (true)
            {
                num2 = reader.ReadByte();

                if (num2 == 3 || num2 == 4)
                {
                    string name;
                    try
                    {
                        name = list.Count == 0
                            ? reader.ReadPkg2DirString2(pkg2Keys2)
                            : reader.ReadString(decrypter);
                    }
                    catch
                    {
                        this.WzStructure.encryption.Pkg2EncType = Wz_CryptoKeyType.BMS;
                        name = reader.ReadString(pkg2Keys2);
                    }

                    int dataLength = reader.ReadCompressedInt32();
                    int checksum = reader.ReadCompressedInt32();

                    long forcedOffset = 0;
                    if (num2 == 4 && index1 < this.ret.Count)
                    {
                        forcedOffset = dataLength == this.ret[index1].Item2
                            ? this.ret[index1].Item1
                            : 0;
                        index1++;
                    }

                    list.Add(new Wz_File.Pkg2DirEntry
                    {
                        NodeType = num2,
                        Name = name,
                        DataLength = dataLength,
                        Checksum = checksum,
                        ForcedOffset = forcedOffset
                    });
                    continue;
                }
                if (num2 == 128)
                    break;
                break;
            }
            if (num2 != count && (count < -127 || count > sbyte.MaxValue))
                throw new Exception($"Unknown type {num2} in WzDirTree.");
            reader.BaseStream.Position--;

            if (reader.ReadCompressedInt32() != num1 || list.Count == 0)
                return;

            var span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                ref var entry = ref span[i];

                int dummy = (int)reader.ReadUInt32();

                switch (entry.NodeType)
                {
                    case 3:
                        this.directories.Add(new Wz_Directory(entry.Name, entry.DataLength, entry.Checksum, 0, 0, this));
                        dirs.Add(entry.Name);
                        break;

                    case 4:
                        var img = new Wz_Image(entry.Name, entry.DataLength, entry.Checksum, 0, 0, this)
                        {
                            ForcedOffset = entry.ForcedOffset,
                            Offset = entry.ForcedOffset
                        };

                        var node = parent.Nodes.Add(entry.Name);
                        node.Value = img;
                        img.OwnerNode = node;

                        this.imageCount++;
                        break;
                }
            }

        }

        public void FindAllHits(Wz_File file)
        {
            if (this.retInited)
                return;
            PartialStream partialStream = new PartialStream((Stream)file.FileStream, file.Header.DataStartPosition, file.FileStream.Length - file.Header.DataStartPosition, true);
            partialStream.Position = 0L;
            string name = $"{file.FileStream}_{file.Header.DataStartPosition}";
            var reader = new WzBinaryReader(partialStream, false, name);
            this.ret.Clear();
            byte[] pattern = new byte[5]
            {
        (byte) 115,
        (byte) 248,
        (byte) 250,
        (byte) 217,
        (byte) 195
            };
            long position = reader.BaseStream.Position;
            reader.BaseStream.Position = 0L;
            List<long> allPatterns = Wz_File.FindAllPatterns(reader, pattern);
            List<long> longList = Wz_File.DiffAdjacent(allPatterns);
            if (allPatterns.Count == longList.Count + 1)
            {
                long dataStartPosition = this.header.DataStartPosition;
                for (int index = 0; index < longList.Count; ++index)
                    this.ret.Add(new Tuple<long, long>(allPatterns[index] + this.header.DataStartPosition, longList[index]));
            }
            reader.BaseStream.Position = position;
        }

        private static List<long> FindAllPatterns(WzBinaryReader reader, byte[] pattern)
        {
            int num1 = pattern != null && pattern.Length != 0 ? pattern.Length : throw new ArgumentException("pattern must not be empty");
            int num2 = num1 - 1;
            byte[] numArray1 = ArrayPool<byte>.Shared.Rent(262144 + num2);
            List<long> allPatterns = new List<long>();
            int[] numArray2 = new int[256];
            for (int index = 0; index < numArray2.Length; ++index)
                numArray2[index] = num1;
            for (int index = 0; index < num1 - 1; ++index)
                numArray2[(int)pattern[index]] = num1 - 1 - index;
            long num3 = 0;
            try
            {
                int num4;
                while ((num4 = reader.BaseStream.Read(numArray1, num2, 262144)) > 0)
                {
                    int num5 = num4 + num2;
                    int num6 = num5 - num1;
                    int num7 = 0;
                    while (num7 <= num6)
                    {
                        int index = num1 - 1;
                        while (index >= 0 && (int)numArray1[num7 + index] == (int)pattern[index])
                            --index;
                        if (index < 0)
                        {
                            long num8 = num3 + (long)num7 - (long)num2;
                            if (num8 >= 0L)
                                allPatterns.Add(num8);
                            ++num7;
                        }
                        else
                            num7 += numArray2[(int)numArray1[num7 + num1 - 1]];
                    }
                    if (num2 > 0)
                        Buffer.BlockCopy((Array)numArray1, num5 - num2, (Array)numArray1, 0, num2);
                    num3 += (long)num4;
                }
                allPatterns.Add(reader.BaseStream.Length);
                return allPatterns;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(numArray1);
            }
        }

        private static List<long> DiffAdjacent(List<long> list)
        {
            if (list.Count == 0)
                return list;
            List<long> longList = new List<long>();
            for (int index = 1; index < list.Count; ++index)
                longList.Add(list[index] - list[index - 1]);
            return longList;
        }

        private string getFullPath(Wz_Node parent, string name)
        {
            List<string> path = new List<string>(5);
            path.Add(name.ToLower());
            while (parent != null && !(parent.Value is Wz_File))
            {
                path.Insert(0, parent.Text.ToLower());
                parent = parent.ParentNode;
            }
            if (parent != null)
            {
                path.Insert(0, parent.Text.ToLower().Replace(".wz", ""));
            }
            return string.Join("/", path.ToArray());
        }

        public void DetectWzType()
        {
            this.type = Wz_Type.Unknown;
            if (this.node == null)
            {
                return;
            }

            if (this.node.Nodes["smap.img"] != null
                || this.node.Nodes["zmap.img"] != null)
            {
                this.type = Wz_Type.Base;
            }
            else if (this.node.Nodes["00002000.img"] != null
                || this.node.Nodes["Accessory"] != null
                || this.node.Nodes["Weapon"] != null)
            {
                this.type = Wz_Type.Character;
            }
            else if (this.node.Nodes["BasicEff.img"] != null
                || this.node.Nodes["SetItemInfoEff.img"] != null)
            {
                this.type = Wz_Type.Effect;
            }
            else if (this.node.Nodes["Commodity.img"] != null
                || this.node.Nodes["Curse.img"] != null)
            {
                this.type = Wz_Type.Etc;
            }
            else if (this.node.Nodes["Cash"] != null
                || this.node.Nodes["Consume"] != null)
            {
                this.type = Wz_Type.Item;
            }
            else if (this.node.Nodes["Back"] != null
                || this.node.Nodes["Obj"] != null
                || this.node.Nodes["Physics.img"] != null)
            {
                this.type = Wz_Type.Map;
            }
            else if (this.node.Nodes["PQuest.img"] != null
                || this.node.Nodes["QuestData"] != null)
            {
                this.type = Wz_Type.Quest;
            }
            else if (this.node.Nodes["Attacktype.img"] != null
                || this.node.Nodes["Recipe_9200.img"] != null)
            {
                this.type = Wz_Type.Skill;
            }
            else if (this.node.Nodes["Bgm00.img"] != null
                || this.node.Nodes["BgmUI.img"] != null)
            {
                this.type = Wz_Type.Sound;
            }
            else if (this.node.Nodes["MonsterBook.img"] != null
                || this.node.Nodes["EULA.img"] != null)
            {
                this.type = Wz_Type.String;
            }
            else if (this.node.Nodes["CashShop.img"] != null
                || this.node.Nodes["UIWindow.img"] != null)
            {
                this.type = Wz_Type.UI;
            }

            if (this.type == Wz_Type.Unknown) //用文件名来判断
            {
                string wzName = this.node.Text;

                Match m = Regex.Match(wzName, @"^([A-Za-z]+)_?(\d+)?(?:\.wz)?$");
                if (m.Success)
                {
                    wzName = m.Result("$1");
                }
                this.type = Enum.TryParse<Wz_Type>(wzName, true, out var result) ? result : Wz_Type.Unknown;
            }
        }

        public void DetectWzVersion()
        {
            if (this.Forced)
            {
                this.header.VersionChecked = true;
            }
            else
            {
                Wz_File.IWzVersionVerifier wzVersionVerifier;
                if (this.Header.Signature == "PKG1")
                {
                    WzVersionVerifyMode? versionVerifyMode = this.wzStructure?.WzVersionVerifyMode;
                    if (versionVerifyMode.HasValue)
                    {
                        switch (versionVerifyMode.GetValueOrDefault())
                        {
                            case WzVersionVerifyMode.Fast:
                                wzVersionVerifier = (Wz_File.IWzVersionVerifier)new Wz_File.FastVersionVerifier();
                                goto label_10;
                        }
                    }
                    wzVersionVerifier = (Wz_File.IWzVersionVerifier)new Wz_File.DefaultVersionVerifier();
                }
                else
                {
                    if (!(this.header.Signature == "PKG2"))
                        throw new Exception("Unknown signature: " + this.header.Signature);
                    wzVersionVerifier = (Wz_File.IWzVersionVerifier)new Wz_File.Pkg2VersionVerifier();
                }
            label_10:
                wzVersionVerifier.Verify(this);
            }
        }

        public void MergeWzFile(Wz_File wz_File)
        {
            var children = wz_File.node.Nodes.ToList();
            wz_File.node.Nodes.Clear();
            foreach (var child in children)
            {
                this.node.Nodes.Add(child);
            }

            if (this.mergedWzFiles == null)
            {
                this.mergedWzFiles = new List<Wz_File>();
            }
            this.mergedWzFiles.Add(wz_File);

            wz_File.ownerWzFile = this;
        }


        public interface IWzVersionVerifier
        {
            bool Verify(Wz_File wzFile);
        }

        public abstract class WzVersionVerifier
        {
            protected abstract uint CalcOffset(Wz_File wzFile, uint filePos, uint hashedOffset);

            protected IEnumerable<Wz_Image> EnumerableAllWzImage(Wz_Node parentNode)
            {
                foreach (var node in parentNode.Nodes)
                {
                    Wz_Image img = node.Value as Wz_Image;
                    if (img != null)
                    {
                        yield return img;
                    }

                    if (!(node.Value is Wz_File) && node.Nodes.Count > 0)
                    {
                        foreach (var imgChild in EnumerableAllWzImage(node))
                        {
                            yield return imgChild;
                        }
                    }
                }
            }

            protected bool FastCheckFirstByte(Wz_Image image, byte firstByte)
            {
                if (image.IsLuaImage)
                {
                    // for lua image, the first byte is always 01
                    return firstByte == 0x01;
                }
                else
                {
                    // first element is always a string
                    return firstByte == 0x73 || firstByte == 0x1b;
                }
            }

            protected void CalcOffset(Wz_File wzFile, IEnumerable<Wz_Image> imgList)
            {
                foreach (var img in imgList)
                {
                    img.Offset = img.ForcedOffset != -1L ? img.ForcedOffset : (long)this.CalcOffset(wzFile, img.HashedOffsetPosition, img.HashedOffset);
                }
            }

            protected bool DetectWithWzImage(Wz_File wzFile, Wz_Image testWzImg)
            {
                while (wzFile.header.TryGetNextVersion())
                {
                    uint offs = this.CalcOffset(wzFile, testWzImg.HashedOffsetPosition, testWzImg.HashedOffset);

                    if (offs < wzFile.header.DirEndPosition || offs + testWzImg.Size > wzFile.fileStream.Length)  //img offset out of file size
                    {
                        continue;
                    }

                    wzFile.fileStream.Position = offs;
                    var firstByte = (byte)wzFile.fileStream.ReadByte();
                    if (!FastCheckFirstByte(testWzImg, firstByte))
                    {
                        continue;
                    }

                    testWzImg.Offset = offs;
                    if (!testWzImg.TryExtract())
                    {
                        continue;
                    }

                    testWzImg.Unextract();
                    wzFile.header.VersionChecked = true;
                    break;
                }

                return wzFile.header.VersionChecked;
            }

            protected bool DetectWithAllWzDir(Wz_File wzFile)
            {
                while (wzFile.header.TryGetNextVersion())
                {
                    bool isSuccess = wzFile.directories.All(testDir =>
                    {
                        uint offs = this.CalcOffset(wzFile, testDir.HashedOffsetPosition, testDir.HashedOffset);

                        if (offs < wzFile.header.DataStartPosition || offs + 1 > wzFile.header.DirEndPosition) // dir offset out of file size.
                        {
                            return false;
                        }

                        wzFile.fileStream.Position = offs;
                        if (wzFile.fileStream.ReadByte() != 0) // for splitted wz format, dir data only contains one byte: 0x00
                        {
                            return false;
                        }

                        return true;
                    });

                    if (isSuccess)
                    {
                        wzFile.header.VersionChecked = true;
                        break;
                    }
                }

                return wzFile.header.VersionChecked;
            }

            protected bool FastDetectWithAllWzImages(Wz_File wzFile, IList<Wz_Image> imgList)
            {
                var imageSizes = new SizeRange[imgList.Count];
                while (wzFile.header.TryGetNextVersion())
                {
                    int count = 0;
                    bool isSuccess = imgList.All(img =>
                    {
                        uint offs = this.CalcOffset(wzFile, img.HashedOffsetPosition, img.HashedOffset);
                        if (offs < wzFile.header.DirEndPosition || offs + img.Size > wzFile.fileStream.Length)  //img offset out of file size
                        {
                            return false;
                        }

                        imageSizes[count++] = new SizeRange()
                        {
                            Start = offs,
                            End = offs + img.Size,
                        };
                        return true;
                    });

                    if (isSuccess)
                    {
                        // check if there's any image overlaps with another image.
                        Array.Sort(imageSizes, 0, count);
                        for (int i = 1; i < count; i++)
                        {
                            if (imageSizes[i - 1].End > imageSizes[i].Start)
                            {
                                isSuccess = false;
                                break;
                            }
                        }

                        if (isSuccess)
                        {
                            wzFile.header.VersionChecked = true;
                            break;
                        }
                    }
                }

                return wzFile.header.VersionChecked;
            }

            private struct SizeRange : IComparable<SizeRange>
            {
                public long Start;
                public long End;

                public int CompareTo(SizeRange sr)
                {
                    int result = this.Start.CompareTo(sr.Start);
                    if (result == 0)
                    {
                        result = this.End.CompareTo(sr.End);
                    }
                    return result;
                }
            }
        }

        public class DefaultVersionVerifier : WzVersionVerifier, IWzVersionVerifier
        {
            public bool Verify(Wz_File wzFile)
            {
                List<Wz_Image> imgList = EnumerableAllWzImage(wzFile.node).Where(_img => _img.WzFile == wzFile).ToList();

                if (wzFile.header.VersionChecked)
                {
                    this.CalcOffset(wzFile, imgList);
                }
                else
                {
                    // find the wzImage with minimum size.
                    Wz_Image minSizeImg = imgList.DefaultIfEmpty().Aggregate((_img1, _img2) => _img1.Size < _img2.Size ? _img1 : _img2);

                    if (minSizeImg == null && imgList.Count > 0)
                    {
                        minSizeImg = imgList[0];
                    }

                    if (minSizeImg != null)
                    {
                        this.DetectWithWzImage(wzFile, minSizeImg);
                    }
                    else if (wzFile.directories.Count > 0)
                    {
                        this.DetectWithAllWzDir(wzFile);
                    }

                    if (wzFile.header.VersionChecked)
                    {
                        this.CalcOffset(wzFile, imgList);
                    }
                }

                return wzFile.header.VersionChecked;
            }

            protected override uint CalcOffset(Wz_File wzFile, uint filePos, uint hashedOffset) => wzFile.CalcOffset(filePos, hashedOffset);
        }

        public class FastVersionVerifier : WzVersionVerifier, IWzVersionVerifier
        {
            public virtual bool Verify(Wz_File wzFile)
            {
                List<Wz_Image> imgList = EnumerableAllWzImage(wzFile.node).Where(_img => _img.WzFile == wzFile).ToList();
                {
                    if (imgList.Count > 0)
                    {
                        this.FastDetectWithAllWzImages(wzFile, imgList);
                    }
                    else if (wzFile.directories.Count > 0)
                    {
                        this.DetectWithAllWzDir(wzFile);
                    }

                    if (wzFile.header.VersionChecked)
                    {
                        this.CalcOffset(wzFile, imgList);
                    }
                }

                return wzFile.header.VersionChecked;
            }

            protected override uint CalcOffset(Wz_File wzFile, uint filePos, uint hashedOffset) => wzFile.CalcOffset(filePos, hashedOffset);
        }

        public class Pkg2VersionVerifier : FastVersionVerifier, IWzVersionVerifier
        {
            private const int CryptoVersionMin = 1;
            private const int CryptoVersionMax = 2;
            public override bool Verify(Wz_File wzFile)
            {
                for (int i = CryptoVersionMax; i >= CryptoVersionMin; i--)
                {
                    this.cryptoVersion = i;
                    wzFile.header.ResetVersionDetector();
                    if (base.Verify(wzFile))
                    {
                        break;
                    }
                }
                return wzFile.header.VersionChecked;
            }

            private int cryptoVersion;
            protected override uint CalcOffset(Wz_File wzFile, uint filePos, uint hashedOffset) => this.cryptoVersion switch
            {
                1 => wzFile.CalcOffsetPkg2V1(filePos, hashedOffset),
                2 => wzFile.CalcOffsetPkg2V2(filePos, hashedOffset),
                _ => throw new InvalidOperationException($"Unknown cryptoVersion {this.cryptoVersion}."),
            };
        }

        private struct Pkg2DirEntry
        {
            public int NodeType;
            public string Name;
            public int DataLength;
            public int Checksum;
            public long ForcedOffset;
        }
    }

    public enum WzVersionVerifyMode
    {
        Default = 0,
        Fast = 1,
    }
}