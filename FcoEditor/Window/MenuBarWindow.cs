using Hexa.NET.ImGui;
using ConverseEditor.ShurikenRenderer;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HekonrayBase.Base;
using HekonrayBase;

namespace ConverseEditor
{
    public class MenuBarWindow : Singleton<MenuBarWindow>, IWindow
    {
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

        public void OnReset(IProgramProject in_Renderer)
        {

        }
        void SaveFileAction(ConverseProject in_Renderer)
        {
            in_Renderer.SaveFcoFiles(null);
        }
        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (ConverseProject)in_Renderer;
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
                            renderer.LoadPairFile(@testdial.Path, possibleFtePath);
                        }
                    }
                    if (ImGui.MenuItem("Open folder"))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FolderPicker(fco);
                        if (testdial.IsOk)
                        {
                            renderer.LoadFolder(@testdial.Path);
                        }
                    }
                    if(ImGui.MenuItem("Open folder in Explorer", false, renderer.IsFteLoaded()))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = Directory.GetParent(renderer.config.ftePath).FullName,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save", "Ctrl + S"))
                    {
                        SaveFileAction(renderer);
                    }
                    if (ImGui.MenuItem("Save As...", "Ctrl + Alt + S"))
                    {
                        SaveFileAsAction(renderer);
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Find","Ctrl + F", FindReplaceTool.Enabled))
                    {
                        FindReplaceTool.SetActive(true, false);
                    }
                    if (ImGui.MenuItem("Find and Replace", "Ctrl + H", FindReplaceTool.Enabled))
                    {
                        FindReplaceTool.SetActive(true, true);
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Preferences", SettingsWindow.Enabled))
                    {
                        SettingsWindow.Enabled = !SettingsWindow.Enabled;
                    }
                    ImGui.EndMenu();
                }
                

                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("How to use Converse"))
                    {
                        OpenUrl("https://wiki.hedgedocs.com/index.php/How_to_use_Converse");
                    }
                    if (ImGui.MenuItem("Report a bug"))
                    {
                        OpenUrl("https://github.com/NextinMono/converse/issues/new");
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
            ProcessShortcuts(renderer);
            ImGui.EndMainMenuBar();
        }

        private void SaveFileAsAction(ConverseProject in_Renderer)
        {
            var testdial = NativeFileDialogSharp.Dialog.FileSave(fco);
            if (testdial.IsOk)
            {
                string path = testdial.Path;
                if (!Path.HasExtension(path))
                    path += ".fco";
                in_Renderer.SaveFcoFiles(path);
            };
        }

        private void ProcessShortcuts(ConverseProject in_Renderer)
        {
            if (ImGui.IsKeyDown(ImGuiKey.ModCtrl))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.F))
                {
                    FindReplaceTool.SetActive(true, false);
                }
                if (ImGui.IsKeyPressed(ImGuiKey.H))
                {
                    FindReplaceTool.SetActive(true, true);
                }
                if (ImGui.IsKeyPressed(ImGuiKey.S))
                {
                    if (ImGui.IsKeyDown(ImGuiKey.ModAlt))
                    {
                        SaveFileAsAction(in_Renderer);
                    }
                    else
                    {
                        SaveFileAction(in_Renderer);
                    }
                }
            };
        }
    }
}
