using ConverseEditor.Rendering;
using ConverseEditor.ShurikenRenderer;
using Hexa.NET.ImGui;
using System;
using System.Numerics;

namespace ConverseEditor
{
    public static class TableViewer
    {
        public static void Render(ConverseProject renderer)
        {

            //TODO: split into new class
            if (renderer.IsFcoLoaded())
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0.7f, 1, 1)));
                ImGui.TextWrapped("A translation table is necessary to be able to edit text from FCOs, as they do not store the character used to type out the sentences.");
                ImGui.PopStyleColor();
                var size = (ImGui.GetContentRegionAvail().X / 3) - (ImGui.GetStyle().ItemSpacing.X);
                ImGui.SetCursorPosX(ImGui.GetStyle().ItemSpacing.X + 4);
                if (ImGui.Button("Import Table", new System.Numerics.Vector2(size, 32)))
                {
                    var testdial = NativeFileDialogSharp.Dialog.FileOpen("json");
                    if (testdial.IsOk)
                    {
                        renderer.ImportTranslationTable(@testdial.Path);
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Create Table", new System.Numerics.Vector2(size, 32)))
                {
                    renderer.CreateTranslationTable();
                }
                ImGui.SameLine();
                if (ImGui.Button("Save Table", new System.Numerics.Vector2(size, 32)))
                {
                    var testdial = NativeFileDialogSharp.Dialog.FileSave("json");
                    if (testdial.IsOk)
                    {
                        renderer.WriteTableToDisk(testdial.Path);
                    }
                }

                if (renderer.IsTableLoaded())
                {
                    ImGui.SeparatorText("Table");
                    if (ImGui.BeginTable("table2", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.ScrollY, new System.Numerics.Vector2(-1, -1)))
                    {
                        var translationTableNew = renderer.config.translationTable;
                        ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.None, 0);
                        ImGui.TableSetupColumn("Sprite", ImGuiTableColumnFlags.None, 0);
                        ImGui.TableHeadersRow();
                        ImGui.TableNextRow(0, 0);
                        ImGui.TableSetColumnIndex(0);
                        /// @separator
                        int maxAmount = Math.Clamp(translationTableNew.Count, 0, 1000);
                        for (int i = 0; i < maxAmount; i++)
                        {
                            Sprite spr = SpriteHelper.GetSpriteFromConverseID(translationTableNew[i].ConverseID);
                            if (spr == null)
                                continue;
                            var letter = translationTableNew[i];

                            ImGui.TableSetColumnIndex(0);
                            ImGui.SetNextItemWidth(-1);
                            ImGui.InputText($"##input{letter.ConverseID}", ref letter.Letter, 256);
                            ImGui.TableSetColumnIndex(1);
                            if (spr.Texture.GlTex != null)
                                ImConverse.DrawConverseCharacter(spr, new Vector4(1, 1, 1, 1), 0, 1);
                            else
                            {
                                ImGui.Text($"[Missing Texture (ID: {letter.ConverseID})]");
                            }
                            ImGui.TableNextRow();
                            translationTableNew[i] = letter;
                        }
                        if (translationTableNew.Count >= maxAmount)
                            ImGui.Text("There were too many entries to display, the rest have been cut off.");
                        /// @separator
                        ImGui.EndTable();
                    }
                }


            }
            else
            {
                ImGui.Text("Open an FCO file to make a translation table for it.");
            }
            ImGui.EndTabItem();
        }
    }
}