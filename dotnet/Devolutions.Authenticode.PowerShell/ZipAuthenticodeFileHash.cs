// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security.Cryptography;

using Devolutions.Authenticode;

namespace Devolutions.Authenticode.PowerShell
{
    /// <summary>
    /// FileHashInfo class contains information about a file hash.
    /// </summary>
    public class FileHashInfo
    {
        /// <summary>
        /// Hash algorithm name.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Hash value.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// File path.
        /// </summary>
        public string Path { get; set; }

        public string ToDigestString()
        {
            return (this.Algorithm + ":" + this.Hash).ToLower();
        }
    }

    /// <summary>
    /// This class implements Get-ZipAuthenticodeHash.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ZipAuthenticodeFileHash", DefaultParameterSetName = PathParameterSet)]
    [OutputType(typeof(FileHashInfo))]
    public class GetZipAuthenticodeFileHashCommand : PSCmdlet
    {
        /// <summary>
        /// Algorithm parameter.
        /// The hash algorithm name: "SHA256".
        /// </summary>
        /// <value></value>
        [Parameter(Position = 1)]
        [ValidateSet(HashAlgorithmNames.SHA256)]
        public string Algorithm
        {
            get
            {
                return _Algorithm;
            }

            set
            {
                // A hash algorithm name is case sensitive
                // and always must be in upper case
                _Algorithm = value.ToUpper();
            }
        }

        private string _Algorithm = HashAlgorithmNames.SHA256;

        /// <summary>
        /// Hash algorithm is used.
        /// </summary>
        protected HashAlgorithm hasher;

        /// <summary>
        /// Hash algorithm names.
        /// </summary>
        internal static class HashAlgorithmNames
        {
            public const string SHA256 = "SHA256";
        }

        /// <summary>
        /// Path parameter.
        /// The paths of the files to calculate hash values.
        /// Resolved wildcards.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path
        {
            get
            {
                return _paths;
            }

            set
            {
                _paths = value;
            }
        }

        /// <summary>
        /// LiteralPath parameter.
        /// The literal paths of the files to calculate a hashs.
        /// Don't resolved wildcards.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = LiteralPathParameterSet, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath", "LP")]
        public string[] LiteralPath
        {
            get
            {
                return _paths;
            }

            set
            {
                _paths = value;
            }
        }

        private string[] _paths;

        /// <summary>
        /// InputStream parameter.
        /// The stream of the file to calculate a hash.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = StreamParameterSet, Position = 0)]
        public Stream InputStream { get; set; }

        /// <summary>
        /// BeginProcessing() override.
        /// This is for hash function init.
        /// </summary>
        protected override void BeginProcessing()
        {

        }

        /// <summary>
        /// ProcessRecord() override.
        /// This is for paths collecting from pipe.
        /// </summary>
        protected override void ProcessRecord()
        {
            List<string> pathsToProcess = new();
            ProviderInfo provider = null;

            switch (ParameterSetName)
            {
                case PathParameterSet:
                    // Resolve paths and check existence
                    foreach (string path in _paths)
                    {
                        try
                        {
                            Collection<string> newPaths = this.GetResolvedProviderPathFromPSPath(path, out provider);
                            if (newPaths != null)
                            {
                                pathsToProcess.AddRange(newPaths);
                            }
                        }
                        catch (ItemNotFoundException e)
                        {
                            if (!WildcardPattern.ContainsWildcardCharacters(path))
                            {
                                ErrorRecord errorRecord = new(e,
                                    "FileNotFound",
                                    ErrorCategory.ObjectNotFound,
                                    path);
                                WriteError(errorRecord);
                            }
                        }
                    }

                    break;
                case LiteralPathParameterSet:
                    foreach (string path in _paths)
                    {
                        string newPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                        pathsToProcess.Add(newPath);
                    }

                    break;
            }

            foreach (string path in pathsToProcess)
            {
                if (ComputeFileHash(path, out string hash))
                {
                    WriteHashResult(Algorithm, hash, path);
                }
            }
        }

        /// <summary>
        /// Perform common error checks.
        /// Populate source code.
        /// </summary>
        protected override void EndProcessing()
        {
            if (ParameterSetName == StreamParameterSet)
            {
                byte[] bytehash = null;
                string hash = null;

                bytehash = hasher.ComputeHash(InputStream);

                hash = BitConverter.ToString(bytehash).Replace("-", string.Empty);
                WriteHashResult(Algorithm, hash, string.Empty);
            }
        }

        /// <summary>
        /// Read the file and calculate the hash.
        /// </summary>
        /// <param name="path">Path to file which will be hashed.</param>
        /// <param name="hash">Will contain the hash of the file content.</param>
        /// <returns>Boolean value indicating whether the hash calculation succeeded or failed.</returns>
        private bool ComputeFileHash(string path, out string hash)
        {
            byte[] bytehash = null;

            hash = null;

            try
            {
                ZipFile zipFile = new ZipFile(path);
                bytehash = zipFile.ComputeHash();
                hash = BitConverter.ToString(bytehash).Replace("-", string.Empty);
            }
            catch (FileNotFoundException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    path);
                WriteError(errorRecord);
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "UnauthorizedAccessError",
                    ErrorCategory.InvalidData,
                    path);
                WriteError(errorRecord);
            }
            catch (IOException ioException)
            {
                var errorRecord = new ErrorRecord(
                    ioException,
                    "FileReadError",
                    ErrorCategory.ReadError,
                    path);
                WriteError(errorRecord);
            }
            finally
            {

            }

            return hash != null;
        }

        /// <summary>
        /// Create FileHashInfo object and output it.
        /// </summary>
        private void WriteHashResult(string Algorithm, string hash, string path)
        {
            FileHashInfo result = new();
            result.Algorithm = Algorithm;
            result.Hash = hash;
            result.Path = path;
            WriteObject(result);
        }

        /// <summary>
        /// Parameter set names.
        /// </summary>
        private const string PathParameterSet = "Path";
        private const string LiteralPathParameterSet = "LiteralPath";
        private const string StreamParameterSet = "StreamParameterSet";
    }
}
