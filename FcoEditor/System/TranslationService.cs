using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConverseEditor.Utility
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
            var entriesRegex = entries.ConvertAll(r => r.Letter != "" ? Regex.Escape(r.Letter) : "");
            //Remove all entries that are empty to avoid the regex filter bugging out
            entriesRegex.RemoveAll(x => x == "");
            string pattern = string.Join("|", entriesRegex);
            string returnVal = text;
            string result = Regex.Replace(returnVal, pattern, match =>
            {
                string key = match.Value;
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
                    .Select(int.Parse)
                    .ToArray();
        }
        public static string RawHEXtoTXT(int[] hex, List<TranslationTable.Entry> entries)
        {
            string returnVal = "";
            foreach(var code in hex)
            {
                foreach (var entry in entries)
                {
                    if (code == entry.ConverseID)
                        returnVal += entry.Letter;
                }
            }
            return returnVal;
        }
    }
}