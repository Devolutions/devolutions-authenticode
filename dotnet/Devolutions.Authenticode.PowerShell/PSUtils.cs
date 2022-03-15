using System;
using System.IO;
using System.Management.Automation;

namespace Devolutions.Authenticode.PowerShell
{
    internal class PSUtils
    {
        /// <summary>
        /// </summary>
        /// <param name="resourceStr">Resource string.</param>
        /// <param name="errorId">Error identifier.</param>
        /// <param name="args">Replacement params for resource string formatting.</param>
        /// <returns></returns>
        internal static
        ErrorRecord CreateFileNotFoundErrorRecord(string resourceStr,
                                                  string errorId,
                                                  params object[] args)
        {
            string message = string.Format(resourceStr, args);

            FileNotFoundException e = new(message);

            ErrorRecord er = new(
                e,
                errorId,
                ErrorCategory.ObjectNotFound,
                targetObject: null);

            return er;
        }
    }
}
