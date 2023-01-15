using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core
{
    public static class Helper
    {
        public static void Switch<T>(this IList<T> array, int index1, int index2)
        {
            var aux = array[index1];
            array[index1] = array[index2];
            array[index2] = aux;
        }
    }

    public class RuleHelper
    {
        public static string ToPascalCase(string original)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }

        public static string Validate(string name, bool isFile = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Empty name";

            string Name = "";

            if (isFile)
            {
                var lastDot = name.LastIndexOf('.');
                string extension = name.Substring(lastDot + 1);
                Name = name.Substring(0, lastDot);
            }
            else
                Name = name;

            // Check if exceeding the maximum of 255 characters
            if (Name.Length > 255)
                return "Name must not be beyond 255 characters in length";

            List<char> invalidChars = new List<char>(){
                // The forbidden printable ASCII characters 
                '<',
                '>',
                ':',
                '\"',
                '/',
                '\\',
                '|',
                '?',
                '*',

                // Non-printable characters
                (char)0,
                (char)1,
                (char)2,
                (char)3,
                (char)4,
                (char)5,
                (char)6,
                (char)7,
                (char)8,
                (char)9,
                (char)10,
                (char)11,
                (char)12,
                (char)13,
                (char)14,
                (char)15,
                (char)16,
                (char)17,
                (char)18,
                (char)19,
                (char)20,
                (char)21,
                (char)22,
                (char)23,
                (char)24,
                (char)25,
                (char)26,
                (char)27,
                (char)28,
                (char)29,
                (char)30,
                (char)31
            };

            List<string> invalidNames = new List<string>()
            {
                // Reserved file names
                "CON",
                "PRN",
                "AUX",
                "CLOCK$",
                "NUL",
                "COM1",
                "COM2",
                "COM3",
                "COM4",
                "COM5",
                "COM6",
                "COM7",
                "COM8",
                "COM9",
                "LPT1",
                "LPT2",
                "LPT3",
                "LPT4",
                "LPT5",
                "LPT6",
                "LPT7",
                "LPT8",
                "LPT9",
                "$Mft",
                "$MftMirr",
                "$LogFile",
                "$Volume",
                "$AttrDef",
                "$Bitmap",
                "$Boot",
                "$BadClus",
                "$Secure",
                "$Upcase",
                "$Extend",
                "$Quota",
                "$Objld",
                "$Reparse"
            };

            // check if prohibited characters found
            foreach (var invalidChar in invalidChars)
                if (Name.Contains(invalidChar))
                    return $"character not valid found: \"{invalidChar}\"";

            // check if prohibited names found
            foreach (var invalidName in invalidNames)
                if (Name.Equals(name))
                    return $"name not valid found: \"{invalidName}\"";

            // check if dots or spaces are at the end
            const string pattern = @"\.+$|\s+$";
            Regex EndDotsAndSpaces = new Regex(pattern);

            if (EndDotsAndSpaces.IsMatch(Name))
                return "End new name up with dots or spaces..";

            return "";
        }
    }
}