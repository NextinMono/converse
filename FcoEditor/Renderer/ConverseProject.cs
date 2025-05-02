using Amicitia.IO.Binary;
using ConverseEditor.Rendering;
using libfco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Texture = ConverseEditor.Rendering.Texture;
using HekonrayBase;
using System.Numerics;
using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using HekonrayBase.Base;

namespace ConverseEditor
{
    public class ConverseProject : Singleton<ConverseProject>, IProgramProject
    {
        public struct SViewportData
        {
            public int csdRenderTextureHandle;
            public Vector2Int framebufferSize;
            public int renderbufferHandle;
            public int framebufferHandle;
        }
        public struct SProjectConfig
        {
            public FontConverse fcoFile;
            public FontTexture fteFile;
            public string fcoPath;
            public string ftePath;
            public string tablePath;
            public List<TranslationTable.Entry> translationTable;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
            public SProjectConfig()
            {
                translationTable = new List<TranslationTable.Entry>();
            }
        }
        public SProjectConfig config;
        private SViewportData viewportData;
        public bool isFileLoaded = false;
        public MainWindow window;
        public Vector2 screenSize => new Vector2(window.WindowSize.X, window.WindowSize.Y);
        public ConverseProject() { }
        public ConverseProject(MainWindow in_Window)
        {
            window = in_Window;
            viewportData = new SViewportData();
            config = new SProjectConfig();
        }
        private void SendResetSignal()
        {
            window.ResetWindows(this);
        }
        public void ShowMessageBoxCross(string title, string message, int logType = 0)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.Information;
                switch(logType)
                {
                    case 0:
                        image = System.Windows.MessageBoxImage.Information;
                        break;
                    case 1:
                        image = System.Windows.MessageBoxImage.Warning;
                        break;
                    case 2:
                        image = System.Windows.MessageBoxImage.Error;
                        break;
                }
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, image);
            }
        }
        private bool LoadFCO(string in_Path)
        {
            BinaryObjectReader reader = new BinaryObjectReader(in_Path, Endianness.Big, Encoding.GetEncoding("UTF-8"));
            try
            {
                config.fcoFile = reader.ReadObject<FontConverse>();
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                config.fcoFile = null;
                config.fteFile = null;
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FCO file.\n{ex.Message}", 2);
                return false;
            }
            return true;
        }
        private bool LoadFTE(string in_Path)
        {
            BinaryObjectReader reader = new BinaryObjectReader(in_Path, Endianness.Big, Encoding.GetEncoding("UTF-8"));
            try
            {
                config.fteFile = reader.ReadObject<FontTexture>();
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                config.fcoFile = null;
                config.fteFile = null;
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FTE file.\n{ex.Message}", 2);
                return false;
            }
            return true;
        }
        public void LoadFile(string in_Path, string in_PathFte)
        {
            config.fcoPath = in_Path;
            config.ftePath = in_PathFte;
            if (!LoadFCO(in_Path) || !LoadFTE(in_PathFte))
                return;
            string parentPath = Directory.GetParent(config.fcoPath).FullName;
            SpriteHelper.Textures = new();

            List<string> missingTextures = new List<string>();
            foreach (var texture in config.fteFile.Textures)
            {
                string pathtemp = Path.Combine(parentPath, texture.Name + ".dds");
                if (File.Exists(pathtemp))
                    SpriteHelper.Textures.Add(new Texture(pathtemp));
                else
                {
                    var commonPathTexture = Path.Combine(Program.Path,"Resources","CommonTextures",texture.Name + ".dds");
                    if (File.Exists(commonPathTexture))
                    {
                        SpriteHelper.Textures.Add(new Texture(commonPathTexture));
                    }
                    else
                    {
                        SpriteHelper.Textures.Add(new Texture(""));
                        missingTextures.Add(texture.Name);
                    }
                }
            }
            if (missingTextures.Count > 0)
            {
                string textureNames = "";
                foreach (string textureName in missingTextures)
                    textureNames += "-" + textureName + "\n";
                ShowMessageBoxCross("Warning", $"The file uses textures that could not be found, they will be replaced with text.\n\nMissing Textures:\n{textureNames}", 1);
            }
            SendResetSignal();
            SpriteHelper.LoadTextures(config.fteFile.Characters);
            isFileLoaded = true;

            //Gens FCO, load All table automatically since it only uses that
            if (config.fcoFile.Header.Version != 0)
            {
                string path = Path.Combine(Program.Path, "Resources", "Tables", "bb", "All.json");
                ImportTranslationTable(path);
            }
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }
        public void SaveCurrentFile(string in_Path)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(in_Path, Endianness.Big, Encoding.UTF8);
            writer.WriteObject(config.fcoFile);
            //if(fcoFile != null)
            //    fcoFile.Write(in_Path);
        }
        public void ImportTranslationTable(string @in_Path)
        {
            config.translationTable.Clear();
            config.tablePath = in_Path;
            config.translationTable = TranslationTable.Read(@in_Path).Tables["Standard"];
            AddMissingFteEntriesToTable(config.translationTable, true);
        }
        void AddMissingFteEntriesToTable(List<TranslationTable.Entry> in_Entries, bool isUnleashed, bool in_AddDefault = true)
        {
            if (in_AddDefault)
            {
                if (isUnleashed)
                {
                    //Add default icons
                    List<string> keys = new List<string>
                {
                    "{A}", "{B}", "{X}", "{Y}", "{LB}", "{RB}", "{LT}", "{RT}",
                    "{LSUP}", "{LSRIGHT}", "{LSDOWN}", "{LSLEFT}", "{RSUP}", "{RSRIGHT}",
                    "{RSDOWN}", "{RSLEFT}", "{DPADUP}", "{DPADRIGHT}", "{DPADDOWN}",
                    "{DPADLEFT}", "{START}", "{SELECT}"
                };
                    //Add first set of keys unaltered
                    int index = 100;
                    foreach (string key in keys)
                    {
                        in_Entries.Add(new TranslationTable.Entry(key, index));
                        index++;
                    }

                }
            }
            for (int i = 0; i < in_Entries.Count; i++)
            {
                //Replace legacy newline with new style
                if (in_Entries[i].Letter == "{NewLine}")
                {
                    var entry2 = in_Entries[i];
                    entry2.Letter = "\n";
                    in_Entries[i] = entry2;
                }
            }
            foreach (var spr in SpriteHelper.CharSprites)
            {
                if (in_Entries.FindAll(x => x.ConverseID == spr.Key.CharacterID).Count == 0)
                {
                    in_Entries.Add(new TranslationTable.Entry("", spr.Key.CharacterID));
                }
            }
        }

        public void CreateTranslationTable(bool in_AddDefault = true)
        {
            config.translationTable = new List<TranslationTable.Entry>();
            config.translationTable.Add(new TranslationTable.Entry("\\n", 0));
            AddMissingFteEntriesToTable(config.translationTable, config.fcoFile.Header.Version == 0, in_AddDefault);
        }

        public void WriteTableToDisk(string @in_Path)
        {
            TranslationTable table = new TranslationTable();
            table.Standard = config.translationTable;
            table.Write(@in_Path);
        }

        internal bool IsFcoLoaded() => !string.IsNullOrEmpty(config.fcoPath);
        internal bool IsFteLoaded() => !string.IsNullOrEmpty(config.ftePath);
        internal bool IsTableLoaded() => config.translationTable.Count > 0;

        internal void AddNewGroup(string in_Name = null)
        {
            config.fcoFile.Groups.Add(new Group(string.IsNullOrEmpty(in_Name) ? $"New_Group_{config.fcoFile.Groups.Count}" : in_Name));
        }
    }
}