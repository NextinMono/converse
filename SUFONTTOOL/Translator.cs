using System.Text;
using System.Text.Json;

namespace SonicUnleashedFCOConv {
    public class Translator {
        // To whomever this may concern, I legit don't even know how I wrote some of this.. It may have been a 2AM job...
        static int thing1;
        static bool iconCheck = false, fontCheck = false, fontSizeFound = false;
        public static string? iconsTablePath, tempTablePath;
        public static List<string> missinglist = new List<string>();
        public static string[] fontarray = {"_Small.json", "_Large.json", "_Extra.json"};
        public enum FontSizes {S = 0, L = 1, X = 2}

        public static string stringConv(string tagstr, string fontsize) {
            tempTablePath = Common.fcoTable;
            Common.fcoTable = XML.tableNoName + fontsize;
            string hexString = Translator.TXTtoHEX(tagstr);
            Common.fcoTable = tempTablePath;

            return hexString;
        }

        public static void fontmerge(List<string> chunks) {
            StringBuilder mergedContent = new StringBuilder();
            bool fontsizer = false;
            int chunkinx = 0, specialinx = 0, thing = 0;
            string[] chunkre = new string[chunks.Count];
            int[] startinx = new int[chunks.Count];
            int[] endinx = new int[chunks.Count];

            foreach (var chunk in chunks)
            {
                if (chunk.Length > 2 && chunk[2] == ':' && chunk.EndsWith("}")) {
                    if (fontsizer == false) {
                        specialinx = chunkinx;
                        fontsizer = true;
                    }
                    mergedContent.Append(chunk.Substring(3, 1));
                }

                if (chunk.Length < 2 && fontsizer) {
                    startinx[thing] = specialinx;
                    endinx[thing] = chunkinx;
                    chunkre[thing] = chunks[chunkinx - 1].Substring(1,1) + "," + mergedContent.ToString();
                    mergedContent.Clear();
                    fontsizer = false;
                    Console.WriteLine(startinx[thing] + endinx[thing] + chunkre[thing]);
                    thing++;
                }

                chunkinx++;

                if (chunkinx == chunks.Count && fontsizer) {
                    startinx[thing] = specialinx;
                    endinx[thing] = chunkinx;
                    chunkre[thing] = chunks[chunkinx - 1].Substring(1,1) + "," + mergedContent.ToString();
                    mergedContent.Clear();
                    fontsizer = false;
                    Console.WriteLine(startinx[thing] + endinx[thing] + chunkre[thing]);
                    thing++;
                }
            }

            for (int i = 0; i < thing; i++) {
                string rebuilt = chunkre[i];
                string mergedString = "{" + chunkre[i].Substring(0,1) + ":" + rebuilt.Substring(2,rebuilt.Length - 2) + "}";

                for (int p = startinx[i]; p < endinx[i]; p++) {
                    chunks[p] = "{FILL}";
                }

                chunks[startinx[i]] = mergedString;
            }

            chunks.RemoveAll(EndsWithSaurus);
        }

        public static string tableSearchCheck(string hexString) {
            string searchResult = "";
            if (iconCheck == false) {
                using JsonDocument docIcon = JsonDocument.Parse(File.ReadAllText(iconsTablePath));
                searchResult = SearchHexStringForLetter(docIcon.RootElement, hexString);
                iconCheck = true;
            }
            else {
                
                    if (File.Exists(Common.fcoTableDir + Common.fcoTableName ))
                    {
                        using JsonDocument docIcon = JsonDocument.Parse(File.ReadAllText(Common.fcoTableDir + Common.fcoTableName));
                        searchResult = SearchHexStringForLetter(docIcon.RootElement, hexString);
                        if (searchResult != "?MISSING?") {
                            fontSizeFound = true;
                        }
                    }
                    else {
                        searchResult = "?MISSING?";
                    }
                

                fontCheck = true;
            }

            return searchResult;
        }

        private static bool EndsWithSaurus(String s) {
            return s.Contains("{FILL}");
        }

        public static int sizeDet(string hexString) {
            byte[] messageByteArray = Common.StringToByteArray(hexString);
            int numberOfBytes = hexString.Length;
            int MessageCharAmount = numberOfBytes / 8;
            //byte[] CellMessageWrite = messageByteArray;

            return MessageCharAmount;
        }

        public static string HEXtoTXT(string hex) {
            List<string> chunks = SplitStringIntoChunks("HEXtoTXT", hex, 11);
            fontmerge(chunks);
            return JoinChunks(chunks);
        }

        public static string TXTtoHEX(string hex) {
            List<string> chunks = SplitStringIntoChunks("TXTtoHEX", hex, 1);
            return JoinChunks(chunks);
        }

        static List<string> SplitStringIntoChunks(string mode, string str, int chunkSize) {
            List<string> chunks = new List<string>();

            int i2 = 0;
            string fontsize = "";
            string taghexString;

            if (mode == "HEXtoTXT") {
                for (int i = 0; i < str.Length; i += chunkSize + 1) {
                    if (i + chunkSize > str.Length) {
                        chunks.Add(JSONRead(mode, str.Substring(i)));
                    }
                    else {
                        chunks.Add(JSONRead(mode, str.Substring(i, chunkSize)));
                    }
                }
            }
            if (mode == "TXTtoHEX") {
                while (i2 < str.Length) {
                    if (str[i2] == '{') {

                        // Find the closing '}'
                        int endIndex = str.IndexOf('}', i2);
                        // Include the closing '}' in the chunk
                        int specialChunkLength = endIndex - i2 + 1;

                        //{S:Small Text}, {L:Large Text}, {X:Extra Large Text}
                        if (str[i2+2] == ':') {
                            switch (str[i2+1]) {
                                case 'S':
                                    fontsize = fontarray[0];
                                    break;
                                case 'L':
                                    fontsize = fontarray[1];
                                    break;
                                case 'X':
                                    fontsize = fontarray[2];
                                    break;
                                default:
                                    Console.WriteLine("ERROR");
                                    Environment.Exit(0);
                                    break;
                            }

                            // Breaking out substring
                            Console.WriteLine(str[i2+1]);
                            string tagstr = str.Substring(i2+3, endIndex - (i2 + 3));
                            Console.WriteLine(tagstr);
                            taghexString = stringConv(tagstr, fontsize);

                            int MessageCharAmount = taghexString.Length / 11;
                            int indstart = 0;
                            for (int i = 0; i < MessageCharAmount; i++) {
                                string tempchunk = taghexString.Substring(indstart, 11);
                                Console.WriteLine(tempchunk);
                                chunks.Add(tempchunk);
                                indstart = indstart + 11;
                            }

                            i2 = endIndex + 1;
                            continue;
                        }
                        
                        //Standard Exit
                        if (endIndex != -1) {
                            chunks.Add(JSONRead(mode, str.Substring(i2, specialChunkLength)));
                            i2 += specialChunkLength; // Move the index past this special chunk
                            continue;
                        }
                    }
                    else {
                        int endIndex = i2 + chunkSize;
                        if (endIndex > str.Length) {
                            endIndex = str.Length;
                        }
                        else {
                            // Ensure not to break in the middle of a "{X}", "{XY}" or "{XYZ}"
                            int nextSpecialIndex = str.IndexOf("{", i2, chunkSize);
                            if (nextSpecialIndex != -1 && nextSpecialIndex < endIndex) {
                                endIndex = nextSpecialIndex;
                            }
                        }

                        string chunk = str.Substring(i2, endIndex - i2);
                        chunks.Add(JSONRead(mode, chunk));
                        i2 = endIndex;
                    }
                }
            }

            return chunks;
        }

        static string JoinChunks(List<string> chunks) {
            return string.Join("", chunks);
        }

        static string JSONRead(string searchMode, string hexString) {
            // Read the JSON file
            // Parse the JSON string into a JsonDocument

            if (File.Exists(Common.fcoTable) == false) {
                Console.WriteLine("\nThis table does not exist\nPlease check your files!\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Common.fcoTable));

            if (searchMode == "HEXtoTXT") {
                string searchResult = SearchHexStringForLetter(doc.RootElement, hexString);

                if (searchResult == "?MISSING?") {
                    searchResult = tableSearchCheck(hexString);

                    if (searchResult == "?MISSING?" && iconCheck == true) {
                        searchResult = tableSearchCheck(hexString);
                    }
                    if (searchResult != "?MISSING?" && fontSizeFound == true) {
                        // Thing
                        searchResult = "{" + Enum.GetName(typeof(FontSizes), thing1) + ":" + searchResult + "}";
                        Console.WriteLine(searchResult);
                        fontSizeFound = false;
                    }
                    if (searchResult == "?MISSING?" && fontCheck == true) {
                        Common.noLetter = true;
                        iconCheck = false;
                        fontCheck = false;
                        missinglist.Add("HexString: " + hexString + " not found in the table " + Common.fcoTableName + "!");
                    }

                    iconCheck = false;
                    fontCheck = false;
                    fontSizeFound = false;
                    return searchResult;
                }
                 else {
                    if (searchResult == "?MISSING") {
                        Common.noLetter = true;
                        iconCheck = false;
                        fontCheck = false;
                        missinglist.Add("HexString: " + hexString + " not found in the table " + Common.fcoTableName + "!");
                    }

                    return searchResult;
                }

                return searchResult;
            }
            if (searchMode == "TXTtoHEX") {
                if (hexString.Contains("{")) {
                    string searchResult = SearchLetterForHexString(doc.RootElement, hexString);
                    
                    if (searchResult == null) {
                        using JsonDocument docIcon = JsonDocument.Parse(File.ReadAllText(iconsTablePath));
                        searchResult = SearchLetterForHexString(docIcon.RootElement, hexString);

                        if (searchResult == null) {
                            Common.noLetter = true;
                            missinglist.Add("Letter: " + hexString + " not found in the table " + Common.fcoTableName + "!");
                        }

                        return searchResult;
                    }

                    return searchResult;
                }
                else {
                    string searchResult = SearchLetterForHexString(doc.RootElement, hexString);

                    if (searchResult == null) {
                        Common.noLetter = true;
                        missinglist.Add("Letter: " + hexString + " not found in the table " + Common.fcoTableName + "!");
                    }

                    return searchResult;
                }
            }

            return null;
        }

        static string SearchHexStringForLetter(JsonElement element, string searchHexString) {
            if (element.ValueKind == JsonValueKind.Array) {
                foreach (JsonElement item in element.EnumerateArray()) {
                    if (item.TryGetProperty("HexString", out JsonElement hexStringElement) && hexStringElement.GetString().Equals(searchHexString)) {
                        if (item.TryGetProperty("Letter", out JsonElement letterElement)) {
                            string? letter = letterElement.GetString();
                            return letter;
                        }
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Object) {
                foreach (JsonProperty property in element.EnumerateObject()) {
                    if (property.Value.ValueKind == JsonValueKind.Array || property.Value.ValueKind == JsonValueKind.Object) {
                        string result = SearchHexStringForLetter(property.Value, searchHexString);
                        if (result != null) {
                            return result; // Return the result immediately after finding the first match
                        }
                    }
                }
            }

            return "?MISSING?"; // Return null if the hexString is not found
        }

        static string SearchLetterForHexString(JsonElement element, string searchLetter)
        {
            if (element.ValueKind == JsonValueKind.Array) {
                foreach (JsonElement item in element.EnumerateArray()) {
                    if (item.TryGetProperty("Letter", out JsonElement letterElement) && letterElement.GetString().Equals(searchLetter)) {
                        if (item.TryGetProperty("HexString", out JsonElement hexStringElement)) {
                            return hexStringElement.GetString();
                        }
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Object) {
                foreach (JsonProperty property in element.EnumerateObject()) {
                    if (property.Value.ValueKind == JsonValueKind.Array || property.Value.ValueKind == JsonValueKind.Object) {
                        string result = SearchLetterForHexString(property.Value, searchLetter);
                        if (result != null) {
                            return result; // Return the result immediately after finding the first match
                        }
                    }
                }
            }

            return null; // Return null if the hexString is not found
        }
    }
}