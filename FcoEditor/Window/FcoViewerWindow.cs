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
using StbTrueTypeSharp;
using System.IO;
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
        int selectedGroupIndex;
        float averageSize = 50;
        float fontSizeMultiplier = 1;
        bool expandAllCells = false;
        public bool tablePresent = false;
        Random random = new Random();
        public List<TranslationTable.Entry> translationTableNew = null;
        List<SLineInfo> lineWidth = new List<SLineInfo>();
        

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
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.6f, 0, 1));
                        
                        if (ImGui.Selectable(string.IsNullOrEmpty(in_Renderer.fcoFile.Groups[i].Name) ? $"Empty{i}" : in_Renderer.fcoFile.Groups[i].Name))
                            selectedGroupIndex = i;

                        if (selectedGroup == i)                        
                            ImGui.PopStyleColor(1);
                        
                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.Selectable("New Group"))
                            {
                                addNewGroup = true;
                            }
                            ImGui.EndPopup();
                        }
                    }
                    if (addNewGroup)
                    {
                        in_Renderer.fcoFile.Groups.Add(new Group($"New_Group_{in_Renderer.fcoFile.Groups.Count}"));
                    }
                }
                else
                {
                    ImGui.Text("Open an FCO file to view its contents.");
                }
                ImGui.EndListBox();
            }
            ImGui.EndGroup();
        }

        
        void DrawCellHeader(SUFcoTool.Group in_SelectedGroup, SUFcoTool.Cell in_Cell, int in_Index)
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
                    in_SelectedGroup.CellList.Add(new Cell());
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
                ImConverse.InputTextCell(in_Cell.Message, cellName, ref in_Cell, translationTableNew, in_Index, lineCount);

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
                    if(ImGui.CollapsingHeader("Highlights"))
                    {
                        if(ImGui.Button("Add"))
                        {
                            in_Cell.Highlights.Add(new CellColor());
                        }
                        for (int i = 0; i < in_Cell.Highlights.Count; i++)
                        {
                            ImGui.PushID($"##highlight_{i}_{in_SelectedGroup.Name}_{in_Cell.Name}");
                            bool hlOpen = ImGui.TreeNodeEx($"Highlight {i}");
                            bool delete = false;
                            if (ImGui.BeginPopupContextItem())
                            {
                                if(ImGui.MenuItem("Delete"))
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
                            if(delete)
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
            if(clHeadDelete)
                in_SelectedGroup.CellList.Remove(in_Cell);
        }
        void DrawCellList(ConverseProject in_Renderer, bool in_FcoFilePresent)
        {
            ImGui.BeginGroup();
            ImGui.Text("Cells");
            float spacingButtons = 0; //32
            if (ImGui.BeginListBox("##listcells", new System.Numerics.Vector2(-1, ImGui.GetContentRegionAvail().Y - spacingButtons)))
            {
                if (in_FcoFilePresent)
                {
                    SUFcoTool.Group selectedGroup = in_Renderer.fcoFile.Groups[selectedGroupIndex];
                    if (selectedGroup.CellList.Count != 0)
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
            //ImGui.SetNextItemWidth(sizeX);
            //ImGui.Button("Add Cell", new System.Numerics.Vector2(sizeX, 25));
            //ImGui.SameLine();
            //ImGui.SetNextItemWidth(sizeX);
            //ImGui.Button("Remove Cell", new System.Numerics.Vector2(sizeX, 25));
            ImGui.EndGroup();
        }
        void AddMissingFteEntriesToTable(List<TranslationTable.Entry> in_Entries, bool isUnleashed)
        {
            if (isUnleashed)
            {
                //Add default icons
                List<string> keys = new List<string>
                {
                    "{A}", "{B}", "{X}", "{Y}", "{LB}", "{RB}", "{LT}", "{RT}",
                    "{LSUP}", "{LSRIGHT}", "{LSDOWN}", "{LSLEFT}", "{RSUP}", "{RSRIGHT}",
                    "{RSDOWN}", "{RSLEFT}", "{DPADUP}", "{DPADRIGHT}", "{DPADDOWN}",
                    "{DPADLEFT}", "{START}", "{SELECT}", "{SAVE}"
                };
                //Add first set of keys unaltered, then do it again but with a _2 suffix
                int index = 100;
                foreach (string key in keys)
                {
                    in_Entries.Add(new TranslationTable.Entry(key, index));
                    index++;
                }
                foreach (string key in keys)
                {
                    in_Entries.Add(new TranslationTable.Entry(key.Insert(key.Length - 1, "_2"), index));
                    index++;
                }

            }
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
                if(FindReplaceTool.Enabled)
                {
                    FindReplaceTool.Render(in_Renderer);
                }
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
                    if (ImGui.BeginTabItem("Translation Table"))
                    {
                        //TODO: split into new class
                        if (isFcoLoaded)
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
                                    LoadTranslationTable(@testdial.Path);
                                }
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Create Table", new System.Numerics.Vector2(size, 32)))
                            {
                                translationTableNew = new List<TranslationTable.Entry>();
                                translationTableNew.Add(new TranslationTable.Entry("\\n", 0));
                                AddMissingFteEntriesToTable(translationTableNew, in_Renderer.fcoFile.Header.Version == 0);
                                tablePresent = true;
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Save Table", new System.Numerics.Vector2(size, 32)))
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
                                ImGui.SeparatorText("Table");
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
                                        ImGui.TableSetColumnIndex(1);
                                        if (spr.Texture.GlTex != null)
                                            averageSize = ImConverse.DrawConverseCharacter(spr, new Vector4(1, 1, 1, 1), 0, fontSizeMultiplier);
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
                    if (ImGui.BeginTabItem("FTE Generator"))
                    {
                        FteTextureGenerator.Draw(in_Renderer);
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        public void LoadTranslationTable(string @in_Path)
        {
            translationTableNew = TranslationTable.Read(@in_Path).Tables["Standard"];
            tablePresent = true;
            AddMissingFteEntriesToTable(translationTableNew, true);
        }
        public List<TranslationTable.Entry> GetTranslationTableEntries() => translationTableNew;



    }
}