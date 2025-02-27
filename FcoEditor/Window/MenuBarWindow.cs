using Hexa.NET.ImGui;
using ConverseEditor.ShurikenRenderer;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
        //https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
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
            if (UpdateChecker.UpdateAvailable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0.7f, 1, 1)));
                var size = ImGui.CalcTextSize("Update Available!").X;
                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - size - ImGui.GetStyle().ItemSpacing.X * 2);
                if (ImGui.Selectable("Update Available!"))
                {
                    OpenUrl("https://github.com/NextinMono/converse/releases/latest");
                }
                ImGui.PopStyleColor();
            }
            ImGui.EndMainMenuBar();
        }
    }
}
