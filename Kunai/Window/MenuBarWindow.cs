using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public static class MenuBarWindow
    {
        public static float menuBarHeight = 32;
        private static readonly string filters = "xncp,yncp,gncp;xncp;yncp;gncp";

        public static void Render(ShurikenRenderHelper in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBarWindow.menuBarHeight = ImGui.GetWindowSize().Y;

                MenuBarWindow.menuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open CSD Project..."))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileOpen(filters);
                        if (testdial.IsOk)
                        {
                            in_Renderer.LoadFile(@testdial.Path);
                        }
                    }
                    if (ImGui.MenuItem("Save", "Ctrl + S"))
                    {
                        in_Renderer.SaveCurrentFile(null);
                    }
                    ImGui.EndMenu();
                }


            }
            ImGui.EndMainMenuBar();
        }
    }
}
