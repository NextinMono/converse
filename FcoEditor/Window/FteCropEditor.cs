using Converse.Rendering;
using Converse.ShurikenRenderer;
using HekonrayBase;
using Hexa.NET.ImGui;
using System;
using System.Numerics;

namespace Converse
{
    public static class FteCropEditor
    {
        public static float ZoomFactor = 1;
        public static bool Enabled = false;
        static int m_SelectedIndex = 2;
        static int m_SelectedSpriteIndex = 0;
        static bool m_ShowAllCoords;
        public static void Reset()
        {
            m_SelectedIndex = 0;
            m_SelectedSpriteIndex = 0;
        }
        private static void DrawQuadList(SCenteredImageData in_Data)
        {
            var cursorpos = ImGui.GetItemRectMin();
            Vector2 screenPos = in_Data.Position + in_Data.ImagePosition - new Vector2(3, 2);
            var viewSize = in_Data.ImageSize;

            for (int i = 0; i < SpriteHelper.Textures[m_SelectedIndex].CropIndices.Count; i++)
            {
                int spriteIdx = SpriteHelper.Textures[m_SelectedIndex].CropIndices[i];
                var sprite = SpriteHelper.Sprites[spriteIdx];
                var qTopLeft = sprite.Crop.TopLeft;
                var qTopRight = new Vector2(sprite.Crop.BottomRight.X, sprite.Crop.TopLeft.Y);
                var qBotLeft = new Vector2(sprite.Crop.TopLeft.X, sprite.Crop.BottomRight.Y);
                var qBotRight = sprite.Crop.BottomRight;
                Vector2 pTopLeft = screenPos + new Vector2(qTopLeft.X * viewSize.X, qTopLeft.Y * viewSize.Y);
                Vector2 pBotRight = screenPos + new Vector2(qBotRight.X * viewSize.X, qBotRight.Y * viewSize.Y);
                Vector2 pTopRight = screenPos + new Vector2(qTopRight.X * viewSize.X, qTopRight.Y * viewSize.Y);
                Vector2 pBotLeft = screenPos + new Vector2(qBotLeft.X * viewSize.X, qBotLeft.Y * viewSize.Y);

                Vector2 mousePos = ImGui.GetMousePos();

                if (m_ShowAllCoords)
                    ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), 1.5f);

                if (ConverseMath.IsPointInRect(mousePos, pTopLeft, pTopRight, pBotRight, pBotLeft) || i == m_SelectedSpriteIndex)
                {
                    //Add selection box
                    ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.3f, 0, 1)), 3);
                    if (ImGui.IsMouseClicked(0))
                    {
                        m_SelectedSpriteIndex = i;
                    }
                    //if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                    //{
                    //    Vector2 mouseInWindow = mousePos - in_WindowPos;
                    //
                    //    Vector2 adjustedMousePos = mouseInWindow - screenPos;
                    //    quad.OriginalData.OriginCast.Position = adjustedMousePos / Renderer.ViewportSize;
                    //}
                }

            }
        }

        private static void CropRegionEditor(ConverseProject renderer, float in_AvgSizeWin)
        {
            if (ImGui.BeginListBox("##texturelist2", new Vector2(in_AvgSizeWin, -1)))
            {
                if (SpriteHelper.Textures.Count > m_SelectedIndex)
                {
                    // Texture "header"
                    Texture texture = SpriteHelper.Textures[m_SelectedIndex];
                    ImGui.SeparatorText("Texture Info");
                    ImGui.BeginDisabled(true);
                    string texName = texture.Name;
                    ImGui.InputText($"Name", ref texName, 512);
                    Vector2 texSizeDds = texture.Size;
                    ImGui.InputFloat2($"Size", ref texSizeDds);
                    ImGui.EndDisabled();
                    if(m_SelectedSpriteIndex >= texture.CropIndices.Count)
                    {
                        m_SelectedSpriteIndex = texture.CropIndices.Count - 1;
                    }
                    // Crop info section
                    if(texture.CropIndices.Count != 0)
                    {
                        var sprite = SpriteHelper.Sprites[texture.CropIndices[m_SelectedSpriteIndex]];
                        var chara = SpriteHelper.GetCharaSpriteFromID(SpriteHelper.GetConverseIDFromSprite(sprite)).Value;
                        var charaIdx = SpriteHelper.ConverseSprites.IndexOf(chara);
                        var fteTex = renderer.config.fteFile.Textures[chara.converseChara.TextureIndex];
                        var fteTexSize = fteTex.Size;
                        int convId = chara.converseChara.CharacterID;
                        Vector2 spriteStart = sprite.Start;
                        Vector2 spriteSize = sprite.Dimensions;
                        ImGui.SeparatorText("Font Texture");
                        ImGui.InputFloat2("Display Size", ref fteTexSize);
                        ImGui.SetItemTooltip("This is used ingame to display the characters at a reasonable size.\nThe formula the game uses is:\n(spriteDimension / ddsTexSize) * fteTextureSize");
                        ImGui.SeparatorText("Crop");
                        ImGui.Text($"Currently editing: Crop ({m_SelectedSpriteIndex})");
                        ImGui.InputInt("Converse ID", ref convId);
                        ImGui.DragFloat2("Position", ref spriteStart, "%.0f");
                        ImGui.DragFloat2("Dimension", ref spriteSize, "%.0f");
                        if (!texture.IsEmpty())
                        {
                            spriteStart.X = Math.Clamp(spriteStart.X, 0, texture.Size.X);
                            spriteStart.Y = Math.Clamp(spriteStart.Y, 0, texture.Size.Y);

                            spriteSize.X = Math.Clamp(spriteSize.X, 1, texture.Size.X);
                            spriteSize.Y = Math.Clamp(spriteSize.Y, 1, texture.Size.Y);
                            sprite.Start = spriteStart;
                            sprite.Dimensions = spriteSize;
                            sprite.Recalculate();
                        }
                        else
                        {
                            sprite.Start = spriteStart;
                            sprite.Dimensions = spriteSize;
                        }

                        fteTex.Size = fteTexSize;
                        renderer.config.fteFile.Textures[chara.converseChara.TextureIndex] = fteTex;
                        chara.converseChara.CharacterID = convId;
                        SpriteHelper.ConverseSprites[charaIdx] = chara;
                    }                    
                }
                ImGui.EndListBox();
            }
        }
        private static void ImageViewportTexture(ConverseProject in_Renderer, float in_AvgSizeWin)
        {
            if (!in_Renderer.IsFteLoaded())
                return;
            Vector2 availableSize = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetContentRegionAvail().Y);
            Vector2 viewportPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
            var textureSize = SpriteHelper.Textures[m_SelectedIndex].Size;

            Vector2 imageSize;
            if (textureSize.X > textureSize.Y)
                imageSize = new Vector2(availableSize.Y, (textureSize.Y / textureSize.X) * availableSize.Y);
            else
                imageSize = new Vector2(availableSize.X, (textureSize.X / textureSize.Y) * availableSize.X);

            //Texture Image
            var size2 = ImGui.GetContentRegionAvail().X - in_AvgSizeWin;
            ImConverse.ImageViewport("##cropEdit", new Vector2(size2, -1), ZoomFactor, SpriteHelper.Textures[m_SelectedIndex], DrawQuadList, new Vector4(0.5f, 0.5f, 0.5f, 1));

            bool windowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && ImGui.IsItemHovered();
            if (windowHovered)
                ZoomFactor += ImGui.GetIO().MouseWheel / 5;
        }

        private static void CropListLeft(ConverseProject in_Renderer, float in_AvgSizeWin)
        {
            if (ImGui.BeginListBox("##texturelist", new Vector2(in_AvgSizeWin, -1)))
            {
                int idx = 0;
                var result = ImConverse.TextureSelector(in_Renderer, true);
                if (result.TextureIndex != -2)
                    m_SelectedIndex = result.TextureIndex;
                if (result.SpriteIndex != -2)
                    m_SelectedSpriteIndex = result.SpriteIndex;
                ImGui.EndListBox();
            }
        }
        public static void Render(ConverseProject renderer)
        {
            if (renderer.IsFteLoaded())
            {
                if(ImGui.Button("Add Texture"))
                {
                    var res = NativeFileDialogSharp.Dialog.FileOpen("dds");
                    if (res.IsOk)
                    {
                        if (!SpriteHelper.DoesTextureExist(res.Path))
                        {
                            SpriteHelper.AddTexture(new Texture(res.Path), true);
                        }
                        else
                        {
                            Application.ShowMessageBoxCross("Error", "A texture with this exact name already exists!\nPlease rename the target texture and try again.");
                        }
                    }
                }
                //ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0.7f, 1, 1)));
                //ImGui.TextWrapped("A translation table is necessary to be able to edit text from FCOs, as they do not store the character used to type out the sentences.");
                //ImGui.PopStyleColor();
                var padding = ImGui.GetStyle().ItemSpacing;
                var avgSizeWin = (ImGui.GetWindowSize().X / 4) - padding.X;
                ZoomFactor = Math.Clamp(ZoomFactor, 0.5f, 5);
                CropListLeft(renderer, avgSizeWin);
                ImGui.SameLine();
                ImageViewportTexture(renderer, avgSizeWin);
                ImGui.SameLine();
                CropRegionEditor(renderer, avgSizeWin);
            }
            else
            {
                ImGui.Text("Open an FCO file to make a translation table for it.");
            }
            ImGui.EndTabItem();
        }
    }
}