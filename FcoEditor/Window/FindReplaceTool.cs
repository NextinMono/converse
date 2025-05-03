using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using HekonrayBase;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ConverseEditor
{
    internal class FindReplaceTool
    {
        public static bool Enabled = false;
        public static int occurencesCount;
        public static bool replaceMode;
        public static string findString = "";
        public static string replaceString = "";
        public static void SetActive(bool in_Status, bool in_ReplaceMode)
        {
            Enabled = in_Status;
            replaceMode = in_ReplaceMode;
        }
        public static void Render(ConverseProject in_Renderer)
        {
            ImGui.OpenPopup("Find and Replace");
            Vector2 size = new Vector2(500, replaceMode ? 400 : 255);
            ImConverse.CenterWindow(size);
            if (ImGui.BeginPopupModal("Find and Replace", ref Enabled, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
            {
                //ImGui.BeginDisabled(!in_Renderer.IsTableLoaded());
                if (ImGui.InputTextMultiline("Find...", ref findString, 2048))
                    occurencesCount = 0;
                if (replaceMode)
                {
                    ImGui.InputTextMultiline("Replace with...", ref replaceString, 2048);
                }
                else
                {
                    if (occurencesCount > 0)
                    {
                        ImGui.Text($"Found \"{findString}\" in {occurencesCount} cells.");
                    }
                }
                ImGui.Separator();
                if(replaceMode)
                {
                    if (ImGui.Button("Replace"))
                    {
                        ReplaceText(in_Renderer);
                        ImGui.CloseCurrentPopup();
                        Enabled = false;
                    }
                }
                else
                {
                    if (ImGui.Button("Find"))
                    {
                        occurencesCount = FindText(in_Renderer);
                    }
                }

                if (!in_Renderer.IsTableLoaded())
                {
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("You cannot enter text directly without a Translation Table.");
                        ImGui.EndTooltip();
                    }
                }
                //ImGui.EndDisabled();
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                    Enabled = false;
                }
                ImGui.EndPopup();
            }
            
        }
        static int FindSequenceIndex(int[] list, int[] sequence)
        {
            for (int i = 0; i <= list.Length - sequence.Length; i++)
            {
                if (list.Skip(i).Take(sequence.Length).SequenceEqual(sequence))
                {
                    return i;
                }
            }
            return -1;
        }
        private static void ReplaceText(ConverseProject in_Renderer)
        {
            var hexFind = TranslationService.RawTXTtoHEX(findString, in_Renderer.config.translationTable);
            var hexReplace = TranslationService.RawTXTtoHEX(replaceString, in_Renderer.config.translationTable);
            foreach(var file in in_Renderer.GetFcoFiles())
            {
                foreach (var group in file.file.Groups)
                {
                    foreach (var cell in group.Cells)
                    {
                        int index = FindSequenceIndex(cell.Message, hexFind);
                        if (index != -1)
                        {
                            var list = cell.Message.ToList();
                            list.RemoveRange(index, hexFind.Length);
                            list.InsertRange(index, hexReplace);
                            cell.Message = list.ToArray();
                        }
                    }
                }
            }
        }
        private static int FindText(ConverseProject in_Renderer)
        {
            int result = 0;
            var hexFind = TranslationService.RawTXTtoHEX(findString, in_Renderer.config.translationTable);
            foreach (var file in in_Renderer.GetFcoFiles())
            {
                foreach (var group in file.file.Groups)
                {
                    foreach (var cell in group.Cells)
                    {
                        int index = FindSequenceIndex(cell.Message, hexFind);
                        if (index != -1)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }
    }
}