using Hexa.NET.ImGui;

namespace Kunai
{
    public class TempSearchBox
    {
        public string searchTxt = "";
        public string m_ComparisonString = "";
        public string m_SearchTxtCopy = "";

        //Call 1st
        public void Render()
        {
            ImGui.TextUnformatted("Search  ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##Search", ref searchTxt, 256);
            ImGui.Separator();
        }

        //Call 2nd
        public void Update(string str)
        {
            m_ComparisonString = str.ToLower();
            m_SearchTxtCopy = searchTxt.ToLower();
        }

        //Call where result is needed
        public bool MatchResult()
        {
            return string.IsNullOrEmpty(m_SearchTxtCopy) ? true : m_ComparisonString.Contains(m_SearchTxtCopy);
        }

    }
}