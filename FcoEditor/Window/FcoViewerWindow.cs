﻿using Hexa.NET.ImGui;
using Hexa.NET.Utilities.Text;
using FcoEditor.ShurikenRenderer;
using Shuriken.Rendering;
using SUFontTool.FCO;
using SUFcoTool;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
namespace FcoEditor
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
        readonly string newLineValue = "00 00 00 00";
        int selectedGroupIndex;
        float averageSize = 50;
        float fontSizeMultiplier = 1;
        bool expandAllCells = false;
        bool tablePresent = false;
        Random random = new Random();
        List<TranslationTable.Entry> translationTableNew = null;

        static void EmptyButton(string in_ConvID)
        {
            ImGui.SameLine(0);
            ImGui.SetNextItemWidth(50);
            ImGui.Button(string.IsNullOrEmpty(in_ConvID) ? "null" : in_ConvID);
        }
        public override void OnReset(ShurikenRenderHelper in_Renderer)
        {
            selectedGroupIndex = 0;
            tablePresent = false;
            translationTableNew = null;
        }
        void DrawGroupSelection(ShurikenRenderHelper in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Groups");
            if (ImGui.BeginListBox("##groupslist", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 3, -1)))
            {
                if (in_FcoFilePresent)
                {
                    for (int i = 0; i < in_Renderer.fcoFile.Groups.Count; i++)
                    {
                        if (ImGui.Selectable(string.IsNullOrEmpty(in_Renderer.fcoFile.Groups[i].Name) ? $"Empty{i}" : in_Renderer.fcoFile.Groups[i].Name))
                            selectedGroupIndex = i;
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
        void DrawFTECharacter(Sprite spr, Vector4 in_Color)
        {
            //TEMPORARY
            //Since the bg is black, if the text is black, itll be unreadable
            if (in_Color.X == 0 && in_Color.Y == 0 && in_Color.Z == 0 && in_Color.W != 0)
            {
                in_Color.X = 1;
                in_Color.Y = 1;
                in_Color.Z = 1;
            }
            FcoEditor.ShurikenRenderer.Vector2 uvTL = new FcoEditor.ShurikenRenderer.Vector2(
                        spr.Start.X / spr.Texture.Width,
                        -(spr.Start.Y / spr.Texture.Height));


            FcoEditor.ShurikenRenderer.Vector2 uvBR = uvTL + new FcoEditor.ShurikenRenderer.Vector2(
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
                ImGui.SameLine(0, 0);
                ImGui.Image(new ImTextureID(spr.Texture.GlTex.ID), new System.Numerics.Vector2(spr.Dimensions.X, spr.Dimensions.Y) * fontSizeMultiplier, uvTL, uvBR, in_Color);
                averageSize = spr.Dimensions.Y * fontSizeMultiplier;
            }
        }
        void DrawCellFromFTE(SUFcoTool.Cell in_Cell, string[] in_ConverseIDs)
        {
            for (int i = 0; i < in_ConverseIDs.Length; i++)
            {
                string converseID = in_ConverseIDs[i];
                if (converseID == newLineValue)
                {
                    ImGui.NewLine();
                    continue;
                }
                if (converseID == "")
                    continue;

                Sprite spr = SpriteHelper.GetSpriteFromConverseID(converseID);
                //In the case that a texture cant be found or if its unregistered through
                //SpriteHelper, print the converse id and skip
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
                    var currentHighlight = in_Cell.Highlights.FirstOrDefault(x => i >= x.Start && i <= x.End);
                    var color = currentHighlight == null ? in_Cell.MainColor : currentHighlight;
                    
                    DrawFTECharacter(spr, color.ArgbColor);
                }

            }
        }
        void CellInputText(string[] in_ConverseIDs, string in_CellName, ref SUFcoTool.Cell in_Cell, int in_Index, int in_LineCount)
        {
            ImGui.BeginDisabled(!tablePresent);
            string joinedIDs = string.Join("", in_ConverseIDs);
            string cellMessageConverted = translationTableNew == null ? in_Cell.MessageConverseIDs : TranslationService.RawHEXtoTXT(joinedIDs, translationTableNew);
            if(ImGui.InputTextMultiline($"##{in_CellName}_{in_Index}text", ref cellMessageConverted, 512, new System.Numerics.Vector2(-1, ImGui.GetTextLineHeight() * in_LineCount)))
            {
                joinedIDs = TranslationService.RawTXTtoHEX(cellMessageConverted, translationTableNew);
                in_Cell.MessageConverseIDs = joinedIDs;
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
            ImGui.PushID($"cell_{in_Index}");
            string cellName = string.IsNullOrEmpty(in_Cell.Name) ? $"Empty Cell ({in_Index})" : in_Cell.Name;
            if (expandAllCells) ImGui.SetNextItemOpen(expandAllCells);
            if (ImGui.CollapsingHeader(cellName))
            {
                ImGui.Indent();

                string[] alignmentOptions = { "Left", "Center", "Right", "Justified" };
                Vector4 colorMain = in_Cell.MainColor.ArgbColor;
                Vector4 colorSub1 = in_Cell.ExtraColor1.ArgbColor;
                Vector4 colorSub2 = in_Cell.ExtraColor2.ArgbColor;
                int alignmentIdx = (int)in_Cell.Alignment;
                string[] converseIDs = in_Cell.MessageConverseIDs.Split(", ");
                int lineCount = 2;
                foreach (var line in converseIDs)
                    if (line == newLineValue)
                        lineCount++;

                CellInputText(converseIDs, cellName, ref in_Cell, in_Index, lineCount);

                ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)));
                if (ImGui.BeginListBox($"##group{in_SelectedGroup.Name}_{in_Cell.Name}", new System.Numerics.Vector2(-1, lineCount * averageSize)))
                {
                    DrawCellFromFTE(in_Cell, converseIDs);
                    ImGui.EndListBox();
                }
                //    Left = 0,
                //Center = 1,
                //Right = 2,
                //Justified = 3
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
                in_Cell.Alignment = (Cell.TextAlign)alignmentIdx;
                in_Cell.MainColor.ArgbColor = colorMain;
                in_Cell.ExtraColor1.ArgbColor = colorSub1;
                in_Cell.ExtraColor2.ArgbColor = colorSub2;
            }
            ImGui.PopID();
        }
        void DrawCellList(ShurikenRenderHelper in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Cells");
            if (ImGui.BeginListBox("##listcells", new System.Numerics.Vector2(-1, -1)))
            {
                if (in_FcoFilePresent)
                {
                    SUFcoTool.Group selectedGroup = in_Renderer.fcoFile.Groups[selectedGroupIndex];
                    for (int x = 0; x < selectedGroup.CellList.Count; x++)
                    {
                        DrawCellHeader(selectedGroup, selectedGroup.CellList[x], x);
                    }
                }
            }
            ImGui.EndListBox();
            ImGui.EndGroup();
        }
        void AddMissingFteEntriesToTable(List<TranslationTable.Entry> in_Entries)
        {
            foreach (var spr in SpriteHelper.CharSprites)
            {
                if (in_Entries.FindAll(x => x.HexString == spr.Key.FcoCharacterID).Count == 0)
                {
                    in_Entries.Add(new TranslationTable.Entry("", spr.Key.FcoCharacterID));
                }
            }
        }
        public override void Render(ShurikenRenderHelper in_Renderer)
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
                                translationTableNew.Add(new TranslationTable.Entry("{NewLine}", "00 00 00 00"));
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
                                        Sprite spr = SpriteHelper.GetSpriteFromConverseID(translationTableNew[i].HexString);
                                        if (spr == null)
                                            continue;
                                        var letter = translationTableNew[i];

                                        ImGui.TableSetColumnIndex(0);
                                        ImGui.SetNextItemWidth(-1);
                                        ImGui.InputText($"##input{letter.HexString}", ref letter.Letter, 256);
                                        ImGui.TableSetColumnIndex(1);
                                        if (spr.Texture.GlTex != null)
                                            DrawFTECharacter(spr, new Vector4(1, 1, 1, 1));
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
