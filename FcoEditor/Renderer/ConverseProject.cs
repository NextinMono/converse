using Amicitia.IO.Binary;
using Converse.Rendering;
using libfco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Texture = Converse.Rendering.Texture;
using HekonrayBase;
using System.Numerics;
using Converse.ShurikenRenderer;
using Converse.Utility;
using HekonrayBase.Base;

namespace Converse
{
    public class ConverseProject : Singleton<ConverseProject>, IProgramProject
    {
        public struct SFileInfo
        {
            public string path;
            public FontConverse file;
            public SFileInfo(FontConverse in_File, string in_Path)
            {
                this.file = in_File;
                this.path = in_Path;
            }

            internal string GetFileName()
            {
                return Path.GetFileName(path);
            }
        }
        public struct SViewportData
        {
            public int csdRenderTextureHandle;
            public Vector2Int framebufferSize;
            public int renderbufferHandle;
            public int framebufferHandle;
        }
        public struct SProjectConfig
        {
            public List<SFileInfo> fcoFile;
            public FontTexture fteFile;
            public string ftePath;
            public string tablePath;
            public List<TranslationTable.Entry> translationTable;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
            public SProjectConfig()
            {
                fcoFile = new List<SFileInfo>();
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
        public void Reset()
        {
            isFileLoaded = false;
            config.fcoFile.Clear();
            config.fteFile = null;
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
        public bool LoadFCO(string in_Path, bool in_OnlyOne = true)
        {
            if (in_OnlyOne)
            {
                config.fcoFile.Clear();
            }
            try
            {
                BinaryObjectReader reader = new BinaryObjectReader(in_Path, Endianness.Big, Encoding.GetEncoding("UTF-8"));
                config.fcoFile.Add(new SFileInfo(reader.ReadObject<FontConverse>(), in_Path));
            }
            catch (Exception ex)
            {
                Reset();
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FCO file.\n{ex.Message}", 2);
                return false;
            }
            return true;
        }
        private bool LoadFTE(string in_Path)
        {
            try
            {
                BinaryObjectReader reader = new BinaryObjectReader(in_Path, Endianness.Big, Encoding.GetEncoding("UTF-8"));
                config.fteFile = reader.ReadObject<FontTexture>();
            }
            catch (Exception ex)
            {
                Reset();
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FTE file.\n{ex.Message}", 2);
                return false;
            }
            config.ftePath = in_Path;
            string parentPath = Directory.GetParent(config.fcoFile[0].path).FullName;
            SpriteHelper.Textures = new();

            List<string> missingTextures = new List<string>();
            foreach (var texture in config.fteFile.Textures)
            {
                string pathtemp = Path.Combine(parentPath, texture.Name + ".dds");
                if (File.Exists(pathtemp))
                    SpriteHelper.Textures.Add(new Texture(pathtemp));
                else
                {
                    var commonPathTexture = Path.Combine(Program.Path, "Resources", "CommonTextures", texture.Name + ".dds");
                    if (File.Exists(commonPathTexture))
                    {
                        SpriteHelper.Textures.Add(new Texture(commonPathTexture));
                    }
                    else
                    {
                        SpriteHelper.Textures.Add(new Texture(""));
                        if(!missingTextures.Contains(texture.Name + ".dds"))
                            missingTextures.Add(texture.Name + ".dds");
                    }
                }
            }
            if (missingTextures.Count > 0)
            {
                string textureNames = "";
                foreach (string textureName in missingTextures)
                    textureNames += "- " + textureName + "\n";
                ShowMessageBoxCross("Warning", $"The FTE file uses some textures that could not be found. Characters that use these textures will be shown as squares with numbers.\n\nMissing Textures:\n{textureNames}\nYou can put these textures in \"Resources\\CommonTextures\" to make them always load whenever you load another FTE file.", 1);
            }

            SpriteHelper.LoadTextures(config.fteFile.Characters);
            return true;
        }
        private void AfterLoadFile()
        {
            SendResetSignal();
            isFileLoaded = true;

            //Gens FCO, load All table automatically since it only uses that
            if (config.fcoFile[0].file.Header.Version != 0)
            {
                string path = Path.Combine(Program.Path, "Resources", "Tables", "bb", "All.json");
                ImportTranslationTable(path);
            }
            if (GetFcoFiles().Count > 0)
            {
                if (GetFcoFiles().Count > 1)
                    window.Title = window.appName + $" - [{config.ftePath}]";
                else
                    window.Title = window.appName + $" - [{config.fcoFile[0].path}]";
            }
            else
                window.Title = window.appName;
        }
        public void LoadPairFile(string in_Path, string in_PathFte)
        {
            if (!LoadFCO(in_Path) || !LoadFTE(in_PathFte))
                return;
            AfterLoadFile();
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }
        public void SaveFcoFiles(string in_Path)
        {
            bool usePathArg = string.IsNullOrEmpty(in_Path);
            string parentDir = !usePathArg ? Directory.GetParent(in_Path).FullName : "";
            foreach(var file in config.fcoFile)
            {
                string path = !usePathArg ? Path.Combine(parentDir, Path.GetFileName(file.path)) : file.path;
                using BinaryObjectWriter writer = new BinaryObjectWriter(path, Endianness.Big, Encoding.UTF8);
                writer.WriteObject(file.file);
            }
            List<Character> characters = new List<Character>();
            SpriteHelper.BuildCharaList(ref characters);
            config.fteFile.Characters = characters;

            using BinaryObjectWriter writer2 = new BinaryObjectWriter(config.ftePath, Endianness.Big, Encoding.UTF8);
            writer2.WriteObject(config.fteFile);

            System.Media.SystemSounds.Asterisk.Play();
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
            foreach (var spr in SpriteHelper.ConverseSprites)
            {
                if (in_Entries.FindAll(x => x.ConverseID == spr.converseChara.CharacterID).Count == 0)
                {
                    in_Entries.Add(new TranslationTable.Entry("", spr.converseChara.CharacterID));
                }
            }
        }

        public void CreateTranslationTable(bool in_AddDefault = true)
        {
            config.translationTable = new List<TranslationTable.Entry>();
            config.translationTable.Add(new TranslationTable.Entry("\\n", 0));
            AddMissingFteEntriesToTable(config.translationTable, config.fcoFile[0].file.Header.Version == 0, in_AddDefault);
        }

        public void WriteTableToDisk(string @in_Path)
        {
            TranslationTable table = new TranslationTable();
            table.Standard = config.translationTable;
            table.Write(@in_Path);
        }

        internal bool IsFteLoaded() => !string.IsNullOrEmpty(config.ftePath);
        internal bool IsTableLoaded() => config.translationTable.Count > 0;

        internal void AddNewGroup(int in_FileIndex, string in_Name = null)
        {
            var list = config.fcoFile[in_FileIndex].file;
            list.Groups.Add(new Group(string.IsNullOrEmpty(in_Name) ? $"New_Group_{list.Groups.Count}" : in_Name));
        }

        internal List<SFileInfo> GetFcoFiles() => config.fcoFile;
        public string AskForFTE(string in_FcoPath, bool in_UseParent = true)
        {
            var possibleFtePath = Path.Combine(in_UseParent ? Directory.GetParent(in_FcoPath).FullName : in_FcoPath, "fte_ConverseMain.fte");

            var testdial2 = NativeFileDialogSharp.Dialog.FileOpen("fte", Directory.GetParent(in_FcoPath).FullName);
            if (testdial2.IsOk)
            {
                possibleFtePath = testdial2.Path;
            }
            return possibleFtePath;
        }

        internal void LoadFolder(string path)
        {
            config.fcoFile.Clear();
            config.fteFile = null;
            config.ftePath = "";
            var files = Directory.GetFiles(path, "*.fco");
            var ftePaths = Directory.GetFiles(path, "*.fte");
            string ftePath = "";
            if (files.Length == 0)
                return;
            if(ftePaths.Length > 1 || ftePaths.Length == 0)
            {
                ftePath = AskForFTE(path);
            }
            else
            {
                ftePath = ftePaths[0];
            }
            foreach (var file in files)
            {
                LoadFCO(file, false);
            }
            LoadFTE(ftePath);
            AfterLoadFile();
        }
    }
}