using Hexa.NET.ImGui;
using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using ConverseEditor.Rendering;
using libfco;
using System;
using System.Collections.Generic;
using System.Numerics;
using HekonrayBase.Base;
using HekonrayBase;
namespace ConverseEditor
{
    public class ViewportWindow : Singleton<ViewportWindow>, IWindow
    {       

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (ConverseProject)in_Renderer;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(renderer.screenSize.X, renderer.screenSize.Y - MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("##FCOViewerWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                if (FindReplaceTool.Enabled)
                {
                    FindReplaceTool.Render(renderer);
                }

                if (ImGui.BeginTabBar("##tabsfco"))
                {
                    bool isFcoLoaded = renderer.config.fcoFile != null;
                    if (ImGui.BeginTabItem("FCO Viewer"))
                    {
                        FcoViewer.Render(renderer);
                    }
                    if (ImGui.BeginTabItem("Translation Table"))
                    {
                        TableViewer.Render(renderer);
                    }
                    if (ImGui.BeginTabItem("FTE Generator"))
                    {
                        FteTextureGenerator.Draw(renderer);
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }
        public void OnReset(IProgramProject in_Renderer)
        {
            var renderer = (ConverseProject)in_Renderer;
            renderer.config.translationTable.Clear();

        }
    }
}