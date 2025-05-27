using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
namespace Converse.Utility
{
    public class TranslationTable
    {
        public struct Entry
        {
            private int m_CharacterID;
            public string Letter;

            [JsonProperty(PropertyName = "HexString")]
            public string LegacyString;
            public int ConverseID
            {
                get
                {
                    //Legacy translation table from Hedgeturd will not have IDs but will have strings instead
                    if(m_CharacterID == 0)
                    {
                        if(!string.IsNullOrEmpty(LegacyString))
                            m_CharacterID = Convert.ToInt32(LegacyString.Replace(" ", ""), 16);
                    }
                    return m_CharacterID;
                }
                set
                {
                    m_CharacterID = value;
                }
            }  
            //Used by Newtonsoft.Json to ignore writing the property
            public bool ShouldSerializeLegacyString()
            {
                return false;
            }
            public Entry(string letter, int converseID)
            {
                Letter = letter;
                ConverseID = converseID;
            }
        }
        public List<Entry> Standard 
        {
            get
            {
                return Tables["Standard"];
            }
            set
            {
                Tables["Standard"] = value;
            }
        }
        public string Name;
        public Dictionary<string, List<Entry>> Tables = new Dictionary<string, List<Entry>>();

        public static TranslationTable Read(string in_Path)
        {
            //You could make this better by not having extra tables.
            TranslationTable newTable = new TranslationTable();
            newTable.Name = Path.GetFileName(in_Path);
            string parentDir = Path.Combine(Directory.GetParent(in_Path).FullName, Path.GetFileNameWithoutExtension(in_Path));
            string pathSmall = parentDir + "_Small.json";
            string pathLarge = parentDir + "_Large.json";
            string pathExtra = parentDir + "_Extra.json";

            if (File.Exists(in_Path)) newTable.Tables.Add("Standard", JsonConvert.DeserializeObject<List<Entry>>(File.ReadAllText(in_Path)));
            if (File.Exists(pathSmall)) newTable.Tables.Add("Small",JsonConvert.DeserializeObject<List<Entry>>(File.ReadAllText(pathSmall)));
            if (File.Exists(pathLarge)) newTable.Tables.Add("Large",JsonConvert.DeserializeObject<List<Entry>>(File.ReadAllText(pathSmall)));
            if (File.Exists(pathExtra)) newTable.Tables.Add("Extra",JsonConvert.DeserializeObject<List<Entry>>(File.ReadAllText(pathExtra)));
            return newTable;
        }

        public void Write(string in_Path)
        {
            string fileContent = JsonConvert.SerializeObject(Tables["Standard"]);
            if (!Path.HasExtension(in_Path)) in_Path += ".json";
            if (File.Exists(in_Path)) File.Delete(in_Path);
            File.WriteAllText(in_Path, fileContent);
        }
    }
}
