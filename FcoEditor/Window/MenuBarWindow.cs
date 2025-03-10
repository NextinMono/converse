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
                    if (ImGui.MenuItem("Save As...", "Ctrl + Alt + S"))
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
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Associate extensions"))
                    {
                        ExecuteAsAdmin(@Path.Combine(@Program.Directory, "FileTypeRegisterService.exe"));
                    }
                    if (ImGui.MenuItem("Preferences", SettingsWindow.Enabled))
                    {
                        SettingsWindow.Enabled = !SettingsWindow.Enabled;
                    }
                    if (ImGui.MenuItem("Render", FindReplaceTool.Enabled))
                    {
                        FindReplaceTool.Enabled = !FindReplaceTool.Enabled;
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
            ImGui.EndMainMenuBar();
        }
        public static string AddQuotesIfRequired(string in_Path)
        {
            return !string.IsNullOrWhiteSpace(in_Path) ?
                in_Path.Contains(" ") && (!in_Path.StartsWith("\"") && !in_Path.EndsWith("\"")) ?
                    "\"" + in_Path + "\"" : in_Path :
                    string.Empty;
        }
        public static void ExecuteAsAdmin(string in_FileName)
        {
            //Reason for this try-catch statement is because
            //if the user cancels the UAC prompt,
            //an exception will be thrown
            try
            {
                in_FileName = AddQuotesIfRequired(in_FileName);
                Process proc = new Process();
                proc.StartInfo.FileName = in_FileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch(Exception e)
            {

            }
        }
    }
}
