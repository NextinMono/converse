using ConverseEditor.ShurikenRenderer;
using DirectXTexNet;
using Hexa.NET.ImGui;
using SixLabors.ImageSharp;
using System;
using System.Numerics;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace ConverseEditor
{
    public static class FteTextureGenerator
    {
        static FontAtlasSettings Settings = new FontAtlasSettings();

        public static unsafe void Draw(ConverseProject in_Renderer)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.7f, 0, 1)));
            ImGui.TextWrapped("WARNING: This tool is EXPERIMENTAL. It might save the FTE file incorrectly and cause crashes in-game!");
            ImGui.PopStyleColor();
            ImGui.BeginDisabled(!FcoViewerWindow.Instance.tablePresent);
            ImGui.InputFloat("Font Size", ref Settings.FontSize);
            ImGui.InputFloat("Kerning (Character Spacing)", ref Settings.Kerning);
            ImGui.InputFloat2("Character Texture Spacing", ref Settings.InterCharacterSpacing, "%.0f");
            ImGui.InputFloat2("Size", ref Settings.FontAtlasSize);
            ImGui.InputText("Font Path", ref Settings.FontPath, 1024);
            ImGui.SameLine();
            if (ImGui.Button("..."))
            {
                var dialog1 = NativeFileDialogSharp.Dialog.FileOpen("otf,ttf");
                if (dialog1.IsOk)
                    Settings.FontPath = dialog1.Path;                
            }
            var size = ImGui.GetContentRegionAvail().X;
            if(ImGui.Button("Generate", new System.Numerics.Vector2(size, 32)))
            {
                Settings.FtePath = ConverseProject.config.WorkFilePathFTE;
                try
                {
                    var atlasStream = FontAtlasGenerator.TryCreateFteTexture(Settings, FcoViewerWindow.Instance.GetTranslationTableEntries(), in_Renderer.fteFile);

                    var texturePath = Path.Combine(Directory.GetParent(Settings.FtePath).FullName, in_Renderer.fteFile.Textures[2].Name + ".dds");
                    if(File.Exists(texturePath))
                    {
                        File.Move(texturePath, Path.ChangeExtension(texturePath, ".old.dds"));
                    }
                    using Image<Rgba32> newDDS = Image.Load<Rgba32>(atlasStream.ToArray());

                    BcEncoder encoder = new BcEncoder();

                    encoder.OutputOptions.GenerateMipMaps = true;
                    encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
                    encoder.OutputOptions.Format = CompressionFormat.Bc3;
                    encoder.OutputOptions.FileFormat = OutputFileFormat.Dds; //Change to Dds for a dds file.

                    using FileStream fs = File.OpenWrite(texturePath);
                    encoder.EncodeToStream(newDDS.CloneAs<Rgba32>(), fs);
                }
                catch(Exception e)
                {
#if DEBUG
                    in_Renderer.ShowMessageBoxCross("Converse", $"A new font atlas could not be generated.\n{e.Message}", 2);
#else
                    throw;
#endif
                }
                in_Renderer.ShowMessageBoxCross("Converse", "A new font atlas and FTE file have been generated in the same folder as the FCO file.\nThe new FTE file is called \"fte_ConverseMain_Generated.fte\".");
            }
            ImGui.EndDisabled();
        }
    }
}
