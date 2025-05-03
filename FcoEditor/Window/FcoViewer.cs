using ConverseEditor.Rendering;
using Hexa.NET.ImGui;
using libfco;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ConverseEditor
{
    public static class FcoViewer
    {
        static int selectedFileIndex;
        static int selectedGroupIndex;
        static bool renamingGroup;
        static bool expandAllCells = false;
        static float averageSize = 50;
        static float fontSizeMultiplier = 1;
        static TempSearchBox searchBox = new TempSearchBox();
        static List<SLineInfo> lineWidth = new List<SLineInfo>();

        static void DrawGroupSelection(ConverseProject in_Renderer, bool in_FcoFilePresent)
        {
            var size = new System.Numerics.Vector2(ImGui.GetWindowSize().X / 3, -1);
            ImGui.BeginGroup();
            //ImGui.Text("Groups");
            searchBox.Render(size);
            var fcoFiles = in_Renderer.GetFcoFiles();
            if (ImConverse.BeginListBoxCustom("##groupslist", size))
            {
                if (in_FcoFilePresent)
                {

                    for (int a = 0; a < fcoFiles.Count; a++)
                    {

                        bool isSelected = false;
                        if (ImConverse.VisibilityNode(fcoFiles[a].GetFileName(), ref isSelected, null, fcoFiles[a].file.Groups.Count > 0, NodeIconResource.File))
                        {

                            for (int i = 0; i < fcoFiles[a].file.Groups.Count; i++)
                            {
                                if (searchBox.IsSearching)
                                {
                                    searchBox.Update(fcoFiles[a].file.Groups[i].Name);
                                    if (!searchBox.MatchResult())
                                        continue;
                                }
                                    if (selectedGroupIndex == i && selectedFileIndex == a)
                                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.6f, 0, 1));

                                    string groupName = string.IsNullOrEmpty(fcoFiles[a].file.Groups[i].Name) ? $"Empty{i}" : fcoFiles[a].file.Groups[i].Name;
                                    bool isSelectedG = selectedGroupIndex == i && selectedFileIndex == a;

                                    if (ImConverse.VisibilityNode(groupName, ref isSelectedG, delegate { RightClickGroup(in_Renderer, i); }, false, NodeIconResource.Group))
                                        ImGui.TreePop();

                                    if (selectedGroupIndex == i && selectedFileIndex == a)
                                        ImGui.PopStyleColor(1);

                                    if (isSelectedG)
                                    {
                                        selectedGroupIndex = i;
                                        selectedFileIndex = a;
                                    }

                                
                            }
                            ImGui.TreePop();
                        }
                        if (isSelected)
                        {
                            selectedFileIndex = a;
                            selectedGroupIndex = 0;
                        }
                    }
                }
                else
                {
                    ImGui.Text("Open an FCO file to view its contents.");
                }
            }
            ImConverse.EndListBoxCustom();
            ImGui.EndGroup();
            if (renamingGroup)
            {
                ImGui.OpenPopup("Rename");
            }

            System.Numerics.Vector2 modalSize = new System.Numerics.Vector2(350, 100);

            // Calculate centered position
            ImConverse.CenterWindow(modalSize);
            if (ImGui.BeginPopupModal("Rename", ref renamingGroup, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                string newName = fcoFiles[selectedFileIndex].file.Groups[selectedGroupIndex].Name;
                ImGui.InputText("New Name", ref newName, 256);
                fcoFiles[selectedFileIndex].file.Groups[selectedGroupIndex].Name = newName;
                ImGui.Separator();
                if (ImGui.Button("OK") || ImGui.IsKeyPressed(ImGuiKey.Enter))
                {
                    renamingGroup = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        private static void RightClickGroup(ConverseProject in_Renderer, int in_CurrentGroup)
        {
            if (ImGui.Selectable("New Group"))
            {
                in_Renderer.AddNewGroup(selectedFileIndex);
            }
            if (ImGui.Selectable("Rename"))
            {
                renamingGroup = true;
                selectedGroupIndex = in_CurrentGroup;
            }
        }

        static void DrawCells(ConverseProject in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Cells");
            float spacingButtons = 0; //32
            if (ImGui.BeginListBox("##listcells", new System.Numerics.Vector2(-1, ImGui.GetContentRegionAvail().Y - spacingButtons)))
            {
                if (in_FcoFilePresent)
                {
                    libfco.Group selectedGroup = in_Renderer.GetFcoFiles()[selectedFileIndex].file.Groups[selectedGroupIndex];
                    if (selectedGroup.Cells.Count != 0)
                    {
                        for (int x = 0; x < selectedGroup.Cells.Count; x++)
                        {
                            DrawCellHeader(selectedGroup, selectedGroup.Cells[x], x, in_Renderer);
                        }
                    }
                }
                ImGui.EndListBox();
            }
            float sizeX = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;
            //ImGui.SetNextItemWidth(sizeX);
            //ImGui.Button("Add Cell", new System.Numerics.Vector2(sizeX, 25));
            //ImGui.SameLine();
            //ImGui.SetNextItemWidth(sizeX);
            //ImGui.Button("Remove Cell", new System.Numerics.Vector2(sizeX, 25));
            ImGui.EndGroup();
        }
        static void DrawCellHeader(libfco.Group in_SelectedGroup, libfco.Cell in_Cell, int in_Index, ConverseProject in_Renderer)
        {
            string cellName = string.IsNullOrEmpty(in_Cell.Name) ? $"Empty Cell ({in_Index})" : in_Cell.Name;
            if (expandAllCells) ImGui.SetNextItemOpen(expandAllCells);

            ImGui.PushID($"cell_{in_Index}");
            ImGui.BeginGroup();
            bool clHeadOpen = ImGui.TreeNodeEx($"{cellName}###cellheader{in_Index}", ImGuiTreeNodeFlags.CollapsingHeader);
            bool clHeadDelete = false;
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Add"))
                {
                    in_SelectedGroup.Cells.Add(new Cell());
                }
                if (ImGui.MenuItem("Delete"))
                {
                    clHeadDelete = true;
                }
                ImGui.EndPopup();
            }
            if (clHeadOpen)
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
                    if (line == ImConverse.newLineValue)
                        lineCount++;

                ImGui.InputText("Name", ref name, 256);
                ImConverse.InputTextCell(in_Cell.Message, cellName, ref in_Cell, in_Renderer.config.translationTable, in_Index, lineCount);

                ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)));
                if (ImGui.BeginListBox($"##group{in_SelectedGroup.Name}_{in_Cell.Name}", new System.Numerics.Vector2(-1, lineCount * averageSize)))
                {
                    ImConverse.DrawCellFromFTE(in_Cell, in_Cell.Message, fontSizeMultiplier, ref lineWidth);
                    ImGui.EndListBox();
                }
                ImGui.PopStyleColor();
                ImGui.Combo("Alignment", ref alignmentIdx, alignmentOptions, 4);
                ImGui.ColorEdit4("Color", ref colorMain);
                ImGui.PushID($"##highlightlist{in_SelectedGroup.Name}_{in_Cell.Name}");
                if (ImGui.TreeNodeEx("Extra"))
                {
                    ImGui.ColorEdit4("Color Sub 1", ref colorSub1);
                    ImGui.ColorEdit4("Color Sub 2", ref colorSub2);
                    if (ImGui.CollapsingHeader("Highlights"))
                    {
                        if (ImGui.Button("Add"))
                        {
                            in_Cell.Highlights.Add(new CellColor(2));
                        }
                        for (int i = 0; i < in_Cell.Highlights.Count; i++)
                        {
                            ImGui.PushID($"##highlight_{i}_{in_SelectedGroup.Name}_{in_Cell.Name}");
                            bool hlOpen = ImGui.TreeNodeEx($"Highlight {i}");
                            bool delete = false;
                            if (ImGui.BeginPopupContextItem())
                            {
                                if (ImGui.MenuItem("Delete"))
                                {
                                    delete = true;
                                }
                                ImGui.EndPopup();
                            }
                            if (hlOpen)
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
                            if (delete)
                                in_Cell.Highlights.RemoveAt(i);
                        }
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
                ImGui.Unindent();
                in_Cell.Name = name;
                in_Cell.Alignment = (Cell.TextAlign)alignmentIdx;
                in_Cell.MainColor.ArgbColor = colorMain;
                in_Cell.ExtraColor1.ArgbColor = colorSub1;
                in_Cell.ExtraColor2.ArgbColor = colorSub2;
            }
            ImGui.PopID();
            ImGui.EndGroup();
            if (clHeadDelete)
                in_SelectedGroup.Cells.Remove(in_Cell);
        }
        public static void Reset()
        {
            selectedGroupIndex = 0;
            selectedFileIndex = 0;
        }
        public static void Render(ConverseProject in_Renderer)
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);
            ImGui.SliderFloat("Font Size", ref fontSizeMultiplier, 0.5f, 2);
            ImGui.SameLine();
            ImGui.Checkbox("Expand all", ref expandAllCells);
            ImGui.Separator();
            bool isFcoLoaded = in_Renderer.GetFcoFiles().Count > 0;
            DrawGroupSelection(in_Renderer, isFcoLoaded);
            ImGui.SameLine();
            DrawCells(in_Renderer, isFcoLoaded);
            ImGui.EndTabItem();
        }
    }
}