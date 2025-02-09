using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Shuriken.Rendering;
using SUFcoTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Texture = Shuriken.Rendering.Texture;

namespace ConverseEditor.ShurikenRenderer
{
    public class ConverseProject
    {
        public struct SViewportData
        {
            public int csdRenderTextureHandle;
            public Vector2i framebufferSize;
            public int renderbufferHandle;
            public int framebufferHandle;
        }
        public struct SProjectConfig
        {
            public string WorkFilePath;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
        }
        public List<Window> windowList = new List<Window>();
        public Renderer renderer;
        public Vector2 viewportSize;
        public Vector2 screenSize;
        public FCO fcoFile;
        public FTE fteFile;
        public SProjectConfig config;
        private SViewportData viewportData;
        public bool isFileLoaded = false;

        public ConverseProject(GameWindow window2, Vector2 in_ViewportSize, Vector2 clientSize)
        {
            viewportSize = in_ViewportSize;
            viewportData = new SViewportData();
            config = new SProjectConfig();
            screenSize = clientSize;
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
        public void LoadFile(string in_Path, string in_PathFte)
        {
            config.WorkFilePath = in_Path;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var pathTable = Path.Combine("tables", "Languages", "English", "Retail", "WorldMap.json");
            try
            {
                fcoFile = FCO.Read(config.WorkFilePath, pathTable, false);
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                fcoFile = null;
                fteFile = null;
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FCO file.\n{ex.Message}", 2);
                return;
            }
            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds}s");
            try
            {
                fteFile = FTE.Read(in_PathFte);
            }
            catch (Exception ex)
            {
                isFileLoaded = false;
                fcoFile = null;
                fteFile = null;
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load the FTE file.\n{ex.Message}", 2);
                return;
            }

            string parentPath = Directory.GetParent(config.WorkFilePath).FullName;
            SpriteHelper.textureList = new("");
            List<string> missingTextures = new List<string>();
            foreach (var texture in fteFile.Textures)
            {
                string pathtemp = Path.Combine(parentPath, texture.Name + ".dds");
                if (File.Exists(pathtemp))
                    SpriteHelper.textureList.Textures.Add(new Texture(pathtemp, false));
                else
                {
                    var commonPathTexture = Path.Combine(Program.programDir,"Resources","CommonTextures",texture.Name + ".dds");
                    if (File.Exists(commonPathTexture))
                    {
                        SpriteHelper.textureList.Textures.Add(new Texture(commonPathTexture, false));
                    }
                    else
                    {
                        SpriteHelper.textureList.Textures.Add(new Texture("", false));
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
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }
        public void SaveCurrentFile(string in_Path)
        {
            if(fcoFile != null)
                fcoFile.Write(in_Path);
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