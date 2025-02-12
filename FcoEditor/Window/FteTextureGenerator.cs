using ConverseEditor.ShurikenRenderer;
using DirectXTexNet;
using Hexa.NET.ImGui;
using System;
using System.Numerics;

namespace ConverseEditor
{
    public static class FteTextureGenerator
    {
        static FontAtlasSettings Settings = new FontAtlasSettings();

        public static unsafe void Draw(ConverseProject in_Renderer)
        {
            ImGui.BeginDisabled(!FcoViewerWindow.Instance.tablePresent);
            ImGui.InputFloat("Font Size", ref Settings.FontSize);
            ImGui.InputFloat("Kerning (Character Spacing)", ref Settings.Kerning);
            ImGui.InputFloat2("Character Texture Spacing", ref Settings.InterCharacterSpacing, "%.0f");
            ImGui.InputFloat2("Size", ref Settings.FontAtlasSize);
            ImGui.InputText("Font Path", ref Settings.FontPath, 1024);
            ImGui.SameLine();
            if (ImGui.Button("..."))
            {
                var dialog1 = NativeFileDialogSharp.Dialog.FileOpen("otf, ttf");
                if (dialog1.IsOk)
                    Settings.FontPath = dialog1.Path;                
            }
            if(ImGui.Button("Generate"))
            {
                Settings.FtePath = ConverseProject.config.WorkFilePathFTE;
                try
                {
                    FontAtlasGenerator.TryCreateFteTexture(Settings, FcoViewerWindow.Instance.GetTranslationTableEntries(), in_Renderer.fteFile);
                }
                catch(Exception e)
                {

                }
                in_Renderer.ShowMessageBoxCross("TEMPORARY", "[TEMP] For now, you have to open the png file that has been created in the same directory as the fte, and resave it as a DDS.");
            }
            ImGui.EndDisabled();
        }
    }
}
