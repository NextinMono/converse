using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Converse.Utility
{
    public class TranslationService
    {
        /// <summary>
        /// Converts text to the corresponding Converse ID using a table entry list.
        /// </summary>
        /// <param name="text">Text to convert into IDs</param>
        /// <param name="entries">List of translation tables</param>
        /// <returns></returns>
        public static int[] RawTXTtoHEX(string @text, List<TranslationTable.Entry> entries)
        {
            //Convert all the entries into a regex pattern
            var entriesRegex = entries
        .Where(e => !string.IsNullOrEmpty(e.Letter))
        .Select(e => Regex.Escape(e.Letter))
        .ToList();


            //Remove all entries that are empty to avoid the regex filter bugging out
            entriesRegex.RemoveAll(x => x == "");
            //Add a specific pattern for sequences with numbers in them, so that you can type IDs directly
            //(e.g: {256} = e)
            string pattern = @"\{\d+\}|" + string.Join("|", entriesRegex);
            string returnVal = text;
            string result = Regex.Replace(returnVal, pattern, match =>
            {
                string key = match.Value;
                // Check if it's an explicit ID
                if (Regex.IsMatch(key, @"^\{\d+\}$"))
                {
                    return key.Trim('{', '}') + ", ";
                }
                foreach (var replacement in entries)
                {                    
                    //In case the table contains empty entries, ignore them
                    if (replacement.Letter == "") continue;

                    //Add a comma+space if its not the last character, cause otherwise random characters will show at the end
                    string separator = match.Index == text.Length - 1 ? "" : ", ";

                    //If the letter corresponds to an entry, replace it with the ID string
                    if (replacement.Letter == key)
                        return replacement.ConverseID + separator;
                }
                return key;
            });
            
            return result.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
        .Select(s => int.TryParse(s, out int num) ? (int?)num : null) // Try to parse, otherwise return null
        .Where(num => num.HasValue) // Remove null values (invalid numbers)
        .Select(num => num.Value)   // Extract valid integers
        .ToArray();

        }
        private static string GetIDAsStringDefault(int in_Id) => "{" + $"{in_Id}" + "}";
        public static string RawHEXtoTXT(int[] hex, List<TranslationTable.Entry> entries)
        {
            string returnVal = "";
            bool found;
            foreach(var code in hex)
            {
                found = false;
                foreach (var entry in entries)
                {
                    if (code == entry.ConverseID)
                    {
                        string letterToAdd = entry.Letter;
                        if(entry.Letter == "")
                        {
                            letterToAdd = GetIDAsStringDefault(entry.ConverseID);
                        }
                        found = true;
                        returnVal += letterToAdd;
                    }
                }
                if(!found)
                    returnVal += GetIDAsStringDefault(code);

            }
            return returnVal;
        }
    }
}