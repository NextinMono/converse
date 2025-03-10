using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
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
            ImGui.BeginDisabled(!FcoViewerWindow.Instance.tablePresent);
            ImGui.InputTextMultiline("Find", ref findString, 1024);
            ImGui.InputTextMultiline("Replace", ref replaceString, 1024);
            if (ImGui.Button("Replace2"))
                ReplaceText(in_Renderer);
            ImGui.Separator();
            ImGui.EndDisabled();
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
            var hexFind = TranslationService.RawTXTtoHEX(findString, FcoViewerWindow.Instance.translationTableNew);
            var hexReplace = TranslationService.RawTXTtoHEX(replaceString, FcoViewerWindow.Instance.translationTableNew);
            foreach (var group in in_Renderer.fcoFile.Groups)
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