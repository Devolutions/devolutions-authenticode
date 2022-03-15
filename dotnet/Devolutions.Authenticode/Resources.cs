
using System;
using System.Reflection;

namespace Devolutions.Authenticode
{
    public class Resources
    {

        private static global::System.Resources.ResourceManager resourceMan = null;

        private static global::System.Globalization.CultureInfo resourceCulture = null;

        /// <summary>constructor</summary>
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan is null)
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Devolutions.Authenticode.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }

                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current threads CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }

            set
            {
                resourceCulture = value;
            }
        }

        internal static string GetResourceString(string name, System.Globalization.CultureInfo Culture)
        {
            return ResourceManager.GetString(name, resourceCulture);
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded because you opted not to run this software now.
        ///  
        /// </summary>
        internal static string Reason_DoNotRun
        {
            get
            {
                return GetResourceString("Reason_DoNotRun", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded because you opted never to run software from this publisher.
        ///  
        /// </summary>
        internal static string Reason_NeverRun
        {
            get
            {
                return GetResourceString("Reason_NeverRun", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} is published by {1}. This publisher is explicitly not trusted on your system. The script will not run on the system. For more information, run the command "get-help about_signing".
        ///  
        /// </summary>
        internal static string Reason_NotTrusted
        {
            get
            {
                return GetResourceString("Reason_NotTrusted", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded because running scripts is disabled on this system. For more information, see about_Execution_Policies at https://go.microsoft.com/fwlink/?LinkID=135170.
        ///  
        /// </summary>
        internal static string Reason_RestrictedMode
        {
            get
            {
                return GetResourceString("Reason_RestrictedMode", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded. {1}.
        ///  
        /// </summary>
        internal static string Reason_Unknown
        {
            get
            {
                return GetResourceString("Reason_Unknown", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded because its operation is blocked by software restriction policies, such as those created by using Group Policy.
        ///  
        /// </summary>
        internal static string Reason_DisallowedBySafer
        {
            get
            {
                return GetResourceString("Reason_DisallowedBySafer", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} cannot be loaded because its content could not be read.
        ///  
        /// </summary>
        internal static string Reason_FileContentUnavailable
        {
            get
            {
                return GetResourceString("Reason_FileContentUnavailable", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign code. The specified certificate is not suitable for code signing.
        ///  
        /// </summary>
        internal static string CertNotGoodForSigning
        {
            get
            {
                return GetResourceString("CertNotGoodForSigning", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign code. The TimeStamp server URL must be fully qualified, and in the format http://<server url>.
        ///  
        /// </summary>
        internal static string TimeStampUrlRequired
        {
            get
            {
                return GetResourceString("TimeStampUrlRequired", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign code. The hash algorithm is not supported.
        ///  
        /// </summary>
        internal static string InvalidHashAlgorithm
        {
            get
            {
                return GetResourceString("InvalidHashAlgorithm", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Do you want to run software from this untrusted publisher?
        ///  
        /// </summary>
        internal static string AuthenticodePromptCaption
        {
            get
            {
                return GetResourceString("AuthenticodePromptCaption", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} is published by {1} and is not trusted on your system. Only run scripts from trusted publishers.
        ///  
        /// </summary>
        internal static string AuthenticodePromptText
        {
            get
            {
                return GetResourceString("AuthenticodePromptText", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Software {0} is published by an unknown publisher. It is recommended that you do not run this software.
        ///  
        /// </summary>
        internal static string AuthenticodePromptText_UnknownPublisher
        {
            get
            {
                return GetResourceString("AuthenticodePromptText_UnknownPublisher", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Security warning
        ///  
        /// </summary>
        internal static string RemoteFilePromptCaption
        {
            get
            {
                return GetResourceString("RemoteFilePromptCaption", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Run only scripts that you trust. While scripts from the internet can be useful, this script can potentially harm your computer. If you trust this script, use the Unblock-File cmdlet to allow the script to run without this warning message. Do you want to run {0}?
        ///  
        /// </summary>
        internal static string RemoteFilePromptText
        {
            get
            {
                return GetResourceString("RemoteFilePromptText", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Ne&ver run
        ///  
        /// </summary>
        internal static string Choice_NeverRun
        {
            get
            {
                return GetResourceString("Choice_NeverRun", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Do not run the script from this publisher now, and do not prompt me to run this script in future. Future attempts to run this script will result in a silent failure.
        ///  
        /// </summary>
        internal static string Choice_NeverRun_Help
        {
            get
            {
                return GetResourceString("Choice_NeverRun_Help", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    &Do not run
        ///  
        /// </summary>
        internal static string Choice_DoNotRun
        {
            get
            {
                return GetResourceString("Choice_DoNotRun", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Do not run the script from this publisher now, and continue to prompt me to run this script in the future.
        ///  
        /// </summary>
        internal static string Choice_DoNotRun_Help
        {
            get
            {
                return GetResourceString("Choice_DoNotRun_Help", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    &Run once
        ///  
        /// </summary>
        internal static string Choice_RunOnce
        {
            get
            {
                return GetResourceString("Choice_RunOnce", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Run the script from this publisher now, and continue to prompt me to run this script in the future.
        ///  
        /// </summary>
        internal static string Choice_RunOnce_Help
        {
            get
            {
                return GetResourceString("Choice_RunOnce_Help", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    &Always run
        ///  
        /// </summary>
        internal static string Choice_AlwaysRun
        {
            get
            {
                return GetResourceString("Choice_AlwaysRun", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Run the script from this publisher now, and do not prompt me to run this script in the future.
        ///  
        /// </summary>
        internal static string Choice_AlwaysRun_Help
        {
            get
            {
                return GetResourceString("Choice_AlwaysRun_Help", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    &Suspend
        ///  
        /// </summary>
        internal static string Choice_Suspend
        {
            get
            {
                return GetResourceString("Choice_Suspend", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Pause the current pipeline and return to the command prompt. Type exit to resume operation when you are done.
        ///  
        /// </summary>
        internal static string Choice_Suspend_Help
        {
            get
            {
                return GetResourceString("Choice_Suspend_Help", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Signature verified.
        ///  
        /// </summary>
        internal static string MshSignature_Valid
        {
            get
            {
                return GetResourceString("MshSignature_Valid", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    The file {0} is not digitally signed. You cannot run this script on the current system. For more information about running scripts and setting execution policy, see about_Execution_Policies at https://go.microsoft.com/fwlink/?LinkID=135170
        ///  
        /// </summary>
        internal static string MshSignature_NotSigned
        {
            get
            {
                return GetResourceString("MshSignature_NotSigned", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    The contents of file {0} might have been changed by an unauthorized user or process, because the hash of the file does not match the hash stored in the digital signature. The script cannot run on the specified system. For more information, run Get-Help about_Signing.
        ///  
        /// </summary>
        internal static string MshSignature_HashMismatch
        {
            get
            {
                return GetResourceString("MshSignature_HashMismatch", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} is signed but the signer is not trusted on this system.
        ///  
        /// </summary>
        internal static string MshSignature_NotTrusted
        {
            get
            {
                return GetResourceString("MshSignature_NotTrusted", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign the file because the system does not support signing operations on {0} files.
        ///  
        /// </summary>
        internal static string MshSignature_NotSupportedFileFormat
        {
            get
            {
                return GetResourceString("MshSignature_NotSupportedFileFormat", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign the file because the system does not support signing operations on files that do not have a file name extension.
        ///  
        /// </summary>
        internal static string MshSignature_NotSupportedFileFormat_NoExtension
        {
            get
            {
                return GetResourceString("MshSignature_NotSupportedFileFormat_NoExtension", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    The signature cannot be verified because it is incompatible with the current system.
        ///  
        /// </summary>
        internal static string MshSignature_Incompatible
        {
            get
            {
                return GetResourceString("MshSignature_Incompatible", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    The signature cannot be verified because it is incompatible with the current system. The hash algorithm is not valid.
        ///  
        /// </summary>
        internal static string MshSignature_Incompatible_HashAlgorithm
        {
            get
            {
                return GetResourceString("MshSignature_Incompatible_HashAlgorithm", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign code. The specified certificate is not suitable for code signing.
        ///  
        /// </summary>
        public static string PowerShell_CertNotGoodForSigning
        {
            get
            {
                return ResourceManager.GetString("PowerShell_CertNotGoodForSigning", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    Cannot sign code.  The TimeStamp server URL must be fully qualified in the form of http://<server url>
        ///  
        /// </summary>
        public static string PowerShell_TimeStampUrlRequired
        {
            get
            {
                return ResourceManager.GetString("PowerShell_TimeStampUrlRequired", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    The Get-AuthenticodeSignature cmdlet does not support directories. Supply a path to a file and retry.
        ///  
        /// </summary>
        public static string PowerShell_CannotRetrieveFromContainer
        {
            get
            {
                return ResourceManager.GetString("PowerShell_CannotRetrieveFromContainer", resourceCulture);
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to 
        ///    File {0} was not found.
        ///  
        /// </summary>
        public static string PowerShell_FileNotFound
        {
            get
            {
                return ResourceManager.GetString("PowerShell_FileNotFound", resourceCulture);
            }
        }
    }

}
