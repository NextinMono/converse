using Hexa.NET.ImGui;
using ConverseEditor.ShurikenRenderer;
using System.IO;
using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using ConverseEditor.Settings;
using TeamSpettro.SettingsSystem;
using HekonrayBase;
using System.Runtime.InteropServices;
using System.Numerics;

namespace ConverseEditor
{
    public class MainWindow : HekonrayMainWindow
    {
        private IntPtr m_IniName;
        private string m_AppName = "Kunai";
        public ConverseProject ConverseProject => (ConverseProject)Project;
        public static ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;

        public MainWindow(Version in_OpenGlVersion, Vector2Int in_WindowSize) : base(in_OpenGlVersion, in_WindowSize)
        {
            Title = m_AppName;
        }

        public override void OnLoad()
        {
            OnActionWithArgs = LoadFromArgs;
            TeamSpettro.Resources.Initialize(Path.Combine(Program.Path, "config.json"));
            Project = new ConverseProject(this);
            base.OnLoad();

            ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
            // Example #10000 for why ImGui.NET is kinda bad
            // This is to avoid having imgui.ini files in every folder that the program accesses
            unsafe
            {
                m_IniName = Marshal.StringToHGlobalAnsi(Path.Combine(Program.Path, "imgui.ini"));
                ImGuiIOPtr io = ImGui.GetIO();
                io.IniFilename = (byte*)m_IniName;
            }
            //    converseProject.windowList.Add(MenuBarWindow.Instance);
            //    converseProject.windowList.Add(FcoViewerWindow.Instance);
            //    converseProject.windowList.Add(SettingsWindow.Instance);
            Windows.Add(ModalHandler.Instance);
            Windows.Add(new MenuBarWindow());
            Windows.Add(new FcoViewerWindow());
            Windows.Add(new SettingsWindow());
            SettingsWindow.Instance.OnReset(null);
        }

        private void LoadFromArgs(string[] in_Args)
        {
            string pathFTE = MenuBarWindow.Instance.AskForFTE(in_Args[0]);
            ConverseProject.LoadFile(in_Args[0], pathFTE);
        }

        //protected override void OnResize(ResizeEventArgs in_E)
        //{
        //    base.OnResize(in_E);
        //    if(KunaiProject != null)
        //        KunaiProject.ScreenSize = new System.Numerics.Vector2(ClientSize.X, ClientSize.Y);
        //}
        //
        public override void OnRenderImGuiFrame()
        {
            if (ShouldRender())
            {
                base.OnRenderImGuiFrame();

                //float deltaTime = (float)(GetDeltaTime());
                //co.Render(KunaiProject.WorkProjectCsd, (float)deltaTime);

               //if (converseProject.isFileLoaded)
               //   Title = m_AppName + $" - [{converseProject.WorkFilePath}]";
               //else
                    Title = m_AppName;
            }
        }
    }
    //protected override void OnLoad()
    //{
    //    base.OnLoad();
    //    TeamSpettro.Resources.Initialize(Path.Combine(Program.Directory, "config.json"));
    //    converseProject = new ConverseProject(this, new ShurikenRenderer.Vector2(1280, 720), new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y));
    //
    //    Title = applicationName;
    //    _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
    //    ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
    //    converseProject.windowList.Add(MenuBarWindow.Instance);
    //    converseProject.windowList.Add(FcoViewerWindow.Instance);
    //    converseProject.windowList.Add(SettingsWindow.Instance);
    //    if (Program.arguments.Length > 0)
    //    {
    //        string pathFTE = MenuBarWindow.Instance.AskForFTE(Program.arguments[0]);
    //        converseProject.LoadFile(Program.arguments[0], pathFTE);
    //    }
    //
    //}

}
