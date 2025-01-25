using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SharpNeedle.Ninja.Csd;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using System.Windows;
using Hexa.NET.ImPlot;
using OpenTK.Windowing.Common.Input;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Kunai
{
    public class MainWindow : GameWindow
    {
        public static readonly string applicationName = "Test";
        private static MemoryStream iconData;
        float test = 1;
        ImGuiController _controller;
        public static ShurikenRenderHelper renderer;
        public static ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings(){ Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
        { }
        protected override void OnLoad()
        {
            base.OnLoad();
            renderer = new ShurikenRenderHelper(this, new ShurikenRenderer.Vector2(1280, 720), new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y));

            Title = applicationName;
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            if (Program.arguments.Length > 0)
            {
                renderer.LoadFile(Program.arguments[0]);
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
        uint BeginDockSpaceRegion(string in_Name, System.Numerics.Vector2 in_Position, System.Numerics.Vector2 in_Size)
        {
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.SetNextWindowPos(in_Position);
            ImGui.SetNextWindowSize(in_Size);
            ImGui.SetNextWindowBgAlpha(0);
            bool notused = true;
            ImGui.Begin(in_Name, ref notused, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoDocking);
            uint dockspaceId = ImGui.GetID(in_Name);
            ImGui.DockSpace(dockspaceId, new System.Numerics.Vector2(), ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingSplit);
            ImGui.End();

            return dockspaceId;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _controller.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 0, 0, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            // Enable Docking
            var ge = ImGui.DockSpaceOverViewport();

           

            
            MenuBarWindow.Render(renderer);
            ViewportWindow.Render(renderer);
            renderer.Render(0); 
            //if (renderer.WorkProjectCsd != null)
            //{
            //    Title = applicationName + $" - [{renderer.config.WorkFilePath}]";
            //    float deltaTime = (float)(e.Time);
            //    renderer.Render(renderer.WorkProjectCsd, (float)deltaTime);                
            //}
            //ImPlot.ShowDemoWindow();
            //if (ImGui.Begin("Testtt"))
            //{
            //    ImGui.End();
            //}

            _controller.Render();

            ImGuiController.CheckGLError("End of frame");

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
