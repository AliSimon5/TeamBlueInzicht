using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDSignaturesTool.Helpers
{
    internal class StrCI
    {
        // Functies die strings vergelijken/vervangen met een case insensitive insteek
        #region Case insensitive methods
        public static string Replace(string argText, string argOldWord, string argNewWord)
        {
            try
            {
                string result = Regex.Replace(argText, argOldWord, argNewWord, RegexOptions.IgnoreCase);

                return result;
            }
            catch { }

            return argText;
        }

        public static bool Equals(string argStringA, string argStringB)
        {
            // Case Insensitive
            if (argStringA.Equals(argStringB, StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }
        public static bool EqualsTrim(string argStringA, string argStringB)
        {
            // Case insensitive and Trim
            if (argStringA.Trim().Equals(argStringB.Trim(), StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }
        public static bool EndsWith(string argString, string argStringBEnding)
        {
            if (argString.EndsWith(argStringBEnding, StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }

        public static bool EndsWithTrim(string argString, string argStringBEnding)
        {
            return EndsWith(argString.Trim(), argStringBEnding.Trim());
        }

        public static bool StartsWith(string argString, string argStringBBeginning)
        {
            if (argString.StartsWith(argStringBBeginning, StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }
        public static bool StartsWithTrim(string argString, string argStringBBeginning)
        {
            return StartsWith(argString.Trim(), argStringBBeginning.Trim());
        }

        public static bool EqualDN(string argDN1, string argDN2)
        {
            return EqualsTrim(argDN1.Replace(", ", ","), argDN2.Replace(", ", ","));
        }

        public static bool Contains(string argContext, string argWord)
        {
            return argContext.IndexOf(argWord, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
