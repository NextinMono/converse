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
using ConverseEditor.ShurikenRenderer;

namespace ConverseEditor
{
    public class ConverseProject : IProgramProject
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
            public string WorkFilePath;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
            public string WorkFilePathFTE;
        }
        public List<ConverseEditor.Window> windowList = new List<ConverseEditor.Window>();
        public FontConverse fcoFile;
        public FontTexture fteFile;
        public static SProjectConfig config;
        private SViewportData viewportData;
        public bool isFileLoaded = false;
        public MainWindow window;
        public Vector2 screenSize => new Vector2(window.WindowSize.X, window.WindowSize.Y);
        public ConverseProject(MainWindow in_Window)
        {
            window = in_Window;
            viewportData = new SViewportData();
            config = new SProjectConfig();
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
                fcoFile = reader.ReadObject<FontConverse>();
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                fcoFile = null;
                fteFile = null;
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
                fteFile = reader.ReadObject<FontTexture>();
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                fcoFile = null;
                fteFile = null;
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FTE file.\n{ex.Message}", 2);
                return false;
            }
            return true;
        }
        public void LoadFile(string in_Path, string in_PathFte)
        {
            config.WorkFilePath = in_Path;
            config.WorkFilePathFTE = in_PathFte;
            if (!LoadFCO(in_Path) || !LoadFTE(in_PathFte))
                return;
            string parentPath = Directory.GetParent(config.WorkFilePath).FullName;
            SpriteHelper.Textures = new();

            List<string> missingTextures = new List<string>();
            foreach (var texture in fteFile.Textures)
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
            ResetWindows();
            SpriteHelper.LoadTextures(fteFile.Characters);
            isFileLoaded = true;

            //Gens FCO, load All table automatically since it only uses that
            if (fcoFile.Header.Version != 0)
            {
                string path = Path.Combine(Program.Path, "Resources", "Tables", "bb", "All.json");
                FcoViewerWindow.Instance.LoadTranslationTable(path);
            }
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }
        public void SaveCurrentFile(string in_Path)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(in_Path, Endianness.Big, Encoding.UTF8);
            writer.WriteObject(fcoFile);
            //if(fcoFile != null)
            //    fcoFile.Write(in_Path);
        }
        internal void RenderWindows()
        {
            foreach (var item in windowList)
            {
                item.Render(this);
            }
        }
        void ResetWindows()
        {
            foreach (var item in windowList)
            {
                item.OnReset(this);
            }
        }
    }
}