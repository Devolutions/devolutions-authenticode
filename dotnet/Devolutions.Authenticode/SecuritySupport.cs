// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable 1634, 1691
#pragma warning disable 56523

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using DWORD = System.UInt32;

namespace Devolutions.Authenticode
{
    public static class SecuritySupport
    {
        /// <summary>
        /// Throw if file does not exist.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>Does not return a value.</returns>
        internal static void CheckIfFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
        }

        private static bool CertHasOid(X509Certificate2 c, string oid)
        {
            foreach (var extension in c.Extensions)
            {
                if (extension is X509EnhancedKeyUsageExtension ext)
                {
                    foreach (Oid ekuOid in ext.EnhancedKeyUsages)
                    {
                        if (ekuOid.Value == oid)
                        {
                            return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Check to see if the specified cert is suitable to be
        /// used as a code signing cert.
        /// </summary>
        /// <param name="c">Certificate object.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool CertIsGoodForSigning(X509Certificate2 c)
        {
            if (!c.HasPrivateKey)
            {
                return false;
            }

            return CertHasOid(c, CertificateFilterInfo.CodeSigningOid);
        }

        /// <summary>
        /// Convert an int to a DWORD.
        /// </summary>
        /// <param name="n">Signed int number.</param>
        /// <returns>DWORD.</returns>
        internal static DWORD GetDWORDFromInt(int n)
        {
            UInt32 result = BitConverter.ToUInt32(BitConverter.GetBytes(n), 0);
            return (DWORD)result;
        }

        /// <summary>
        /// Convert a DWORD to int.
        /// </summary>
        /// <param name="n">Number.</param>
        /// <returns>Int.</returns>
        internal static int GetIntFromDWORD(DWORD n)
        {
            Int64 n64 = n - 0x100000000L;
            return (int)n64;
        }

        /// <summary>
        /// Defines the valid purposes by which
        /// we can filter certificates.
        /// </summary>
        internal enum CertificatePurpose
        {
            /// <summary>
            /// Certificates where a purpose has not been specified.
            /// </summary>
            NotSpecified = 0,

            /// <summary>
            /// Certificates that can be used to sign
            /// code and scripts.
            /// </summary>
            CodeSigning = 0x1,

            /// <summary>
            /// Certificates that can be used to encrypt
            /// data.
            /// </summary>
            DocumentEncryption = 0x2,

            /// <summary>
            /// Certificates that can be used for any
            /// purpose.
            /// </summary>
            All = 0xffff
        }

        /// <summary>
        /// Information used for filtering a set of certs.
        /// </summary>
        internal sealed class CertificateFilterInfo
        {
            internal CertificateFilterInfo()
            {
            }

            /// <summary>
            /// Gets or sets purpose of a certificate.
            /// </summary>
            internal CertificatePurpose Purpose
            {
                get;
                set;
            } = CertificatePurpose.NotSpecified;

            /// <summary>
            /// Gets or sets SSL Server Authentication.
            /// </summary>
            internal bool SSLServerAuthentication
            {
                get;

                set;
            }

/*
            /// <summary>
            /// Gets or sets DNS name of a certificate.
            /// </summary>
            internal WildcardPattern DnsName
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets EKU OID list of a certificate.
            /// </summary>
            internal List<WildcardPattern> Eku
            {
                get;
                set;
            }
*/

            /// <summary>
            /// Gets or sets validity time for a certificate.
            /// </summary>
            internal DateTime Expiring
            {
                get;
                set;
            } = DateTime.MinValue;

            internal const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";
            internal const string OID_PKIX_KP_SERVER_AUTH = "1.3.6.1.5.5.7.3.1";

            // The OID arc 1.3.6.1.4.1.311.80 is assigned to PowerShell. If we need
            // new OIDs, we can assign them under this branch.
            internal const string DocumentEncryptionOid = "1.3.6.1.4.1.311.80.1";
        }
    }

    public static class Utils
    {
        internal const string DefaultPowerShellShellID = "Microsoft.PowerShell";

        internal static string DefaultPowerShellAppBase => GetApplicationBase(DefaultPowerShellShellID);

        internal static string GetApplicationBase(string shellId)
        {
            return "";
        }

        public static long GetFileSize(string filePath)
        {
            long size = 0;

            using (FileStream fs = new(filePath, FileMode.Open))
            {
                size = fs.Length;
            }

            return size;
        }

        internal static bool Succeeded(int hresult)
        {
            return hresult >= 0;
        }

        internal static void CheckArgForNullOrEmpty(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
            else if (arg.Length == 0)
            {
                throw new ArgumentException(argName);
            }
        }

        internal static void CheckArgForNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        internal static Exception NewArgumentException(string paramName, string resourceString, params object[] args)
        {
            string message = string.Format(resourceString, args);
            return new ArgumentException(message, paramName);
        }
    }
}