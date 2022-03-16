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
    /// This class implements Export-ZipAuthenticodeSignature
    /// </summary>
    [Cmdlet(VerbsData.Export, "ZipAuthenticodeSignature", DefaultParameterSetName = PathParameterSet)]
    [OutputType(typeof(FileHashInfo))]
    public class ExportZipAuthenticodeSignatureCommand : PSCmdlet
    {
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
                ProcessFile(path);
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
        /// Export zip authenticode signature for a given file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Boolean value indicating whether the hash calculation succeeded or failed.</returns>
        private bool ProcessFile(string path)
        {
            bool success = false;

            try
            {
                string sigFile = path + ".sig.ps1";
                ZipFile zipFile = new ZipFile(path);
                zipFile.ExportSignatureFile(sigFile);
                success = true;
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

            return success;
        }

        /// <summary>
        /// Parameter set names.
        /// </summary>
        private const string PathParameterSet = "Path";
        private const string LiteralPathParameterSet = "LiteralPath";
        private const string StreamParameterSet = "StreamParameterSet";
    }
}
