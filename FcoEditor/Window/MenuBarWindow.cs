using Hexa.NET.ImGui;
using ConverseEditor.ShurikenRenderer;
using System;
using System.IO;

namespace ConverseEditor
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

            var testdial2 = NativeFileDialogSharp.Dialog.FileOpen(fte, Directory.GetParent(in_FcoPath).FullName);
            if (testdial2.IsOk)
            {
                possibleFtePath = testdial2.Path;
            }
            return possibleFtePath;
        }
        public override void Render(ConverseProject in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowSize().Y;

                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open"))
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
                        in_Renderer.SaveCurrentFile(ConverseProject.config.WorkFilePath);
                    }
                    if (ImGui.MenuItem("Save As...", "Ctrl + S"))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileSave(fco);
                        if (testdial.IsOk)
                        {
                            string path = testdial.Path;
                            if (!Path.HasExtension(path))
                                path += ".fco";
                            in_Renderer.SaveCurrentFile(path);
                        }
                    }
                    if (ImGui.MenuItem("Exit"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }


            }
            ImGui.EndMainMenuBar();
        }
    }
}
