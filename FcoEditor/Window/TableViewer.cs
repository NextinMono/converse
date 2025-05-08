using Converse.Rendering;
using Converse.ShurikenRenderer;
using Hexa.NET.ImGui;
using Octokit;
using System;
using System.Numerics;

namespace Converse
{
    public static class TableViewer
    {
        static int selectedBox = 0;
        static int currentTextureIdx = 0;
        static float zoomFactor = 0;
        static ConverseProject project;
        public static void Render(ConverseProject renderer)
        {
            project = renderer;
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
                    cursor.X = renderer.screenSize.X / 4;
                    cursor.Y += renderer.screenSize.Y - (renderer.screenSize.Y / 3);
                    if (ImConverse.BeginListBoxCustom("##tableview", new Vector2(renderer.screenSize.X / 2, renderer.screenSize.Y - (renderer.screenSize.Y / 3))))
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
                                CharacterSprite? spr = SpriteHelper.GetCharaSpriteFromID(translationTableNew[i].ConverseID);
                                if (spr == null)
                                    continue;
                                var letter = translationTableNew[i];

                                ImGui.TableSetColumnIndex(0);
                                ImGui.SetNextItemWidth(-1);
                                ImGui.InputText($"##input{letter.ConverseID}", ref letter.Letter, 256);
                                if (ImGui.IsItemActive())
                                    selectedBox = i;
                                ImGui.TableSetColumnIndex(1);
                                if (spr.Value.sprite.Texture.GlTex != null)
                                    ImConverse.DrawConverseCharacter(spr.Value, renderer.config.fteFile, new Vector4(1, 1, 1, 1), 0, 1);
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
                    CharacterSprite? spr2 = SpriteHelper.GetCharaSpriteFromID(translationTableNew[selectedBox].ConverseID);

                    if(spr2 != null)
                    {
                        if (!spr2.Value.sprite.IsNull())
                        {
                            currentTextureIdx = SpriteHelper.Textures.IndexOf(spr2.Value.sprite.Texture);
                            var avgSizeWin = (ImGui.GetContentRegionAvail().X / 2);
                            Vector2 availableSize = new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetContentRegionAvail().Y);
                            Vector2 viewportPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                            var textureSize = SpriteHelper.Textures[currentTextureIdx].Size;

                            Vector2 imageSize;
                            if (textureSize.X > textureSize.Y)
                                imageSize = new Vector2(availableSize.Y, (textureSize.Y / textureSize.X) * availableSize.Y);
                            else
                                imageSize = new Vector2(availableSize.X, (textureSize.X / textureSize.Y) * availableSize.X);

                            //Texture Image
                            var size2 = ImGui.GetContentRegionAvail().X - avgSizeWin - 20;
                            ImGui.SameLine();
                            ImConverse.ImageViewport("##cropEdit", new Vector2(size2, -1), SpriteHelper.Textures[currentTextureIdx].Size.Y / SpriteHelper.Textures[currentTextureIdx].Size.X, zoomFactor, new ImTextureID(SpriteHelper.Textures[currentTextureIdx].GlTex.Id), DrawQuadList, new Vector4(0.5f, 0.5f, 0.5f, 1));

                            bool windowHovered = ImGui.IsItemHovered() && ImGui.IsKeyDown(ImGuiKey.ModCtrl);
                            if (windowHovered)
                                zoomFactor += ImGui.GetIO().MouseWheel / 5;

                            zoomFactor = Math.Clamp(zoomFactor, 0.5f, 10);
                            ImGui.SetCursorPosY(cursor.Y);
                            ImGui.NewLine();
                            ImGui.NewLine();
                            ImConverse.DrawConverseCharacter(spr2.Value, renderer.config.fteFile, new Vector4(1, 1, 1, 1), cursor.X - spr2.Value.sprite.Dimensions.X / 2, 2);
                            ImGui.NewLine();
                            string text = "Converse ID: " + "{" + translationTableNew[selectedBox].ConverseID.ToString() + "}";
                            float textWidth = ImGui.CalcTextSize(text).X;
                            float centeredX = cursor.X + (spr2.Value.sprite.Dimensions.X - textWidth) / 2;
                            ImGui.SetCursorPosX(centeredX);
                            ImGui.Text(text);
                        }
                    }
                    
                }
            }
            else
            {
                ImGui.Text("Open an FCO file to make a translation table for it.");
            }
            ImGui.EndTabItem();
        }
        private static void DrawQuadList(SCenteredImageData in_Data)
        {
            var cursorpos = ImGui.GetItemRectMin();
            Vector2 screenPos = in_Data.Position + in_Data.ImagePosition - new Vector2(3, 2);
            var viewSize = in_Data.ImageSize;


            Sprite sprite = SpriteHelper.GetSpriteFromConverseID(project.config.translationTable[selectedBox].ConverseID);
            var qTopLeft = sprite.Crop.TopLeft;
            var qTopRight = new Vector2(sprite.Crop.BottomRight.X, sprite.Crop.TopLeft.Y);
            var qBotLeft = new Vector2(sprite.Crop.TopLeft.X, sprite.Crop.BottomRight.Y);
            var qBotRight = sprite.Crop.BottomRight;
            Vector2 pTopLeft = screenPos + new Vector2(qTopLeft.X * viewSize.X, qTopLeft.Y * viewSize.Y);
            Vector2 pBotRight = screenPos + new Vector2(qBotRight.X * viewSize.X, qBotRight.Y * viewSize.Y);
            Vector2 pTopRight = screenPos + new Vector2(qTopRight.X * viewSize.X, qTopRight.Y * viewSize.Y);
            Vector2 pBotLeft = screenPos + new Vector2(qBotLeft.X * viewSize.X, qBotLeft.Y * viewSize.Y);

            ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.3f, 0, 1)), 3);

           
        }
    }
}