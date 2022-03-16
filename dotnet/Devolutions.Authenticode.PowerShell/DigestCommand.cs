// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security.Cryptography;

using Devolutions.Authenticode;

namespace Devolutions.Authenticode.PowerShell
{
    /// <summary>
    /// DigestInfo class contains information about a file hash.
    /// </summary>
    public class DigestInfo
    {
        /// <summary>
        /// Digest string.
        /// </summary>
        public string Digest { get; set; }

        /// <summary>
        /// File path.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// This class implements Get-ZipAuthenticodeDigest.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ZipAuthenticodeDigest", DefaultParameterSetName = PathParameterSet)]
    [OutputType(typeof(DigestInfo))]
    public class GetZipAuthenticodeDigestCommand : PSCmdlet
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
        /// Export digest string to signature file.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = false)]
        public SwitchParameter Export
        {
            get { return _export; }
            set { _export = value; }
        }
        private bool _export = false;

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
                if (ComputeDigest(path, out string digest))
                {
                    WriteDigestResult(digest, path);
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

            }
        }

        /// <summary>
        /// Read the file and calculate the hash.
        /// </summary>
        /// <param name="path">Path to file which will be hashed.</param>
        /// <param name="hash">Will contain the hash of the file content.</param>
        /// <returns>Boolean value indicating whether the hash calculation succeeded or failed.</returns>
        private bool ComputeDigest(string path, out string digest)
        {
            digest = null;

            try
            {
                ZipFile zipFile = new ZipFile(path);
                digest = zipFile.GetDigestString();

                if (_export)
                {
                    string sigFile = path + ".sig.ps1";
                    File.WriteAllBytes(sigFile, Encoding.UTF8.GetBytes(digest));
                }
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

            return digest != null;
        }

        /// <summary>
        /// Create DigestInfo object and output it.
        /// </summary>
        private void WriteDigestResult(string digest, string path)
        {
            DigestInfo result = new();
            result.Digest = digest;
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
