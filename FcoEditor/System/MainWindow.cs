﻿using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ConverseEditor.ShurikenRenderer;
using System.IO;
using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using ConverseEditor.Settings;
using TeamSpettro.SettingsSystem;

namespace ConverseEditor
{
    public class MainWindow : GameWindow
    {
        public static readonly string applicationName = "Converse";
        ImGuiController _controller;
        public static ConverseProject renderer;
        public byte[] IconData;
        public static uint viewportDock;
        public static ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;
       
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(800, 1000), APIVersion = new Version(3, 3) })
        {
            MemoryStream ms = new MemoryStream();
            Title = applicationName;
            GetIcon();
        }

        void GetIcon()
        {
            // TODO: eventually replace with program's own embedded icon?
            using SixLabors.ImageSharp.Image<Rgba32> newDds = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine(Program.Directory, "Resources", "Icons", "ico.png"));
            IconData = new byte[newDds.Width * newDds.Height * Unsafe.SizeOf<Rgba32>()];
            newDds.CopyPixelDataTo(IconData);

            OpenTK.Windowing.Common.Input.Image windowIcon = new OpenTK.Windowing.Common.Input.Image(newDds.Width, newDds.Height, IconData);
            Icon = new OpenTK.Windowing.Common.Input.WindowIcon(windowIcon);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            TeamSpettro.Resources.Initialize(Path.Combine(Program.Directory, "config.json"));
            renderer = new ConverseProject(this, new ShurikenRenderer.Vector2(1280, 720), new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y));

            Title = applicationName;
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
            renderer.windowList.Add(MenuBarWindow.Instance);
            renderer.windowList.Add(FcoViewerWindow.Instance);
            renderer.windowList.Add(SettingsWindow.Instance);
            if (Program.arguments.Length > 0)
            {
                string pathFTE = MenuBarWindow.Instance.AskForFTE(Program.arguments[0]);
                renderer.LoadFile(Program.arguments[0], pathFTE);
            }

        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            renderer.screenSize = new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (renderer.screenSize.X != 0 && renderer.screenSize.Y != 0)
            {
                if (IsFocused)
                {
                    base.OnRenderFrame(e);
                    _controller.Update(this, (float)e.Time);

                    GL.ClearColor(new Color4(0, 0, 0, 255));
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.Blend);
                    GL.Disable(EnableCap.CullFace);
                    GL.BlendEquation(BlendEquationMode.FuncAdd);
                    // Enable Docking
                    viewportDock = ImGui.DockSpaceOverViewport();

                    if (renderer.isFileLoaded)
                    {
                        Title = $"{applicationName} - [{ConverseProject.config.WorkFilePath}]";
                    }
                    else
                    {
                        Title = applicationName;
                    }
                    renderer.RenderWindows();
                    _controller.Render();

                    ImGuiController.CheckGLError("End of frame");

                }
            }
            SwapBuffers();
        }
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);


            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _controller.MouseScroll(e.Offset);
        }
    }
}
