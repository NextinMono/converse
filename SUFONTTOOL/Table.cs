using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Drawing;
using SUFontTool;

namespace SonicUnleashedFCOConv {
    public static class Table {
        public static List<Structs.Character> characters = XML.characters;
        public class ConversionEntry {
            public string? letter { get; set; }
            public string? hexString { get; set; }
        }
        public static void WriteJSON() {

            List<ConversionEntry> conversionTable = new List<ConversionEntry>();

            string? userInput;

            foreach (Structs.Character character in characters) {
                Console.WriteLine(character.convID + ": ");
                userInput = Console.ReadLine();
                if (userInput == null) {
                    userInput = "{FILL}";
                }
                conversionTable.Add(new ConversionEntry { letter = userInput, hexString = character.convID });
            }

            // Convert the list to JSON and write to a file
            string tableData = JsonSerializer.Serialize(conversionTable, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("conversionTable.json", tableData);

            Console.WriteLine("JSON file created successfully.");
        }
    }
}