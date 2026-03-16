using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace WzComparerR2.WzLib.Utilities
{
    internal class WzBinaryReader
    {
        public WzBinaryReader(Stream stream, bool useStringPool)
            : this(stream, useStringPool ? new SimpleWzStringPool() : null)
        {
        }

        public WzBinaryReader(Stream stream, IWzStringPool stringPool)
        {
            this.BaseStream = stream;
            this.bReader = new BinaryReader(this.BaseStream, System.Text.Encoding.ASCII, true);
            this.stringPool = stringPool;
        }

        public WzBinaryReader(Stream stream, bool useStringPool, string name)
          : this(stream, useStringPool ? (IWzStringPool)new SimpleWzStringPool() : (IWzStringPool)null)
        {
            this.Name = name;
        }

        public Stream BaseStream { get; private set; }
        public int StringReferenceOffsetBytes { get; set; }
        public string Name { get; set; }
        private BinaryReader bReader;
        private IWzStringPool stringPool;

        public byte ReadByte()
        {
            return this.bReader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return this.bReader.ReadSByte();
        }

        public short ReadInt16()
        {
            return this.bReader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return this.bReader.ReadUInt16();
        }

        public int ReadCompressedInt32()
        {
            int s = this.bReader.ReadSByte();
            return (s == -128) ? this.bReader.ReadInt32() : s;
        }

        public int ReadInt32()
        {
            return this.bReader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return this.bReader.ReadUInt32();
        }

        public long ReadCompressedInt64()
        {
            int s = this.bReader.ReadSByte();
            return (s == -128) ? this.bReader.ReadInt64() : s;
        }

        public long ReadInt64()
        {
            return this.bReader.ReadInt64();
        }

        public float ReadCompressedSingle()
        {
            float fl = this.bReader.ReadSByte();
            return (fl == -128) ? this.bReader.ReadSingle() : fl;
        }

        public double ReadDouble()
        {
            return this.bReader.ReadDouble();
        }

        public char[] ReadChars(int count)
        {
            return this.bReader.ReadChars(count);
        }

        public string ReadString(IWzDecrypter decrypter)
        {
            long currentPos = this.BaseStream.Position;

            int size = this.ReadSByte();
            if (size < 0) // read ASCII/cp1252 string
            {
                size = (size == -128) ? this.ReadInt32() : -size;

                // for net6+ we can use Stream.Read(Span<byte>) instead, the array buffer is not needed.
                var buffer = ArrayPool<byte>.Shared.Rent(size);
                try
                {
                    this.BaseStream.ReadExactly(buffer, 0, size);
                    decrypter.Decrypt(buffer, 0, size);

                    using var charBuffer = MemoryPool<char>.Shared.Rent(size);
                    Span<char> chars = charBuffer.Memory.Span.Slice(0, size);
                    // TODO: SIMD optimization for net6
                    byte mask = 0xAA;
                    for (int i = 0; i < size; i++)
                    {
                        chars[i] = (char)(buffer[i] ^ mask);
                        mask++;
                    }
                    return this.stringPool != null ? this.stringPool.GetOrAdd(currentPos, chars) : chars.ToString();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else if (size > 0) // read UTF-16LE string
            {
                if (size == 127)
                {
                    size = this.bReader.ReadInt32();
                }
                int byteSize = size * 2;
                var buffer = ArrayPool<byte>.Shared.Rent(byteSize);
                try
                {
                    this.BaseStream.ReadExactly(buffer, 0, byteSize);
                    decrypter.Decrypt(buffer, 0, byteSize);

                    Span<char> chars = MemoryMarshal.Cast<byte, char>(buffer.AsSpan(0, byteSize));
                    // TODO: SIMD optimization for net6
                    ushort mask = 0xAAAA;
                    for (int i = 0; i < size; i++)
                    {
                        chars[i] = (char)(chars[i] ^ mask);
                        mask++;
                    }
                    return this.stringPool != null ? this.stringPool.GetOrAdd(currentPos, chars) : chars.ToString();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else
            {
                return string.Empty;
            }
        }

        // Introduced in KMST1198
        public string ReadPkg2DirString(IWzDecrypter decrypter)
        {
            long currentPos = this.BaseStream.Position;

            int size = this.ReadSByte();
            if (size < 0)
            {
                size = -size;
                int byteSize = size * 2;
                var buffer = ArrayPool<byte>.Shared.Rent(byteSize);
                try
                {
                    this.BaseStream.ReadExactly(buffer, 0, byteSize);
                    decrypter.Decrypt(buffer, 0, byteSize);
                    Span<char> chars = MemoryMarshal.Cast<byte, char>(buffer.AsSpan(0, byteSize));
                    return this.stringPool != null ? this.stringPool.GetOrAdd(currentPos, chars) : chars.ToString();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else if (size > 0)
            {
                throw new Exception($"Unexpected string length: {size}");
            }
            else
            {
                return string.Empty;
            }
        }

        // Introduced in KMST1199
        public string ReadPkg2DirString2(IWzDecrypter decrypter, string force = null)
        {
            long position = this.BaseStream.Position;
            int num1 = (int)this.ReadSByte();
            if (num1 < 0)
            {
                int length = -num1;
                int num2 = length * 2;
                byte[] numArray = ArrayPool<byte>.Shared.Rent(num2);
                try
                {
                    this.BaseStream.ReadExactly(numArray, 0, num2);
                    MemoryMarshal.Cast<byte, char>(numArray.AsSpan<byte>(0, num2));
                    Span<char> span = (Span<char>)new char[length];
                    byte num3 = 157;
                    for (int index = 0; index < length; ++index)
                        span[index] = index == 0 || index % 4 == 0 ? (char)((uint)numArray[index * 2] ^ (uint)num3) : (char)((uint)numArray[index * 2] ^ (uint)numArray[index * 2 - 1]);
                    string str = span.ToString();
                    char ch1 = str[length - 1];
                    char ch2 = str[length - 2];
                    char ch3 = str[length - 3];
                    char ch4 = str[length - 4];
                    if (ch4 == '.' && ch3 != 'i' && ch2 == 'm' && ch1 == 'g')
                    {
                        byte num4 = (byte)((uint)numArray[(length - 3) * 2] ^ 105U);
                        for (int index = length - 3; index >= 0; index -= 4)
                            span[index] = (char)((uint)numArray[index * 2] ^ (uint)num4);
                        return span.ToString();
                    }
                    if (ch4 == '.' && ch3 == 'i' && ch2 != 'm' && ch1 == 'g')
                    {
                        byte num5 = (byte)((uint)numArray[(length - 2) * 2] ^ 109U);
                        for (int index = length - 2; index >= 0; index -= 4)
                            span[index] = (char)((uint)numArray[index * 2] ^ (uint)num5);
                        return span.ToString();
                    }
                    if (ch4 == '.' && ch3 == 'i' && ch2 == 'm' && ch1 != 'g')
                    {
                        byte num6 = (byte)((uint)numArray[(length - 1) * 2] ^ 103U);
                        for (int index = length - 1; index >= 0; index -= 4)
                            span[index] = (char)((uint)numArray[index * 2] ^ (uint)num6);
                        return span.ToString();
                    }
                    if (ch4 != '.' && ch3 == 'i' && ch2 == 'm' && ch1 == 'g')
                    {
                        byte num7 = (byte)((uint)numArray[(length - 4) * 2] ^ 46U);
                        for (int index = length - 4; index >= 0; index -= 4)
                            span[index] = (char)((uint)numArray[index * 2] ^ (uint)num7);
                        return span.ToString();
                    }
                    if (str.Length == 6)
                        str = "Dragon";
                    else if (str.Length == 7)
                        str = "_Canvas";
                    else if (str.Length == 4)
                        str = "Cash";
                    return str;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(numArray);
                }
            }
            else
            {
                if (num1 > 0)
                {
                    throw new Exception($"Unexpected string length: {num1}");
                }
                return string.Empty;
            }
        }

        public string ReadImageObjectTypeName(IWzDecrypter decrypter)
        {
            int flag = this.bReader.ReadByte();
            switch (flag)
            {
                case 0x73:
                    return this.ReadString(decrypter);
                case 0x1B:
                    return this.ReadStringAt(this.ReadInt32() + this.StringReferenceOffsetBytes, decrypter);
                default:
                    throw new Exception($"Unexpected flag '{flag}' when reading string at {this.BaseStream.Position}.");
            }
        }

        public string ReadImageString(IWzDecrypter decrypter)
        {
            int flag = this.bReader.ReadByte();
            switch (flag)
            {
                case 0x00:
                    return this.ReadString(decrypter);
                case 0x01:
                    return this.ReadStringAt(this.ReadInt32() + this.StringReferenceOffsetBytes, decrypter);
                case 0x04:
                    this.SkipBytes(8);
                    return null;
                default:
                    throw new Exception($"Unexpected flag '{flag}' when reading string at {this.BaseStream.Position}.");
            }
        }

        public string ReadStringAt(long offset, IWzDecrypter decrypter)
        {
            if (this.stringPool != null && this.stringPool.TryGet(offset, out string s))
            {
                return s;
            }
            long currentPos = this.BaseStream.Position;
            this.BaseStream.Position = offset;
            s = this.ReadString(decrypter);
            this.BaseStream.Position = currentPos;
            return s;
        }

        public byte[] ReadBytes(int count)
        {
            return this.bReader.ReadBytes(count);
        }

        public void SkipBytes(int count)
        {
            if (this.BaseStream.CanSeek)
            {
                this.BaseStream.Position += count;
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(Math.Min(count, 16384));
                try
                {
                    while (count > 0)
                    {
                        int actual = this.BaseStream.Read(buffer, 0, Math.Min(count, buffer.Length));
                        if (actual == 0)
                        {
                            throw new EndOfStreamException();
                        }
                        count -= actual;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}