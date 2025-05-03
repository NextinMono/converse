using ConverseEditor.Rendering;
using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using Hexa.NET.ImGui;
using libfco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ConverseEditor
{
    public static class ImConverse
    {
        public static readonly int newLineValue = 0;
        public static void EmptyButton(int in_ConvID)
        {
            ImGui.SameLine(0);
            ImGui.SetNextItemWidth(50);
            ImGui.PushID($"{Random.Shared.Next(0, 1000)}_{in_ConvID}");
            ImGui.Button(in_ConvID.ToString());
            ImGui.PopID();
        }

        public static void CenterWindow(Vector2 in_Size)
        {
            // Calculate centered position
            var viewport = ImGui.GetMainViewport();
            System.Numerics.Vector2 centerPos = new System.Numerics.Vector2(
                viewport.WorkPos.X + (viewport.WorkSize.X - in_Size.X) * 0.5f,
                viewport.WorkPos.Y + (viewport.WorkSize.Y - in_Size.Y) * 0.5f
            );
            ImGui.SetNextWindowPos(centerPos);
            ImGui.SetNextWindowSize(in_Size);
        }
        public static void EndListBoxCustom()
        {
            ImGui.EndGroup();
            ImGui.EndChild();
        }
        public static float DrawConverseCharacter(Sprite spr, Vector4 in_Color, float in_OffsetX, float in_FontSize)
        {
            //TEMPORARY
            //Since the bg is black, if the text is black, itll be unreadable
            if (in_Color.X == 0 && in_Color.Y == 0 && in_Color.Z == 0 && in_Color.W != 0)
            {
                in_Color.X = 1;
                in_Color.Y = 1;
                in_Color.Z = 1;
            }
            Vector2 uvTL = new Vector2(
                        spr.Start.X / spr.Texture.Width,
                        -(spr.Start.Y / spr.Texture.Height));


            Vector2 uvBR = uvTL + new Vector2(
            spr.Dimensions.X / spr.Texture.Width,
            -(spr.Dimensions.Y / spr.Texture.Height));

            
                float width = (float)spr.Dimensions.X;
                //Draw sprite
                ImGui.SameLine(0, in_OffsetX);

                ImGui.Image(new ImTextureID(spr.Texture.GlTex.Id), new System.Numerics.Vector2(spr.Dimensions.X, spr.Dimensions.Y) * in_FontSize, uvTL, uvBR);
                
            
            return spr.Dimensions.Y * in_FontSize;
        }
        static void AlignForWidth(float width, float alignment = 0.5f)
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float off = (avail - width) * alignment;
            if (off > 0.0f)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);
                ImGui.PushStyleColor(ImGuiCol.Button, 0);
                var clicked = ImGui.Button("##invis", new System.Numerics.Vector2(1, 0));
                ImGui.PopStyleColor();
            }
        }
        static float GetOffsetFromAlignment(Cell in_Cell)
        {
            switch (in_Cell.Alignment)
            {
                case Cell.TextAlign.Justified:
                case Cell.TextAlign.Left:
                    return 0;
                case Cell.TextAlign.Center:
                    return 0.5f;
                case Cell.TextAlign.Right:
                    return 1;
            }
            return 0;
        }
        static void CalculateAlignmentSpacing(Cell in_Cell, int[] in_ConverseIDs,float in_FontSize, ref List<SLineInfo> in_LineWidths)
        {
            //Calculate the width and the amount of characters per line
            if (in_Cell.Alignment != Cell.TextAlign.Left)
            {
                int lineIndex = 0;
                in_LineWidths.Clear();
                foreach (var converseID in in_ConverseIDs)
                {
                    if (converseID == newLineValue)
                    {
                        lineIndex++;
                        continue;
                    }

                    Sprite spr = SpriteHelper.GetSpriteFromConverseID(converseID);
                    if (in_LineWidths.Count - 1 < lineIndex)
                        in_LineWidths.Add(new SLineInfo(0, 0));
                    in_LineWidths[lineIndex].width += spr.Width * in_FontSize;
                    in_LineWidths[lineIndex].amount++;
                }
                //Set the first line to be aligned
                AlignForWidth(in_LineWidths[0].width, GetOffsetFromAlignment(in_Cell));
            }
            else
                in_LineWidths.Clear();
        }
        static string GetMessageAsString(int[] in_IDs)
        {
            return string.Join(", ", in_IDs);
        }
        public static bool VisibilityNode(string in_Name, ref bool in_IsSelected, Action in_RightClickAction = null, bool in_ShowArrow = true, SIconData in_Icon = new(), string in_Id = "")
        {
            bool returnVal = true;
            bool idPresent = !string.IsNullOrEmpty(in_Id);
            string idName = idPresent ? in_Id : in_Name;
            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            var isLeaf = !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None;
            returnVal = ImGui.TreeNodeEx($"##{idName}header", isLeaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
            ImGui.PopStyleVar();
            //Rightclick action
            if (in_RightClickAction != null)
            {
                if (ImGui.BeginPopupContextItem())
                {
                    in_RightClickAction.Invoke();
                    ImGui.EndPopup();
                }
            }
            //Visibility checkbox
            //ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //ImGui.Checkbox($"##{idName}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)

            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.SetNextItemAllowOverlap();

            //Setup button so that the borders and background arent seen unless its hovered
            ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            bool iconPresent = !in_Icon.IsNull();
            in_IsSelected = ImGui.Button($"##invButton{idName}", new Vector2(-1, 25));
            ImGui.PopStyleColor(3);

            //Begin drawing text & icon if it exists
            ImGui.SetNextItemAllowOverlap();
            ImGui.PushID($"##text{idName}");
            ImGui.BeginGroup();

            if (iconPresent)
            {
                //Draw icon
                //ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0, 0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.TextColored(in_Icon.Color, in_Icon.Icon);
                //ImGui.PopFont();
                ImGui.SameLine(0, 0);
            }
            else
            {
                //Set size for the text as if there was an icon
                ImGui.SetCursorScreenPos(p + new Vector2(0, 2));
            }
            ImGui.SetNextItemAllowOverlap();
            ImGui.Text(iconPresent ? $" {in_Name}" : in_Name);

            ImGui.EndGroup();
            ImGui.PopID();
            return returnVal;
        }
        /// <summary>
        /// Fake list box that allows horizontal scrolling
        /// </summary>
        /// <param name="in_Label"></param>
        /// <param name="in_Size"></param>
        /// <returns></returns>
        public static bool BeginListBoxCustom(string in_Label, Vector2 in_Size)
        {
            bool returnVal = ImGui.BeginChild(in_Label, in_Size, ImGuiChildFlags.FrameStyle, ImGuiWindowFlags.HorizontalScrollbar);
            unsafe
            {
                //Ass Inc.
                //This is so that the child window has the same color as normal list boxes would
                ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.FrameBg)));
            }
            ImGui.BeginGroup();
            ImGui.PopStyleColor();
            return returnVal;
        }
        public static void InputTextCell(int[] in_ConverseIDs, string in_CellName, ref Cell in_Cell, List<TranslationTable.Entry> translationTableNew, int in_Index, int in_LineCount)
        {
            bool tablePresent = translationTableNew?.Count > 1;
            string cellMessageConverted = TranslationService.RawHEXtoTXT(in_ConverseIDs, translationTableNew);
            cellMessageConverted = cellMessageConverted.Replace("@@", "");
            if (ImGui.InputTextMultiline($"##{in_CellName}_{in_Index}text", ref cellMessageConverted, 512, new System.Numerics.Vector2(-1, ImGui.GetTextLineHeight() * in_LineCount)))
            {
                var joinedIDs2 = TranslationService.RawTXTtoHEX(cellMessageConverted, translationTableNew);
                in_Cell.Message = joinedIDs2;
            }

            if (!tablePresent)
            {
                if(ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("You need a Translation Table to be able to type text directly.");
                    ImGui.EndTooltip();
                }
            }
        }
        public static float DrawCellFromFTE(libfco.Cell in_Cell, int[] in_ConverseIDs, float in_FontSize, ref List<SLineInfo> in_LineWidths)
        {
            CalculateAlignmentSpacing(in_Cell, in_ConverseIDs, in_FontSize, ref in_LineWidths);
            int lineIdx = 0;
            float averageSize = -1;
            for (int i = 0; i < in_ConverseIDs.Length; i++)
            {
                int converseID = in_ConverseIDs[i];
                if (converseID == newLineValue)
                {
                    lineIdx++;
                    if (in_LineWidths.Count > lineIdx && in_Cell.Alignment != Cell.TextAlign.Justified)
                    {
                        AlignForWidth(in_LineWidths[lineIdx].width, GetOffsetFromAlignment(in_Cell));
                    }
                    else
                    {
                        ImGui.NewLine();
                    }
                    continue;
                }

                //In the case that a texture cant be found or if its unregistered through
                //SpriteHelper, print the converse id and skip
                Sprite spr = SpriteHelper.GetSpriteFromConverseID(converseID);
                if (spr == null)
                {
                    ImConverse.EmptyButton(converseID);
                    continue;
                }
                else
                {
                    if (spr.Texture.GlTex == null)
                    {
                        ImConverse.EmptyButton(converseID);
                        continue;
                    }
                    //Get the color to render the text with
                    CellColor currentHighlight = in_Cell.Highlights.FirstOrDefault(x => i >= x.Start && i <= x.End);
                    CellColor color = currentHighlight == null ? in_Cell.MainColor : currentHighlight;
                    //Calc spacing for justified text
                    float offset = 0;
                    if (in_Cell.Alignment == Cell.TextAlign.Justified)
                    {
                        offset = ((ImGui.GetContentRegionAvail().X - 50) - ((spr.Dimensions.X * in_FontSize) * in_LineWidths[lineIdx].amount)) / (in_LineWidths[lineIdx].amount - 1);
                    }
                    averageSize =  ImConverse.DrawConverseCharacter(spr, color.ArgbColor, offset, in_FontSize);
                }
            }
            return averageSize;
        }
    }
}
