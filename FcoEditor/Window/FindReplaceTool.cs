using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using HekonrayBase;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConverseEditor
{
    internal class FindReplaceTool
    {
        public static bool Enabled = false;
        public static string findString = "";
        public static string replaceString = "";
        public static void Render(ConverseProject in_Renderer)
        {
            ImGui.OpenPopup("Find and Replace");
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 400));
            if (ImGui.BeginPopupModal("Find and Replace", ref Enabled, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
            {
                ImGui.BeginDisabled(!in_Renderer.IsTableLoaded());
                ImGui.InputTextMultiline("Find", ref findString, 1024);
                ImGui.InputTextMultiline("Replace", ref replaceString, 1024);
                ImGui.Separator();
                if (ImGui.Button("Execute"))
                    ReplaceText(in_Renderer);

                if (!in_Renderer.IsTableLoaded())
                {
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("You cannot edit text unless you have a Translation Table open.");
                        ImGui.EndTooltip();
                    }
                }
                ImGui.EndDisabled();
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
            foreach (var group in in_Renderer.config.fcoFile.Groups)
            {
                foreach(var cell in group.CellList)
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
}