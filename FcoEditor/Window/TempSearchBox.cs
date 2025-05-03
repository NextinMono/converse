using Hexa.NET.ImGui;
using System.Numerics;

namespace ConverseEditor
{
    public class TempSearchBox
    {
        public string SearchTxt = "";
        public string MComparisonString = "";
        public string MSearchTxtCopy = "";
        public bool IsSearching { get { return SearchTxt != ""; } }

        //Call 1st
        public void Render(Vector2 in_Size)
        {
            ImGui.TextUnformatted("Search  ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(in_Size.X - ImGui.CalcTextSize("Search   ").X - 3);
            ImGui.InputText("##Search", ref SearchTxt, 256);
        }

        //Call 2nd
        public void Update(string in_Str)
        {
            MComparisonString = in_Str.ToLower();
            MSearchTxtCopy = SearchTxt.ToLower();
        }

        //Call where result is needed
        public bool MatchResult()
        {
            return string.IsNullOrEmpty(MSearchTxtCopy) ? true : MComparisonString.Contains(MSearchTxtCopy);
        }

    }
}