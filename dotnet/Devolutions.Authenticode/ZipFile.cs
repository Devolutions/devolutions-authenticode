using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Devolutions.Authenticode;

namespace Devolutions.Authenticode
{
    public class ZipFile
    {
        // https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT

        public const uint ZipLocalFileHeaderSignature = 0x04034b50;
        public const uint ZipLocalFileHeaderSize = 30;

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 30)]
        public struct ZipLocalFileHeader
        {
            public uint signature;
            public ushort version;
            public ushort bitflags;
            public ushort compressionMethod;
            public ushort lastModFileTime;
            public ushort lastModFileDate;
            public uint crc32;
            public uint compressedSize;
            public uint uncompressedSize;
            public ushort fileNameLength;
            public ushort extraFieldLength;
        }

        public const uint ZipCentralFileHeaderSignature = 0x02014b50;
        public const uint ZipCentralFileHeaderSize = 46;

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 46)]
        public struct ZipCentralFileHeader
        {
            public uint signature;
            public ushort versionUsed;
            public ushort versionRequired;
            public ushort bitflags;
            public ushort compressionMethod;
            public ushort lastModeFileTime;
            public ushort lastModFileDate;
            public uint crc32;
            public uint compressedSize;
            public uint uncompressedSize;
            public ushort fileNameLength;
            public ushort extraFieldLength;
            public ushort fileCommentLength;
            public ushort diskNumberStart;
            public ushort internalFileAttributes;
            public uint externalFileAttributes;
            public uint relativeOffsetOfLocalHeader;
        }

        public const uint ZipEndOfCentralDirHeaderSignature = 0x06054b50;
        public const uint ZipEndOfCentralDirHeaderSize = 22;

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 22)]
        public struct ZipEndOfCentralDirHeader
        {
            public uint signature;
            public ushort diskNumberCurrent;
            public ushort diskNumberCentral;
            public ushort diskEntryCountCurrent;
            public ushort diskEntryCountCentral;
            public uint centralDirSize;
            public uint centralDirOffset;
            public ushort fileCommentLength;
        }

        public byte[] data;

        private long GetLocalFileRecordSize(byte[] data, long offset)
        {
            long size = data.Length;
            long headerSize = ZipLocalFileHeaderSize;

            unsafe
            {
                if ((offset + headerSize) > size)
                {
                    return -1;
                }

                fixed (byte* ptr = data)
                {
                    ZipLocalFileHeader* hdr = (ZipLocalFileHeader*) &ptr[offset];
                    long recordSize = sizeof(ZipLocalFileHeader) + hdr->compressedSize + hdr->fileNameLength + hdr->extraFieldLength;

                    if ((offset + recordSize) > size)
                    {
                        return -1;
                    }

                    return recordSize;
                }
            }
        }

        private long GetCentralFileRecordSize(byte[] data, long offset)
        {
            long size = data.Length;
            long headerSize = ZipCentralFileHeaderSize;

            unsafe
            {
                if ((offset + headerSize) > size)
                {
                    return -1;
                }

                fixed (byte* ptr = data)
                {
                    ZipCentralFileHeader* hdr = (ZipCentralFileHeader*) &ptr[offset];
                    long recordSize = sizeof(ZipCentralFileHeader) + hdr->fileNameLength + hdr->extraFieldLength;

                    if ((offset + recordSize) > size)
                    {
                        return -1;
                    }

                    return recordSize;
                }
            }
        }

        private long GetEndOfCentralDirRecordSize(byte[] data, long offset)
        {
            long size = data.Length;
            long headerSize = ZipEndOfCentralDirHeaderSize;

            unsafe
            {
                if ((offset + headerSize) > size)
                {
                    return -1;
                }

                fixed (byte* ptr = data)
                {
                    ZipEndOfCentralDirHeader* hdr = (ZipEndOfCentralDirHeader*)&ptr[offset];
                    long recordSize = sizeof(ZipEndOfCentralDirHeader) + hdr->fileCommentLength;

                    if ((offset + recordSize) > size)
                    {
                        return -1;
                    }

                    return recordSize;
                }
            }
        }

        private long FindZipFooterOffset(byte[] data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    long offset = 0;
                    long size = data.Length;
                    long recordSize = 0;

                    while (offset < (size - 4))
                    {
                        uint hdrSignature = *((uint*)&ptr[offset]);

                        if (hdrSignature == ZipLocalFileHeaderSignature)
                        {
                            recordSize = GetLocalFileRecordSize(data, offset);

                            if (recordSize < 0)
                            {
                                throw new InvalidDataException("Invalid zip local file record");
                            }

                            offset += recordSize;
                        }
                        else if (hdrSignature == ZipCentralFileHeaderSignature)
                        {
                            recordSize = GetCentralFileRecordSize(data, offset);

                            if (recordSize < 0)
                            {
                                throw new InvalidDataException("Invalid zip central file record");
                            }

                            offset += recordSize;
                        }
                        else if (hdrSignature == ZipEndOfCentralDirHeaderSignature)
                        {
                            recordSize = GetEndOfCentralDirRecordSize(data, offset);

                            if (recordSize < 0)
                            {
                                throw new InvalidDataException("Invalid end of zip central dir record");
                            }

                            return offset;
                        }
                        else
                        {
                            string message = String.Format("Unhandled zip record header 0x{0:X}", hdrSignature);
                            throw new InvalidDataException(message);
                        }
                    }

                    throw new InvalidDataException("Count not parse zip file");
                }
            }
        }

        private string? GetFileComment(byte[] data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    long offset = FindZipFooterOffset(data);

                    if (offset < 0)
                        return null;

                    ZipEndOfCentralDirHeader* hdr = (ZipEndOfCentralDirHeader*)&ptr[offset];

                    if (hdr->signature != ZipEndOfCentralDirHeaderSignature)
                        return null;

                    if (hdr->fileCommentLength < 0)
                        return null;

                    offset += sizeof(ZipEndOfCentralDirHeader);

                    return Encoding.UTF8.GetString(&ptr[offset], hdr->fileCommentLength);
                }
            }
        }

        public string? SetFileComment(string? comment)
        {
            comment = comment ?? String.Empty;
            string oldComment = String.Empty;

            long fileCommentOffset = 0;
            byte[] newCommentBytes = Encoding.UTF8.GetBytes(comment);
            ushort newCommentLength = (ushort)newCommentBytes.Length;

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    long offset = FindZipFooterOffset(data);

                    if (offset < 0)
                        return null;

                    ZipEndOfCentralDirHeader* hdr = (ZipEndOfCentralDirHeader*)&ptr[offset];

                    if (hdr->signature != ZipEndOfCentralDirHeaderSignature)
                        return null;

                    offset += sizeof(ZipEndOfCentralDirHeader);

                    fileCommentOffset = offset;

                    if (hdr->fileCommentLength > 0)
                    {
                        oldComment = Encoding.UTF8.GetString(&ptr[fileCommentOffset], (int) hdr->fileCommentLength);
                    }

                    hdr->fileCommentLength = newCommentLength;
                }
            }

            int newFileSize = (int)fileCommentOffset + newCommentBytes.Length;

            if (data.Length != newFileSize)
            {
                Array.Resize(ref data, newFileSize);
            }

            Array.Copy(newCommentBytes, 0, data, fileCommentOffset, newCommentLength);

            return oldComment;
        }

        public string GetDigestString()
        {
            // https://github.com/opencontainers/image-spec/blob/v1.0.1/descriptor.md#digests

            using (SHA256 sha256 = SHA256.Create())
            {
                long offset = FindZipFooterOffset(data);
                offset += ZipEndOfCentralDirHeaderSize;
                byte[] tbsData = new byte[offset];
                Array.Copy(data, 0, tbsData, 0, offset);
                tbsData[offset - 1] = 0;
                tbsData[offset - 2] = 0;
                byte[] result = sha256.ComputeHash(tbsData);
                return "sha256:" + BitConverter.ToString(result).Replace("-", string.Empty).ToLower();
            }
        }

        public byte[] ComputeHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                long offset = FindZipFooterOffset(data);
                offset += ZipEndOfCentralDirHeaderSize;
                byte[] tbsData = new byte[offset];
                Array.Copy(data, 0, tbsData, 0, offset);
                tbsData[offset - 1] = 0;
                tbsData[offset - 2] = 0;
                byte[] result = sha256.ComputeHash(tbsData);
                return result;
            }
        }

        public string? GetSignatureString()
        {
            string? fileComment = GetFileComment(data);

            if (fileComment == null)
                return null;

            string[] commentLines = fileComment.Split('\n');

            for (int i = 0; i < commentLines.Length; i++)
            {
                string commentLine = commentLines[i].Trim();
                
                if (commentLine.StartsWith("ZipAuthenticode"))
                {
                    return commentLine;
                }
            }

            return null;
        }

        public string? GetSignatureFileData()
        {
            string? sigCommentLine = GetSignatureString();

            if (sigCommentLine == null)
                return null;

            return SaveSignatureData(sigCommentLine);
        }

        public void ExportSignatureFile(string filename)
        {
            string? sigCommentLine = GetSignatureString();

            if (sigCommentLine != null)
            {
                SaveSignatureFile(filename, sigCommentLine);
            }
        }

        public static string LoadSignatureFile(string filename, out string digest, out string block)
        {
            bool inSigBlock = false;
            string[] lines = File.ReadAllLines(filename);

            if (lines.Length < 4)
            {
                throw new InvalidDataException("Invalid zip signature file!");
            }

            digest = lines[0].Trim();

            StringBuilder sb = new StringBuilder();

            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index];

                if (line.StartsWith("# "))
                {
                    if (line.StartsWith("# SIG # Begin signature block"))
                    {
                        inSigBlock = true;
                    }
                    else if (line.StartsWith("# SIG # End signature block"))
                    {
                        inSigBlock = false;
                        break;
                    }
                    else if (inSigBlock)
                    {
                        sb.Append(line, 2, line.Length - 2);
                    }
                }
            }


            block = sb.ToString();

            sb = new StringBuilder();
            sb.Append("ZipAuthenticode=");
            sb.Append(digest);
            sb.Append(",");
            sb.Append(block);
            return sb.ToString();
        }

        public static void SplitSignatureCommentLine(string sigCommentLine, out string digest, out string block)
        {
            if (!sigCommentLine.StartsWith("ZipAuthenticode=sha256:") || (sigCommentLine.Length < 89))
            {
                throw new InvalidDataException();
            }

            digest = sigCommentLine.Substring(16, 71);
            block = sigCommentLine.Substring(88, sigCommentLine.Length - 88);
        }

        public static string SaveSignatureData(string sigCommentLine)
        {
            string sigDigest = string.Empty;
            string sigBlock = string.Empty;

            SplitSignatureCommentLine(sigCommentLine, out sigDigest, out sigBlock);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(sigDigest);

            int index = 0;
            int length = sigBlock.Length;

            sb.AppendLine("# SIG # Begin signature block");

            while (index < length)
            {
                int chunk = 64;

                if (length < (index + 64))
                {
                    chunk = length - index;
                }

                sb.AppendLine("# " + sigBlock.Substring(index, chunk));

                index += chunk;
            }

            sb.AppendLine("# SIG # End signature block");

            return sb.ToString();
        }

        public static void SaveSignatureFile(string filename, string sigCommentLine)
        {
            string sigFileData = SaveSignatureData(sigCommentLine);
            File.WriteAllBytes(filename, Encoding.UTF8.GetBytes(sigFileData));
        }

        public void Save(string filename)
        {
            File.WriteAllBytes(filename, data);
        }

        public ZipFile(string filename)
        {
            data = File.ReadAllBytes(filename);
            string comment = GetFileComment(data) ?? string.Empty;
        }

        public static Authenticode.Signature Sign(Authenticode.SigningOption option, string fileName,
            X509Certificate2 certificate, string timeStampServerUrl, string hashAlgorithm)
        {
            Authenticode.Signature signature = null;
            string sigFileName = fileName + ".sig.ps1";

            ZipFile zipFile = new ZipFile(fileName);
            string zipDigest = zipFile.GetDigestString();
            File.WriteAllBytes(sigFileName, Encoding.UTF8.GetBytes(zipDigest));

            signature = Authenticode.SignatureHelper.SignFile(option, sigFileName, certificate, timeStampServerUrl, hashAlgorithm);

            string sigDigest = string.Empty;
            string sigBlock = string.Empty;
            string sigCommentLine = ZipFile.LoadSignatureFile(sigFileName, out sigDigest, out sigBlock);

            zipFile.SetFileComment(sigCommentLine);
            zipFile.Save(fileName);

            return signature;
        }

        public static Authenticode.Signature GetSignature(string fileName)
        {
            Authenticode.Signature signature = null;
            string sigFileName = fileName + ".sig.ps1";

            ZipFile zipFile = new ZipFile(fileName);
            string zipDigest = zipFile.GetDigestString();

            string? sigCommentLine = zipFile.GetSignatureString();

            string sigDigest = string.Empty;
            string sigBlock = string.Empty;
            SplitSignatureCommentLine(sigCommentLine, out sigDigest, out sigBlock);

            string sigFileData = zipFile.GetSignatureFileData() ?? String.Empty;
            signature = Authenticode.SignatureHelper.GetSignature(".sig.ps1", sigFileData);

            if (!sigDigest.Equals(zipDigest))
            {
                throw new Exception("zip digest mismatch!");
            }

            return signature;
        }

        public static string GetZipDigestString(string filename)
        {
            ZipFile zipFile = new ZipFile(filename);
            return zipFile.GetDigestString();
        }
    }
}
