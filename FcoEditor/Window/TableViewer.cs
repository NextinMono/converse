using ConverseEditor.Rendering;
using ConverseEditor.ShurikenRenderer;
using Hexa.NET.ImGui;
using System;
using System.Numerics;

namespace ConverseEditor
{
    public static class TableViewer
    {
        static int selectedBox = 0;
        public static void Render(ConverseProject renderer)
        {
            //TODO: split into new class
            if (renderer.IsFteLoaded())
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
                    var translationTableNew = renderer.config.translationTable;
                    var cursor = ImGui.GetCursorPos();
                    cursor.X = renderer.screenSize.X / 2;
                    cursor.Y += renderer.screenSize.Y - (renderer.screenSize.Y / 3);
                    if (ImConverse.BeginListBoxCustom("##tableview", new Vector2(-1, renderer.screenSize.Y - (renderer.screenSize.Y / 3))))
                    {
                        if (ImGui.BeginTable("table2", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.ScrollY, new System.Numerics.Vector2(-1, -1)))
                        {
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
                                if (ImGui.IsItemActive())
                                    selectedBox = i;
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
                        ImConverse.EndListBoxCustom();
                    }
                    Sprite spr2 = SpriteHelper.GetSpriteFromConverseID(translationTableNew[selectedBox].ConverseID);

                    if (!spr2.IsNull())
                    {
                        cursor.X -= spr2.Dimensions.X / 2;
                        ImGui.SetCursorPosY(cursor.Y);
                        ImGui.NewLine();
                        ImGui.NewLine();
                        ImConverse.DrawConverseCharacter(spr2, new Vector4(1, 1, 1, 1), cursor.X, 2);

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