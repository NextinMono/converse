using Hexa.NET.ImGui;
using Hexa.NET.Utilities.Text;
using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using Converse.Rendering;
using SUFcoTool;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
namespace ConverseEditor
{
    public class FcoViewerWindow : Window
    {
        internal static FcoViewerWindow instance;
        public static FcoViewerWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FcoViewerWindow();
                }
                return instance;
            }
        }
        readonly int newLineValue = 0;
        int selectedGroupIndex;
        float averageSize = 50;
        float fontSizeMultiplier = 1;
        bool expandAllCells = false;
        bool tablePresent = false;
        Random random = new Random();
        List<TranslationTable.Entry> translationTableNew = null;
        List<SLineInfo> lineWidth = new List<SLineInfo>();
        class SLineInfo
        {
            public float width;
            public int amount;

            public SLineInfo(float in_Width, int in_Amount)
            {
                this.width = in_Width;
                this.amount = in_Amount;
            }
        }

        static void EmptyButton(int in_ConvID)
        {
            ImGui.SameLine(0);
            ImGui.SetNextItemWidth(50);
            ImGui.Button(in_ConvID.ToString());
        }
        public override void OnReset(ConverseProject in_Renderer)
        {
            selectedGroupIndex = 0;
            tablePresent = false;
            translationTableNew = null;
        }
        void DrawGroupSelection(ConverseProject in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Groups");
            if (ImGui.BeginListBox("##groupslist", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 3, -1)))
            {
                if (in_FcoFilePresent)
                {
                    bool addNewGroup = false;
                    for (int i = 0; i < in_Renderer.fcoFile.Groups.Count; i++)
                    {
                        int selectedGroup = selectedGroupIndex;
                        if (selectedGroup == i)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0,1,0,1));
                        }
                        if (ImGui.Selectable(string.IsNullOrEmpty(in_Renderer.fcoFile.Groups[i].Name) ? $"Empty{i}" : in_Renderer.fcoFile.Groups[i].Name))
                            selectedGroupIndex = i;
                        if (selectedGroup == i)
                        {
                            ImGui.PopStyleColor(1);
                        }
                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.Selectable("New Group"))
                            {
                                addNewGroup = true;
                            }
                            ImGui.EndPopup();
                        }
                    }                    
                    if(addNewGroup)
                    {
                        in_Renderer.fcoFile.Groups.Add(new Group($"New_Group_{in_Renderer.fcoFile.Groups.Count}"));
                    }
                }
                else
                {
                    ImGui.Text("Open an FCO file to view its groups.");
                }
                ImGui.EndListBox();
            }
            
            ImGui.EndGroup();
        }
        void DrawFTECharacter(Sprite spr, Vector4 in_Color, float in_OffsetX)
        {
            //TEMPORARY
            //Since the bg is black, if the text is black, itll be unreadable
            if (in_Color.X == 0 && in_Color.Y == 0 && in_Color.Z == 0 && in_Color.W != 0)
            {
                in_Color.X = 1;
                in_Color.Y = 1;
                in_Color.Z = 1;
            }
            ConverseEditor.ShurikenRenderer.Vector2 uvTL = new ConverseEditor.ShurikenRenderer.Vector2(
                        spr.Start.X / spr.Texture.Width,
                        -(spr.Start.Y / spr.Texture.Height));


            ConverseEditor.ShurikenRenderer.Vector2 uvBR = uvTL + new ConverseEditor.ShurikenRenderer.Vector2(
            spr.Dimensions.X / spr.Texture.Width,
            -(spr.Dimensions.Y / spr.Texture.Height));

            unsafe
            {
                float width = (float)spr.Dimensions.X;
                const int bufferSize = 256;
                byte* buffer = stackalloc byte[bufferSize];
                StrBuilder sb = new(buffer, bufferSize);
                sb.Append($"##pattern{random.Next(0, 500)}");
                sb.End();
                //Draw sprite
                ImGui.SameLine(0, in_OffsetX);

                ImGui.Image(new ImTextureID(spr.Texture.GlTex.ID), new System.Numerics.Vector2(spr.Dimensions.X, spr.Dimensions.Y) * fontSizeMultiplier, uvTL, uvBR, in_Color);
                averageSize = spr.Dimensions.Y * fontSizeMultiplier;
            }
        }
        float GetOffsetFromAlignment(Cell in_Cell)
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
        void AlignForWidth(float width, float alignment = 0.5f)
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float off = (avail - width) * alignment;
            if (off > 0.0f)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);
                ImGui.PushStyleColor(ImGuiCol.Button, 0);
                var clicked = ImGui.Button("##invis", new System.Numerics.Vector2(1,0));
                ImGui.PopStyleColor();
            }
        }
        void CalculateAlignmentSpacing(Cell in_Cell, int[] in_ConverseIDs)
        {
            //Calculate the width and the amount of characters per line
            if (in_Cell.Alignment != Cell.TextAlign.Left)
            {
                int lineIndex = 0;
                lineWidth.Clear();
                foreach (var converseID in in_ConverseIDs)
                {
                    if (converseID == newLineValue)
                    {
                        lineIndex++;
                        continue;
                    }

                    Sprite spr = SpriteHelper.GetSpriteFromConverseID(converseID);
                    if (lineWidth.Count - 1 < lineIndex)
                        lineWidth.Add(new SLineInfo(0,0));
                    lineWidth[lineIndex].width += spr.Width * fontSizeMultiplier;
                    lineWidth[lineIndex].amount++;
                }
                //Set the first line to be aligned
                AlignForWidth(lineWidth[0].width, GetOffsetFromAlignment(in_Cell));
            }
            else
                lineWidth.Clear();
        }
        void DrawCellFromFTE(SUFcoTool.Cell in_Cell, int[] in_ConverseIDs)
        {
            CalculateAlignmentSpacing(in_Cell, in_ConverseIDs);
            int lineIdx = 0;
            for (int i = 0; i < in_ConverseIDs.Length; i++)
            {
                int converseID = in_ConverseIDs[i];
                if (converseID == newLineValue)
                {
                    lineIdx++;
                    if (lineWidth.Count > lineIdx && in_Cell.Alignment != Cell.TextAlign.Justified)
                    {
                        AlignForWidth(lineWidth[lineIdx].width, GetOffsetFromAlignment(in_Cell));
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
                    EmptyButton(converseID);
                    continue;
                }
                else
                {
                    if (spr.Texture.GlTex == null)
                    {
                        EmptyButton(converseID);
                        continue;
                    }
                    //Get the color to render the text with
                    CellColor currentHighlight = in_Cell.Highlights.FirstOrDefault(x => i >= x.Start && i <= x.End);
                    CellColor color = currentHighlight == null ? in_Cell.MainColor : currentHighlight;
                    //Calc spacing for justified text
                    float offset = 0;
                    if(in_Cell.Alignment == Cell.TextAlign.Justified)
                    {
                        offset = ((ImGui.GetContentRegionAvail().X - 50) - ((spr.Dimensions.X * fontSizeMultiplier) * lineWidth[lineIdx].amount)) / (lineWidth[lineIdx].amount - 1);
                    }
                    DrawFTECharacter(spr, color.ArgbColor, offset);
                }
            }
        }
        string GetMessageAsString(int[] in_IDs)
        {
            return string.Join(", ", in_IDs);
        }
        int[] GetStringAsMessage(string in_String)
        {
            return in_String.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToArray();
        }

        void CellInputText(int[] in_ConverseIDs, string in_CellName, ref SUFcoTool.Cell in_Cell, int in_Index, int in_LineCount)
        {
            ImGui.BeginDisabled(!tablePresent);
            string cellMessageConverted = 
                translationTableNew == null 
                ? GetMessageAsString(in_Cell.Message)
                : TranslationService.RawHEXtoTXT(in_ConverseIDs, translationTableNew);
            cellMessageConverted = cellMessageConverted.Replace("@@", "");
            if (ImGui.InputTextMultiline($"##{in_CellName}_{in_Index}text", ref cellMessageConverted, 512, new System.Numerics.Vector2(-1, ImGui.GetTextLineHeight() * in_LineCount)))
            {
                var joinedIDs2 = TranslationService.RawTXTtoHEX(cellMessageConverted, translationTableNew);
                in_Cell.Message = joinedIDs2;
            }
            
            ImGui.EndDisabled();
            if (!tablePresent)
            {
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("You cannot edit text unless you have a Translation Table open.");
                    ImGui.EndTooltip();
                }
            }
        }
        void DrawCellHeader(SUFcoTool.Group in_SelectedGroup, SUFcoTool.Cell in_Cell, int in_Index)
        {
            string cellName = string.IsNullOrEmpty(in_Cell.Name) ? $"Empty Cell ({in_Index})" : in_Cell.Name;
            if (expandAllCells) ImGui.SetNextItemOpen(expandAllCells);
            
            ImGui.PushID($"cell_{in_Index}");
            ImGui.BeginGroup();
            if (ImGui.CollapsingHeader(cellName))
            {
                ImGui.Indent();
                string name = in_Cell.Name;
                string[] alignmentOptions = { "Left", "Center", "Right", "Justified" };
                Vector4 colorMain = in_Cell.MainColor.ArgbColor;
                Vector4 colorSub1 = in_Cell.ExtraColor1.ArgbColor;
                Vector4 colorSub2 = in_Cell.ExtraColor2.ArgbColor;
                int alignmentIdx = (int)in_Cell.Alignment;
                int lineCount = 2;
                foreach (var line in in_Cell.Message)
                    if (line == newLineValue)
                        lineCount++;

                ImGui.InputText("Name", ref name, 256);
                CellInputText(in_Cell.Message, cellName, ref in_Cell, in_Index, lineCount);

                ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)));
                if (ImGui.BeginListBox($"##group{in_SelectedGroup.Name}_{in_Cell.Name}", new System.Numerics.Vector2(-1, lineCount * averageSize)))
                {
                    DrawCellFromFTE(in_Cell, in_Cell.Message);
                    ImGui.EndListBox();
                }
                ImGui.Combo("Alignment", ref alignmentIdx, alignmentOptions, 4);
                ImGui.ColorEdit4("Color", ref colorMain);
                ImGui.ColorEdit4("Color Sub 1", ref colorSub1);
                ImGui.ColorEdit4("Color Sub 2", ref colorSub2);
                ImGui.PushID($"##highlightlist{in_SelectedGroup.Name}_{in_Cell.Name}");
                if (ImGui.TreeNodeEx("Highlights"))
                {
                    for (int i = 0; i < in_Cell.Highlights.Count; i++)
                    {
                        ImGui.PushID($"##highlight_{i}_{in_SelectedGroup.Name}_{in_Cell.Name}");
                        if (ImGui.TreeNodeEx($"Highlight {i}"))
                        {
                            //ImGui in c# is ass.
                            CellColor highlight = in_Cell.Highlights[i];
                            Vector4 color = highlight.ArgbColor;
                            int startIdx = highlight.Start;
                            int endIdx = highlight.End;
                            ImGui.InputInt("Start", ref startIdx);
                            ImGui.InputInt("End", ref endIdx);
                            ImGui.ColorEdit4("Color", ref color);
                            highlight.Start = startIdx;
                            highlight.End = endIdx;
                            highlight.ArgbColor = color;
                            in_Cell.Highlights[i] = highlight;

                            ImGui.TreePop();

                        }
                        ImGui.PopID();
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.Unindent();
                in_Cell.Name = name;
                in_Cell.Alignment = (Cell.TextAlign)alignmentIdx;
                in_Cell.MainColor.ArgbColor = colorMain;
                in_Cell.ExtraColor1.ArgbColor = colorSub1;
                in_Cell.ExtraColor2.ArgbColor = colorSub2;
            }
            ImGui.EndGroup();            
            ImGui.PopID();
        }
        void DrawCellList(ConverseProject in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Cells");
            if (ImGui.BeginListBox("##listcells", new System.Numerics.Vector2(-1, ImGui.GetContentRegionAvail().Y - 32)))
            {
                if (in_FcoFilePresent)
                {
                    SUFcoTool.Group selectedGroup = in_Renderer.fcoFile.Groups[selectedGroupIndex];
                    if(selectedGroup.CellList.Count != 0)
                    {
                        for (int x = 0; x < selectedGroup.CellList.Count; x++)
                        {
                            DrawCellHeader(selectedGroup, selectedGroup.CellList[x], x);
                        }
                    }
                }
            }
            ImGui.EndListBox();
            float sizeX = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;
            ImGui.SetNextItemWidth(sizeX);
            ImGui.Button("Add Cell", new System.Numerics.Vector2(sizeX, 25));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(sizeX);
            ImGui.Button("Remove Cell", new System.Numerics.Vector2(sizeX, 25));
            ImGui.EndGroup();
        }
        void AddMissingFteEntriesToTable(List<TranslationTable.Entry> in_Entries)
        {
            for (int i = 0; i < in_Entries.Count; i++)
            {
                //Replace legacy newline with new style
                if (in_Entries[i].Letter == "{NewLine}")
                {
                    var entry2 = in_Entries[i];
                    entry2.Letter = "\n";
                    in_Entries[i] = entry2;
                }
            }
            foreach (var spr in SpriteHelper.CharSprites)
            {
                if (in_Entries.FindAll(x => x.ConverseID == spr.Key.CharacterID).Count == 0)
                {
                    in_Entries.Add(new TranslationTable.Entry("", spr.Key.CharacterID));
                }
            }
        }
        public override void Render(ConverseProject in_Renderer)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(in_Renderer.screenSize.X, in_Renderer.screenSize.Y - MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("##FCOViewerWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Checkbox("Expand all", ref expandAllCells);
                ImGui.SameLine();
                ImGui.SliderFloat("Font Size", ref fontSizeMultiplier, 0.5f, 2);
                ImGui.Separator();

                if (ImGui.BeginTabBar("##tabsfco"))
                {                    
                    bool isFcoLoaded = in_Renderer.fcoFile != null;
                    if (ImGui.BeginTabItem("FCO Viewer"))
                    {                        
                        DrawGroupSelection(in_Renderer, isFcoLoaded);
                        ImGui.SameLine();
                        DrawCellList(in_Renderer, isFcoLoaded);
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Table Generator"))
                    {
                        if (isFcoLoaded)
                        {
                            ImGui.TextWrapped("A translation table is necessary to be able to edit text from FCOs, as they do not store the character used to type out the sentences.");

                            if (ImGui.Button("Import Table"))
                            {
                                var testdial = NativeFileDialogSharp.Dialog.FileOpen("json");
                                if (testdial.IsOk)
                                {
                                    translationTableNew = TranslationTable.Read(@testdial.Path).Tables["Standard"];
                                    tablePresent = true;
                                    AddMissingFteEntriesToTable(translationTableNew);
                                }
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Create Table"))
                            {
                                translationTableNew = new List<TranslationTable.Entry>();
                                translationTableNew.Add(new TranslationTable.Entry("\\n", 0));
                                AddMissingFteEntriesToTable(translationTableNew);
                                tablePresent = true;
                            }
                            ImGui.SameLine();
                            if(ImGui.Button("Save Table"))
                            {
                                var testdial = NativeFileDialogSharp.Dialog.FileSave("json");
                                if (testdial.IsOk)
                                {
                                    TranslationTable table = new TranslationTable();
                                    table.Standard = translationTableNew;
                                    table.Write(@testdial.Path);
                                }
                            }

                            if (tablePresent)
                            {
                                if (ImGui.BeginTable("table2", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.ScrollY, new System.Numerics.Vector2(-1, -1)))
                                {
                                    ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.None, 0);
                                    ImGui.TableSetupColumn("Sprite", ImGuiTableColumnFlags.None, 0);
                                    ImGui.TableHeadersRow();
                                    ImGui.TableNextRow(0, 0);
                                    ImGui.TableSetColumnIndex(0);
                                    /// @separator

                                    for (int i = 0; i < translationTableNew.Count; i++)
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
                                            DrawFTECharacter(spr, new Vector4(1, 1, 1, 1), 0);
                                        else
                                        {
                                            ImGui.Text("[Missing Texture]");
                                        }
                                        ImGui.TableNextRow();
                                        translationTableNew[i] = letter;
                                    }
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
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }
    }


}
