using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

namespace Archiver
{
    public class Buffer
    {
        private StringBuilder buffer = new StringBuilder();
        private int maxSize = 32 * 1024;

        public Buffer() { }

        public Buffer(int windowSize)
        {
            maxSize = windowSize;
        }

        public string GetBuffer()
        {
            return buffer.ToString();
        }

        public int Size()
        {
            return buffer.Length;
        }

        public void Add(string data)
        {
            buffer.Append(data);
            if (buffer.Length > maxSize)
            {
                buffer.Remove(0, buffer.Length - maxSize);
            }
        }

        public void Add(char c)
        {
            buffer.Append(c);
            if (buffer.Length > maxSize)
            {
                buffer.Remove(0, 1);
            }
        }

        public void Shift(string data)
        {
            Add(data);
        }

        public void Clear()
        {
            buffer.Clear();
        }

        public char this[int index]
        {
            get { return buffer[index]; }
        }

        public override string ToString()
        {
            return $"Window[{Size()}]: {GetBuffer()}";
        }
    }

    public class HuffmanCompressor : ICompressor
    {
        private class Node : IComparable<Node>
        {
            public char Symbol { get; }
            public int Frequency { get; }
            public Node Left { get; }
            public Node Right { get; }

            public Node(char symbol, int frequency)
            {
                Symbol = symbol;
                Frequency = frequency;
            }

            public Node(int frequency, Node left, Node right)
            {
                Symbol = '\0';
                Frequency = frequency;
                Left = left;
                Right = right;
            }

            public bool IsLeaf()
            {
                return Left == null && Right == null;
            }

            public int CompareTo(Node other)
            {
                return Frequency.CompareTo(other.Frequency);
            }
        }

        private Node root;
        private Dictionary<char, string> huffmanCodes = new Dictionary<char, string>();

        public HuffmanCompressor()
        {
            root = null;
        }

        public string Encode(string text)
        {
            if (huffmanCodes.Count == 0)
            {
                BuildHuffmanTree(text);
            }

            StringBuilder encodedText = new StringBuilder();
            foreach (char c in text)
            {
                encodedText.Append(huffmanCodes[c]);
            }
            return encodedText.ToString();
        }

        public string Decode(string encodedText)
        {
            if (root == null)
            {
                throw new InvalidOperationException("Huffman tree not built - cannot decode");
            }

            StringBuilder decodedText = new StringBuilder();
            Node current = root;

            foreach (char bit in encodedText)
            {
                if (bit == '0')
                {
                    current = current.Left;
                }
                else if (bit == '1')
                {
                    current = current.Right;
                }
                else
                {
                    throw new ArgumentException("Invalid encoded character - expected '0' or '1'");
                }

                if (current == null)
                {
                    throw new ArgumentException("Invalid encoded data - reached null node");
                }

                if (current.IsLeaf())
                {
                    decodedText.Append(current.Symbol);
                    current = root;
                }
            }

            if (current != root)
            {
                throw new ArgumentException("Invalid encoded data - incomplete sequence");
            }

            return decodedText.ToString();
        }

        private void BuildHuffmanTree(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            Dictionary<char, int> frequencies = new Dictionary<char, int>();
            foreach (char c in text)
            {
                if (frequencies.ContainsKey(c))
                    frequencies[c]++;
                else
                    frequencies[c] = 1;
            }

            var priorityQueue = new SortedSet<Node>(Comparer<Node>.Create((a, b) =>
            {
                int freqCompare = a.Frequency.CompareTo(b.Frequency);
                if (freqCompare == 0) return 1;
                return freqCompare;
            }));

            foreach (var pair in frequencies)
            {
                priorityQueue.Add(new Node(pair.Key, pair.Value));
            }

            while (priorityQueue.Count > 1)
            {
                var left = priorityQueue.Min;
                priorityQueue.Remove(left);
                var right = priorityQueue.Min;
                priorityQueue.Remove(right);

                var parent = new Node(left.Frequency + right.Frequency, left, right);
                priorityQueue.Add(parent);
            }

            if (priorityQueue.Count > 0)
            {
                root = priorityQueue.Min;
                GenerateCodes(root, "");
            }
        }

        private void GenerateCodes(Node node, string code)
        {
            if (node == null) return;

            if (node.IsLeaf())
            {
                huffmanCodes[node.Symbol] = code;
                return;
            }

            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }

    public interface ICompressor
    {
        string Encode(string text);
        string Decode(string encodedText);
    }

    public class LZ77Compressor
    {
        public struct LZ77Node
        {
            public int Offset { get; }
            public int Length { get; }
            public char Next { get; }

            public LZ77Node(int offset, int length, char next)
            {
                Offset = offset;
                Length = length;
                Next = next;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is LZ77Node)) return false;
                LZ77Node other = (LZ77Node)obj;
                return Offset == other.Offset &&
                       Length == other.Length &&
                       Next == other.Next;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Offset, Length, Next);
            }

            public static bool operator ==(LZ77Node left, LZ77Node right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(LZ77Node left, LZ77Node right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"({Offset},{Length},'{Next}')";
            }
        }

        private Buffer buffer = new Buffer();

        public LZ77Compressor() { }

        public List<int> FindMatching(string searchBuffer, string lookAheadBuffer, int pos)
        {
            int bestOffset = 0;
            int bestLength = 0;

            if (string.IsNullOrEmpty(searchBuffer) || pos >= lookAheadBuffer.Length)
            {
                return new List<int> { 0, 0 };
            }

            int searchLen = searchBuffer.Length;
            int maxLength = lookAheadBuffer.Length - pos;

            for (int start = 0; start < searchLen; start++)
            {
                int length = 0;

                while (length < maxLength &&
                       start + length < searchLen &&
                       searchBuffer[start + length] == lookAheadBuffer[pos + length])
                {
                    length++;
                }

                if (length > bestLength)
                {
                    bestLength = length;
                    bestOffset = searchLen - start;
                }
            }

            return new List<int> { bestOffset, bestLength };
        }

        public List<LZ77Node> Encode(string text)
        {
            List<LZ77Node> encoded = new List<LZ77Node>();
            int pos = 0;

            while (pos < text.Length)
            {
                List<int> numbers = FindMatching(buffer.GetBuffer(), text, pos);
                int offset = numbers[0];
                int length = numbers[1];

                char nextChar = '\0';
                if (pos + length < text.Length)
                {
                    nextChar = text[pos + length];
                }

                encoded.Add(new LZ77Node(offset, length, nextChar));
                string toAdd = text.Substring(pos, length + 1);
                buffer.Shift(toAdd);

                pos += length + 1;
            }

            return encoded;
        }

        public string Decode(List<LZ77Node> encoded)
        {
            StringBuilder result = new StringBuilder();

            foreach (LZ77Node node in encoded)
            {
                if (node.Length > 0)
                {
                    int start = result.Length - node.Offset;
                    for (int i = 0; i < node.Length; i++)
                    {
                        result.Append(result[start + i]);
                    }
                }

                if (node.Next != '\0')
                {
                    result.Append(node.Next);
                }
            }

            return result.ToString();
        }
    }

    public class NodeFunction
    {
        public string ToString(LZ77Compressor.LZ77Node node)
        {
            return $"{node.Offset}{node.Length}{node.Next}";
        }
    }

    public class DeflateCompressor : ICompressor
    {
        private LZ77Compressor lz77 = new LZ77Compressor();
        private HuffmanCompressor huffmanCoder = new HuffmanCompressor();

        public string Encode(string text)
        {
            List<LZ77Compressor.LZ77Node> encodedNodes = lz77.Encode(text);
            StringBuilder encodedText = new StringBuilder();

            foreach (var node in encodedNodes)
            {
                encodedText.AppendFormat("[{0},{1},{2}]",
                    node.Offset,
                    node.Length,
                    node.Next == '\0' ? "\\0" : node.Next.ToString());
            }

            return huffmanCoder.Encode(encodedText.ToString());
        }

        public string Decode(string encodedText)
        {
            string decodedHuffman = huffmanCoder.Decode(encodedText);
            List<LZ77Compressor.LZ77Node> nodes = new List<LZ77Compressor.LZ77Node>();

            int pos = 0;
            while (pos < decodedHuffman.Length)
            {
                if (decodedHuffman[pos] != '[')
                {
                    pos++;
                    continue;
                }

                int endPos = decodedHuffman.IndexOf(']', pos);
                if (endPos == -1) break;

                string nodeStr = decodedHuffman.Substring(pos + 1, endPos - pos - 1);
                string[] parts = nodeStr.Split(',');

                if (parts.Length >= 3)
                {
                    int offset = int.Parse(parts[0]);
                    int length = int.Parse(parts[1]);
                    char next = parts[2] == "\\0" ? '\0' : parts[2][0];

                    nodes.Add(new LZ77Compressor.LZ77Node(offset, length, next));
                }

                pos = endPos + 1;
            }

            return lz77.Decode(nodes);
        }
    }

    public class FileSystemWorker
    {
        public FileSystemWorker() { }

        public bool IsFile(string path)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                bool result = fileInfo.Exists && (fileInfo.Attributes & FileAttributes.Directory) == 0;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in IsFile: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool IsDirectory(string path)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                bool result = dirInfo.Exists && (dirInfo.Attributes & FileAttributes.Directory) != 0;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in IsDirectory: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool IsDirectoryEmpty(string path)
        {
            if (!IsDirectory(path)) return false;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                bool result = !dirInfo.EnumerateFileSystemInfos().Any();
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in IsDirectoryEmpty: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<string> ListElems(string path)
        {
            List<string> allItems = new List<string>();

            try
            {
                if (!Directory.Exists(path))
                {
                    return allItems;
                }

                if (!IsDirectory(path))
                {
                    return allItems;
                }

                allItems.AddRange(Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ListElems: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return allItems;
        }

        public void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in CreateDirectory: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteDirectory(string path)
        {
            if (!IsDirectory(path)) return;

            try
            {
                if (IsDirectoryEmpty(path))
                {
                    Directory.Delete(path);
                }
                else
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in DeleteDirectory: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateFile(string path)
        {
            if (IsDirectory(path))
            {
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fs = File.Create(path)) { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating file: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteFile(string path)
        {
            if (!IsFile(path)) return;

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public struct CompressMethod
    {
        public ushort Deflate;
        public ushort Store;

        public CompressMethod()
        {
            Deflate = 8;
            Store = 0;
        }

        public CompressMethod(ushort deflate, ushort store)
        {
            Deflate = deflate;
            Store = store;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CompressMethod)) return false;
            CompressMethod other = (CompressMethod)obj;
            return Deflate == other.Deflate && Store == other.Store;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Deflate, Store);
        }

        public static bool operator ==(CompressMethod left, CompressMethod right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CompressMethod left, CompressMethod right)
        {
            return !left.Equals(right);
        }
    }

    public struct HeaderFileCD
    {
        public uint Signature;
        public ushort VersionMadeBy;
        public ushort VersionToExtract;
        public ushort Flags;
        public CompressMethod CompressionMethod;
        public ushort LastModTime;
        public ushort LastModDate;
        public uint Crc32;
        public uint CompressedSize;
        public uint UncompressedSize;
        public ushort FileNameLength;
        public ushort ExtraFieldLength;
        public ushort FileCommentLength;
        public ushort DiskNumberStart;
        public ushort InternalFileAttributes;
        public uint ExternalFileAttributes;
        public uint RelativeOffsetOfLocalHeader;

        public HeaderFileCD()
        {
            Signature = 0x02014b50;
        }
    }

    public struct HeaderFileArchive
    {
        public uint Signature;
        public ushort DiskNumber;
        public ushort DiskNumberStart;
        public ushort NumEntriesThisDisk;
        public ushort TotalEntries;
        public uint CentralDirectorySize;
        public uint CentralDirectoryOffset;

        public HeaderFileArchive()
        {
            Signature = 0x06054b50;
        }
    }

    public struct HeaderLocalFile
    {
        public uint Signature;
        public ushort VersionToExtract;
        public ushort Flags;
        public CompressMethod CompressionMethod;
        public ushort LastModTime;
        public ushort LastModDate;
        public uint Crc32;
        public uint CompressedSize;
        public uint UncompressedSize;
        public ushort FileNameLength;
        public ushort ExtraFieldLength;

        public HeaderLocalFile()
        {
            Signature = 0x04034b50;
        }
    }

    public class ZipFile : IDisposable
    {
        private string archivePath;
        private List<HeaderFileCD> centralDirectory = new List<HeaderFileCD>();
        private string rootArchivePath;
        private DeflateCompressor deflateCompressor;
        private FileSystemWorker fsWorker;
        private FileStream archiveStream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private bool isDisposed;

        public ZipFile() { }

        public ZipFile(DeflateCompressor newDeflateCompressor, FileSystemWorker newFsWorker, string path)
        {
            deflateCompressor = newDeflateCompressor;
            fsWorker = newFsWorker;
            archivePath = path;
        }

        public ZipFile(ZipFile other)
        {
            archivePath = other.archivePath;
            centralDirectory = new List<HeaderFileCD>(other.centralDirectory);
            rootArchivePath = other.rootArchivePath;
            deflateCompressor = other.deflateCompressor;
            fsWorker = other.fsWorker;
        }

        public string ArchivePath
        {
            get { return archivePath; }
            set { archivePath = value; }
        }

        public List<HeaderFileCD> CentralDirectory => centralDirectory;

        public void OpenForWriting()
        {
            archiveStream = new FileStream(archivePath, FileMode.Create, FileAccess.ReadWrite);
            writer = new BinaryWriter(archiveStream, Encoding.UTF8);
        }

        public void OpenForReading()
        {
            archiveStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(archiveStream, Encoding.UTF8);
        }

        public void CalculateCRC32(string filePath, out uint crc32, out byte[] fileData)
        {
            crc32 = 0;
            fileData = Array.Empty<byte>();

            if (!fsWorker.IsFile(filePath))
                return;

            try
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    CRC32Calculator crcCalculator = new CRC32Calculator();

                    while ((bytesRead = fs.Read(buffer, 0, bufferSize)) > 0)
                    {
                        crcCalculator.Update(buffer, bytesRead);
                    }

                    crc32 = crcCalculator.GetValue();

                    fs.Seek(0, SeekOrigin.Begin);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        fileData = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating CRC32 for {filePath}: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CompressWithCompression(byte[] data, out byte[] compressedData)
        {
            compressedData = data;

            try
            {
                string text = Encoding.UTF8.GetString(data);
                string compressedText = deflateCompressor.Encode(text);
                compressedData = Encoding.UTF8.GetBytes(compressedText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in compression: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                compressedData = data;
            }
        }

        public void CompressWithoutCompression(byte[] data, out byte[] compressedData)
        {
            compressedData = new byte[data.Length];
            Array.Copy(data, compressedData, data.Length);
        }

        public void WriteLocalFileHeader(string fileName, CompressMethod compressionMethod,
            uint crc32, uint compressedSize, uint uncompressedSize)
        {
            HeaderLocalFile localHeader = new HeaderLocalFile
            {
                VersionToExtract = 20,
                Flags = 0,
                CompressionMethod = compressionMethod,
                LastModTime = GetDOSTime(DateTime.Now),
                LastModDate = GetDOSDate(DateTime.Now),
                Crc32 = crc32,
                CompressedSize = compressedSize,
                UncompressedSize = uncompressedSize,
                FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName),
                ExtraFieldLength = 0
            };

            writer.Write(localHeader.Signature);
            writer.Write(localHeader.VersionToExtract);
            writer.Write(localHeader.Flags);
            writer.Write(localHeader.CompressionMethod.Deflate);
            writer.Write(localHeader.LastModTime);
            writer.Write(localHeader.LastModDate);
            writer.Write(localHeader.Crc32);
            writer.Write(localHeader.CompressedSize);
            writer.Write(localHeader.UncompressedSize);
            writer.Write(localHeader.FileNameLength);
            writer.Write(localHeader.ExtraFieldLength);

            writer.Write(Encoding.UTF8.GetBytes(fileName));
        }

        public void WriteFileData(byte[] data)
        {
            writer.Write(data);
        }

        public void AddToCentralDirectory(string fileName, CompressMethod compressionMethod,
            uint crc32, uint compressedSize, uint uncompressedSize, uint localHeaderOffset)
        {
            HeaderFileCD cdEntry = new HeaderFileCD
            {
                VersionMadeBy = 20,
                VersionToExtract = 20,
                Flags = 0,
                CompressionMethod = compressionMethod,
                LastModTime = GetDOSTime(DateTime.Now),
                LastModDate = GetDOSDate(DateTime.Now),
                Crc32 = crc32,
                CompressedSize = compressedSize,
                UncompressedSize = uncompressedSize,
                FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName),
                ExtraFieldLength = 0,
                FileCommentLength = 0,
                DiskNumberStart = 0,
                InternalFileAttributes = 0,
                ExternalFileAttributes = GetFileAttributes(fileName),
                RelativeOffsetOfLocalHeader = localHeaderOffset
            };

            centralDirectory.Add(cdEntry);
        }

        public void WriteCentralDirectory()
        {
            foreach (var entry in centralDirectory)
            {
                writer.Write(entry.Signature);
                writer.Write(entry.VersionMadeBy);
                writer.Write(entry.VersionToExtract);
                writer.Write(entry.Flags);
                writer.Write(entry.CompressionMethod.Deflate);
                writer.Write(entry.LastModTime);
                writer.Write(entry.LastModDate);
                writer.Write(entry.Crc32);
                writer.Write(entry.CompressedSize);
                writer.Write(entry.UncompressedSize);
                writer.Write(entry.FileNameLength);
                writer.Write(entry.ExtraFieldLength);
                writer.Write(entry.FileCommentLength);
                writer.Write(entry.DiskNumberStart);
                writer.Write(entry.InternalFileAttributes);
                writer.Write(entry.ExternalFileAttributes);
                writer.Write(entry.RelativeOffsetOfLocalHeader);

                string fileName = Path.GetFileName(this.archivePath);
                writer.Write(Encoding.UTF8.GetBytes(fileName));
            }
        }

        public void WriteEndOfCentralDirectory()
        {
            HeaderFileArchive eocd = new HeaderFileArchive
            {
                DiskNumber = 0,
                DiskNumberStart = 0,
                NumEntriesThisDisk = (ushort)centralDirectory.Count,
                TotalEntries = (ushort)centralDirectory.Count,
                CentralDirectorySize = (uint)(centralDirectory.Count * 46),
                CentralDirectoryOffset = (uint)archiveStream.Position
            };

            writer.Write(eocd.Signature);
            writer.Write(eocd.DiskNumber);
            writer.Write(eocd.DiskNumberStart);
            writer.Write(eocd.NumEntriesThisDisk);
            writer.Write(eocd.TotalEntries);
            writer.Write(eocd.CentralDirectorySize);
            writer.Write(eocd.CentralDirectoryOffset);

            ushort commentLength = 0;
            writer.Write(commentLength);
        }

        private ushort GetDOSTime(DateTime dateTime)
        {
            int hour = dateTime.Hour;
            int minute = dateTime.Minute;
            int second = dateTime.Second / 2;

            return (ushort)((hour << 11) | (minute << 5) | second);
        }

        private ushort GetDOSDate(DateTime dateTime)
        {
            int year = dateTime.Year - 1980;
            int month = dateTime.Month;
            int day = dateTime.Day;

            return (ushort)((year << 9) | (month << 5) | day);
        }

        private uint GetFileAttributes(string fileName)
        {
            if (fsWorker.IsFile(fileName))
            {
                return 0x20;
            }
            else if (fsWorker.IsDirectory(fileName))
            {
                return 0x10;
            }
            return 0;
        }

        public void WriteToArchive(string filePath, bool compress = true)
        {
            try
            {
                CalculateCRC32(filePath, out uint crc32, out byte[] fileData);

                byte[] compressedData;
                uint compressedSize;
                CompressMethod method = new CompressMethod();

                if (compress)
                {
                    CompressWithCompression(fileData, out compressedData);
                    compressedSize = (uint)compressedData.Length;
                    method.Deflate = 8;
                }
                else
                {
                    CompressWithoutCompression(fileData, out compressedData);
                    compressedSize = (uint)compressedData.Length;
                    method.Store = 0;
                }

                uint localHeaderOffset = (uint)archiveStream.Position;

                WriteLocalFileHeader(Path.GetFileName(filePath), method,
                    crc32, compressedSize, (uint)fileData.Length);

                WriteFileData(compressedData);

                AddToCentralDirectory(Path.GetFileName(filePath), method,
                    crc32, compressedSize, (uint)fileData.Length, localHeaderOffset);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing file {filePath} to archive: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close()
        {
            if (writer != null)
            {
                WriteCentralDirectory();
                WriteEndOfCentralDirectory();
                writer.Flush();
                writer.Close();
                writer = null;
            }

            if (reader != null)
            {
                reader.Close();
                reader = null;
            }

            if (archiveStream != null)
            {
                archiveStream.Close();
                archiveStream = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Close();
                }
                isDisposed = true;
            }
        }

        ~ZipFile()
        {
            Dispose(false);
        }
    }

    public class CRC32Calculator
    {
        private static readonly uint[] Table = new uint[256];
        private uint crc = 0xFFFFFFFF;

        static CRC32Calculator()
        {
            for (uint i = 0; i < 256; i++)
            {
                uint entry = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ 0xEDB88320;
                    else
                        entry >>= 1;
                }
                Table[i] = entry;
            }
        }

        public void Update(byte[] data, int length)
        {
            for (int i = 0; i < length; i++)
            {
                crc = Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }
        }

        public uint GetValue()
        {
            return crc ^ 0xFFFFFFFF;
        }

        public static uint Calculate(byte[] data)
        {
            CRC32Calculator calculator = new CRC32Calculator();
            calculator.Update(data, data.Length);
            return calculator.GetValue();
        }
    }

    public class ZipArchiver : FileSystemWorker
    {
        private List<string> selectedElements = new List<string>();
        private string currentArchivePath;
        private ZipFile zipFile;

        public ZipArchiver() { }

        public ZipArchiver(List<string> elems, string archivePath, bool compress = true)
        {
            selectedElements = elems;
            currentArchivePath = archivePath;
            InitializeZipFile(compress);
        }


        public ZipArchiver(ZipArchiver other)
        {
            selectedElements = new List<string>(other.selectedElements);
            currentArchivePath = other.currentArchivePath;
            zipFile = other.zipFile;
        }

        private void InitializeZipFile(bool compress)
        {
            DeflateCompressor deflateCompressor = new DeflateCompressor();
            FileSystemWorker fsWorker = new FileSystemWorker();
            zipFile = new ZipFile(deflateCompressor, fsWorker, currentArchivePath);
        }

        public List<string> SelectedElements
        {
            get { return selectedElements; }
            set { selectedElements = value; }
        }

        public string CurrentArchivePath
        {
            get { return currentArchivePath; }
            set { currentArchivePath = value; }
        }

        public void CreateArchive(string archivePath, bool compress = true)
        {
            try
            {
                currentArchivePath = archivePath;
                InitializeZipFile(compress);

                zipFile.OpenForWriting();

                zipFile.WriteToArchive(archivePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create archive: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public void FillingOutArchive(bool compress = true)
        {
            if (zipFile == null || string.IsNullOrEmpty(currentArchivePath))
            {
                return;
            }

            try
            {
                int totalFiles = selectedElements.Count;
                int processed = 0;

                foreach (string element in selectedElements)
                {
                    processed++;
                    int percent = (processed * 100) / totalFiles;

                    if (IsFile(element))
                    {
                        AddFileToArchive(element, compress);
                    }
                    else if (IsDirectory(element))
                    {
                        AddDirectoryToArchive(element, compress);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filling archive: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddFileToArchive(string filePath, bool compress)
        {
            try
            {
                zipFile.WriteToArchive(filePath, compress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding file {filePath}: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddDirectoryToArchive(string directoryPath, bool compress)
        {
            try
            {
                string dirName = Path.GetFileName(directoryPath);
                if (string.IsNullOrEmpty(dirName))
                    dirName = new DirectoryInfo(directoryPath).Name;

                string tempFile = Path.Combine(Path.GetTempPath(), $"dir_{Guid.NewGuid()}.tmp");
                File.WriteAllText(tempFile, "");
                zipFile.WriteToArchive(tempFile, compress);
                File.Delete(tempFile);

                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    AddFileToArchive(file, compress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding directory {directoryPath}: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateCentralCatalog()
        {
            if (zipFile == null)
            {
                return;
            }

            try
            {
                zipFile.WriteCentralDirectory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating central catalog: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateEOCD()
        {
            if (zipFile == null)
            {
                return;
            }

            try
            {
                zipFile.WriteEndOfCentralDirectory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating EOCD: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ExtractArchive(string extractPath = null)
        {
            if (string.IsNullOrEmpty(currentArchivePath) || !File.Exists(currentArchivePath))
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(extractPath))
                {
                    extractPath = Path.Combine(Path.GetDirectoryName(currentArchivePath),
                        Path.GetFileNameWithoutExtension(currentArchivePath));
                }

                CreateDirectory(extractPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting archive: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ListArchiveContents()
        {
            if (string.IsNullOrEmpty(currentArchivePath) || !File.Exists(currentArchivePath))
            {
                return;
            }

            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error listing archive contents: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CloseArchive()
        {
            try
            {
                if (zipFile != null)
                {
                    zipFile.Close();
                    zipFile = null;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing archive: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}