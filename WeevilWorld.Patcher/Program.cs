using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using LZ4;
using uTinyRipper;
using uTinyRipper.BundleFiles;
using uTinyRipper.Lz4;

namespace WeevilWorld.Patcher
{
    public static class Program
    {
        public static readonly string s_unityInstallDir = @"D:\Software\Unity\Hub\Editor\2019.1.0f2";
        
        private record StorageBlockRecord(uint m_uncompressedSize, uint m_compressedSize, StorageBlockFlags m_flags);

        private record FileEntryRecord
        {
            public readonly string m_name;
            public readonly string m_nameOrigin;
            public long m_size;
            public long m_offset;
            
            public FileEntryRecord(FileEntry entry)
            {
                m_name = entry.Name;
                m_nameOrigin = entry.NameOrigin;
                m_size = entry.Size;
                m_offset = entry.Offset;
            }
        }

        public static void Main(string[] args)
        {
            //CompressUnityWebFile(Path.GetFullPath("bundletemp.bin"), Path.GetFullPath("bundletemp_brotli.bin"));

            //var testReparse = GameCollection.LoadScheme(@"bundletemp.bin", "bundletemp.bin");
            //var testReparse2 = GameCollection.LoadScheme(@"temp\04.data.unityweb", "04.data.unityweb");
            
            //var testReparse3 = GameStructure.Load(new []{@"bundletemp.bin"});
            
            //return;

            Console.WriteLine("Hello World!");

            var inputDirectory = @"D:\re\bw\archive\cdn.binw.net\WeevilWorld\v268\Build";
            var outputDirectory = @"D:\re\bw\ZingyWeevils\WeevilWorld.Server\wwwroot\WeevilWorld\v268\Build\";
            
            const string tempDir = "temp";
            Directory.CreateDirectory(tempDir);

            foreach (var inputFile in Directory.GetFiles(inputDirectory, "*.unityweb"))
            {
                var inputFileName = Path.GetFileName(inputFile);

                byte[] decompressedWebFile;
                using (var inputStream = File.OpenRead(inputFile))
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress)) 
                using (var decompressOutputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(decompressOutputStream);
                    decompressedWebFile = decompressOutputStream.ToArray();
                }

                if (inputFile.EndsWith(".data.unityweb"))
                {
                    var webFile = WebFile.ReadScheme(decompressedWebFile, "");

                    var entries = new FileEntryRecord[webFile.Metadata.Entries.Length];
                    for (var i = 0; i < webFile.Metadata.Entries.Length; i++)
                    {
                        var fileEntry = webFile.Metadata.Entries[i];
                        entries[i] = new FileEntryRecord(fileEntry);
                    }

                    for (var i = 0; i < webFile.Metadata.Entries.Length; i++)
                    {
                        var fileEntry = entries[i];
                        
                        Debug.Assert(fileEntry.m_offset <= int.MaxValue);
                        Debug.Assert(fileEntry.m_size <= int.MaxValue);
                        var fileEntrySpan = new Span<byte>(decompressedWebFile, (int) fileEntry.m_offset, (int) fileEntry.m_size);

                        if (fileEntry.m_name == "il2cppdata/metadata/global-metadata.dat")
                        {
                            PatchGlobalMetadata(fileEntrySpan);
                        } else if (fileEntry.m_name == "data.unity3d")
                        {
                            var newUnityFSBytes = PatchUnityFS(webFile, fileEntry, decompressedWebFile);
                            var deltaSize = newUnityFSBytes.Length - fileEntry.m_size;

                            var newDecompressedBytes = new byte[decompressedWebFile.Length + deltaSize];
                            Array.Copy(decompressedWebFile, 0, newDecompressedBytes, 0, fileEntry.m_offset); // copy bytes before this file
                            Array.Copy(newUnityFSBytes, 0, newDecompressedBytes, fileEntry.m_offset, newUnityFSBytes.Length); // copy this file
                            Array.Copy(decompressedWebFile, fileEntry.m_offset+fileEntry.m_size, newDecompressedBytes, fileEntry.m_offset + newUnityFSBytes.Length, decompressedWebFile.Length-fileEntry.m_offset-fileEntry.m_size); // copy data after this file

                            fileEntry.m_size = newUnityFSBytes.Length;
                            for (var j = i+1; j < webFile.Metadata.Entries.Length; j++)
                            {
                                entries[j].m_offset += deltaSize;
                            }
                            
                            decompressedWebFile = newDecompressedBytes;
                        }
                    }

                    var entriesDataOffset = "UnityWebData1.0".Length + 1 + 4; // header+0term + sizeof(entriesCount)
                    var entriesDataSpan = new Span<byte>(decompressedWebFile).Slice(entriesDataOffset);
                    for (var i = 0; i < entries.Length; i++)
                    {
                        var entry = entries[i];
                        BinaryPrimitives.WriteUInt32LittleEndian(entriesDataSpan, (uint)entry.m_offset);
                        BinaryPrimitives.WriteUInt32LittleEndian(entriesDataSpan.Slice(4), (uint)entry.m_size);

                        var nameOffset = 8 + 4; // sizeof(offset) + sizeof(size) + sizeof(name len)
                        var nameSpan = entriesDataSpan.Slice(nameOffset, entry.m_nameOrigin.Length);
                        var nameRead = Encoding.UTF8.GetString(nameSpan);

                        if (entry.m_nameOrigin != nameRead)
                        {
                            throw new Exception("writing entries misaligned");
                        }

                        entriesDataSpan = entriesDataSpan.Slice(nameOffset + nameSpan.Length);
                    }
                }

                var patchedFile = Path.GetFullPath(Path.Combine(tempDir, inputFileName));
                using (var patchedFileStream = File.OpenWrite(patchedFile))
                {
                    patchedFileStream.SetLength(0);
                    patchedFileStream.Write(decompressedWebFile);
                }

                var outputFile = Path.Combine(outputDirectory, inputFileName);
                CompressUnityWebFile(patchedFile, outputFile);
            }

            //var scheme = (WebFileScheme) GameCollection.ReadScheme(bytes, fn, Path.GetFileName(fn));

            //foreach (var subScheme in scheme.Schemes)
            //{
            //    if (subScheme is not BundleFileScheme bundle) continue;
            //    
            //}

            //using var stream = new MemoryStream(bytes, false);
            //foreach (var fileEntry in scheme.Metadata.Entries)
            //{
            //    stream.Position = fileEntry.Offset;
            //    var fileBuffer = new byte[fileEntry.Size];
            //    stream.Read(fileBuffer);
            //    var filePath = Path.Combine("webfile_out", fileEntry.Name);
            //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            //    using var outputStream = File.OpenWrite(filePath);
            //    outputStream.SetLength(0);
            //    outputStream.Write(fileBuffer);
            //}
        }

        private static byte[] PatchUnityFS(WebFileScheme webFile, FileEntryRecord fileEntry, byte[] decompressedBytes)
        {
            var bundleSchema = (BundleFileScheme) webFile.Schemes.Single(x => x.Name == fileEntry.m_name);

            var storageBlocks = new List<StorageBlockRecord>();

            var decompressedStreamSize = 0u;
            for (var i = 0; i < bundleSchema.Metadata.BlocksInfo.StorageBlocks.Length; i++)
            {
                var block = bundleSchema.Metadata.BlocksInfo.StorageBlocks[i];
                decompressedStreamSize += block.UncompressedSize;
            }
            
            var decompressedStream = new byte[decompressedStreamSize];
            var rawDataOffset = (uint) bundleSchema.Header.FileStream.CompressedBlocksInfoSize + 49u; // + hdr size
            var decompressedDataOffset = 0u;
            for (var i = 0; i < bundleSchema.Metadata.BlocksInfo.StorageBlocks.Length; i++)
            {
                var block = bundleSchema.Metadata.BlocksInfo.StorageBlocks[i];
                var compressType = block.Flags.GetCompression();
                
                switch (compressType)
                {
                    case CompressionType.Lz4:
                    case CompressionType.Lz4HC:
                        using (var lzStream = new Lz4DecodeStream(decompressedBytes, (int) rawDataOffset + (int) fileEntry.m_offset, (int) block.CompressedSize))
                        {
                            lzStream.ReadBuffer(decompressedStream, (int)decompressedDataOffset, (int) block.UncompressedSize);
                        }

                        break;
                    default:
                        throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
                }
                rawDataOffset += block.CompressedSize;
                decompressedDataOffset += block.UncompressedSize;
            }

            for (var i = 0; i < bundleSchema.Metadata.DirectoryInfo.Nodes.Length; i++)
            {
                var node = bundleSchema.Metadata.DirectoryInfo.Nodes[i];
                var nodeSpan = new Span<byte>(decompressedStream, (int)node.Offset, (int)node.Size);
                if (node.Path == "globalgamemanagers")
                {
                    var globalGameManagers = (SerializedFileScheme)bundleSchema.Schemes[i];
                    foreach (var objectInfo in globalGameManagers.Metadata.Object)
                    {
                        if (objectInfo.ClassID == ClassIDType.UnityConnectSettings)
                        {
                            var unityConnectOffset = objectInfo.ByteStart + globalGameManagers.Header.DataOffset;
                            var unityConnectData = nodeSpan.Slice((int)unityConnectOffset, objectInfo.ByteSize);
                            
                            unityConnectData.Fill(0); // disable all tracking
                        }
                    }
                }
            }
            
            decompressedDataOffset = 0;
            using var bundleDataStream = new MemoryStream();
            for (var i = 0; i < bundleSchema.Metadata.BlocksInfo.StorageBlocks.Length; i++)
            {
                var block = bundleSchema.Metadata.BlocksInfo.StorageBlocks[i];
                
                var compressed = LZ4Codec.EncodeHC(decompressedStream, (int) decompressedDataOffset, (int) block.UncompressedSize);
                bundleDataStream.Write(compressed);
                storageBlocks.Add(new StorageBlockRecord(block.UncompressedSize, (uint) compressed.Length, (StorageBlockFlags)CompressionType.Lz4HC));
                
                // todo: use heuristic to get better brotli size?
                // var blockSlice = new ReadOnlySpan<byte>(decompressedStream).Slice((int)decompressedDataOffset, (int)block.UncompressedSize);
                // bundleDataStream.Write(blockSlice);
                // storageBlocks.Add(new StorageBlockRecord(block.UncompressedSize, block.UncompressedSize, 0));

                decompressedDataOffset += block.UncompressedSize;
            }

            using var bundleMetadataStream = new MemoryStream();
            using var bundleMetadataWriter = new EndianWriter(bundleMetadataStream, EndianType.BigEndian);
            bundleMetadataWriter.Write(new byte[16]);
            bundleMetadataWriter.Write((uint)storageBlocks.Count);
            foreach (var storageBlock in storageBlocks)
            {
                bundleMetadataWriter.Write(storageBlock.m_uncompressedSize);
                bundleMetadataWriter.Write(storageBlock.m_compressedSize);
                bundleMetadataWriter.Write((ushort)storageBlock.m_flags);
            }
            bundleMetadataWriter.Write((uint)bundleSchema.Metadata.DirectoryInfo.Nodes.Length);
            foreach (var node in bundleSchema.Metadata.DirectoryInfo.Nodes)
            {
                bundleMetadataWriter.Write(node.Offset);
                bundleMetadataWriter.Write(node.Size);
                bundleMetadataWriter.Write(node.BlobIndex);
                bundleMetadataWriter.WriteStringZeroTerm(node.PathOrigin);
            }
            
            using var bundleStream = new MemoryStream();
            using var bundleWriter = new EndianWriter(bundleStream, EndianType.BigEndian);
            bundleWriter.WriteStringZeroTerm("UnityFS");
            bundleWriter.Write((uint) bundleSchema.Header.Version);
            bundleWriter.WriteStringZeroTerm(bundleSchema.Header.UnityWebBundleVersion);
            bundleWriter.WriteStringZeroTerm(bundleSchema.Header.UnityWebMinimumRevision.ToString());
            var fullFileLengthPos = bundleWriter.BaseStream.Position;
            bundleWriter.Write(0ul); // Size - full file length
            bundleWriter.Write((uint)bundleMetadataStream.Length); // CompressedBlocksInfoSize
            bundleWriter.Write((uint)bundleMetadataStream.Length); // UncompressedBlocksInfoSize
            bundleWriter.Write((uint)BundleFlags.BlocksAndDirectoryInfoCombined); // Flags
            bundleWriter.Write(bundleMetadataStream.ToArray());
            bundleWriter.Write(bundleDataStream.ToArray());

            bundleWriter.BaseStream.Position = fullFileLengthPos; // fix the full file size
            bundleWriter.Write((ulong)bundleWriter.BaseStream.Length);

            using var bundleOutTemp = File.OpenWrite("bundletemp.bin");
            bundleOutTemp.SetLength(0);
            bundleStream.Position = 0;
            bundleStream.CopyTo(bundleOutTemp);
            
            bundleStream.Position = 0;
            return bundleStream.ToArray();
        }
        
        private static void PatchGlobalMetadata(Span<byte> fileEntrySpan)
        {
            // todo: hmm
            PatchDomain("cdn.binw.net", "ww.zingy.dev", fileEntrySpan);
            PatchDomain("www.weevilworld.com", "ww.zingy.dev/play__", fileEntrySpan);
            PatchDomain("ad.weevilworld.com", "ww.zingy.dev/ad___", fileEntrySpan);
        }

        private static void PatchDomain(string from, string to, Span<byte> fileEntrySpan)
        {
            var fromBytes = Encoding.ASCII.GetBytes(from);
            var toBytes = Encoding.ASCII.GetBytes(to);
            if (fromBytes.Length != toBytes.Length) throw new InvalidDataException();

            var index = 0;
            while (true)
            {
                var indexRel = fileEntrySpan.Slice(index).IndexOf(fromBytes);
                if (indexRel == -1) break;
                index += indexRel;

                toBytes.CopyTo(fileEntrySpan.Slice(index));
            }
        }

        private static void CompressUnityWebFile(string inputPath, string outputPath)
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
            
            Console.Out.WriteLine($"Compressing {Path.GetFileName(outputPath)}");
            
            // https://unitycoder.com/blog/2021/04/07/compress-webgl-build-manually-from-command-line-brotli/
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(s_unityInstallDir, @"Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\Emscripten_Win\python\2.7.5.3_64bit\python.exe"),
                Arguments = Path.Combine(s_unityInstallDir, string.Join(" ", new []
                {
                    @"Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\Brotli\python\bro.py",
                    $"-i \"{inputPath}\"",
                    $"-o \"{outputPath}\"",
                    "--comment \"UnityWeb Compressed Content (brotli)\""
                })),
                Environment =
                {
                    {"PYTHONPATH", Path.Combine(s_unityInstallDir, @"Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\Brotli\dist\Brotli-0.4.0-py2.7-win-amd64.egg")}
                }
            });
            process.WaitForExit();
        }
    }
}
