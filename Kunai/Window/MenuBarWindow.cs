using Hexa.NET.ImGui;
using IconFonts;
using FcoEditor.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FcoEditor
{
    public class MenuBarWindow : Window
    {
        internal static MenuBarWindow instance;
        public static MenuBarWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MenuBarWindow();
                }
                return instance;
            }
        }
        public static float menuBarHeight = 32;
        private readonly string fco = "fco";
        private readonly string fte = "fte";

        public string AskForFTE(string in_FcoPath)
        {
            var possibleFtePath = Path.Combine(Directory.GetParent(in_FcoPath).FullName, "fte_ConverseMain.fte");
            //if (!File.Exists(possibleFtePath))
            //{
                var testdial2 = NativeFileDialogSharp.Dialog.FileOpen(fte, Directory.GetParent(in_FcoPath).FullName);
                if (testdial2.IsOk)
                {
                    possibleFtePath = testdial2.Path;
                }
            //}
            return possibleFtePath;
        }
        public override void Render(ShurikenRenderHelper in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open File..."))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileOpen(fco);
                        if (testdial.IsOk)
                        {
                            var possibleFtePath = AskForFTE(testdial.Path);
                            in_Renderer.LoadFile(@testdial.Path, possibleFtePath);
                        }
                    }
                    if (ImGui.MenuItem("Save", "Ctrl + S"))
                    {
                        in_Renderer.SaveCurrentFile(in_Renderer.config.WorkFilePath);
                    }
                    ImGui.EndMenu();
                }


            }
            ImGui.EndMainMenuBar();
        }
    }
}
