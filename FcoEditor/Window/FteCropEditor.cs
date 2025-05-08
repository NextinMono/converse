using Converse.Rendering;
using Converse.ShurikenRenderer;
using Hexa.NET.ImGui;
using Octokit;
using System;
using System.Numerics;

namespace Converse
{
    public static class FteCropEditor
    {
        public static float ZoomFactor = 1;
        public static bool Enabled = false;
        static int m_SelectedIndex = 0;
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
                    Texture texture = SpriteHelper.Textures[m_SelectedIndex];
                    ImGui.SeparatorText("Texture Info");
                    ImGui.Text($"Name: {texture.Name}");
                    ImGui.Text($"Width: {texture.Width}");
                    ImGui.Text($"Height: {texture.Height}");
                    ImGui.SeparatorText("Crop");
                    ImGui.Text($"Currently editing: Crop ({m_SelectedSpriteIndex})");
                    if(texture.CropIndices.Count != 0)
                    {
                        var sprite = SpriteHelper.Sprites[texture.CropIndices[m_SelectedSpriteIndex]];
                        ImGui.Text($"Converse ID: {SpriteHelper.GetConverseIDFromSprite(sprite)}");
                        Vector2 spriteStart = sprite.Start;
                        Vector2 spriteSize = sprite.Dimensions; ImGui.DragFloat2("Position", ref spriteStart, "%.0f");
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
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0.7f, 1, 1)));
                ImGui.TextWrapped("A translation table is necessary to be able to edit text from FCOs, as they do not store the character used to type out the sentences.");
                ImGui.PopStyleColor();
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