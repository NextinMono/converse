using Hexa.NET.ImGui;
using System.Numerics;

namespace ConverseEditor.Settings
{
    public static class ImGuiThemeManager
    {
        private static void SetDarkTheme()
        {
            // AdobeInspired stylenexacopic from ImThemes
            var style = ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.None;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 4.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 4.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 20.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.0784313753247261f, 0.0784313753247261f, 0.0784313753247261f, 0.9399999976158142f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(1.0f, 1.0f, 1.0f, 0.1630901098251343f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.08627451211214066f, 0.08627451211214066f, 0.08627451211214066f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1529411822557449f, 0.1529411822557449f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.105882354080677f, 0.105882354080677f, 0.105882354080677f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0f, 0.0f, 0.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.01960784383118153f, 0.01960784383118153f, 0.01960784383118153f, 0.5299999713897705f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.407843142747879f, 0.407843142747879f, 0.407843142747879f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.5098039507865906f, 0.5098039507865906f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.8784313797950745f, 0.8784313797950745f, 0.8784313797950745f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.9803921580314636f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.1490196138620377f, 0.1490196138620377f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.2470588237047195f, 0.2470588237047195f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.3294117748737335f, 0.3294117748737335f, 0.3294117748737335f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.3098039329051971f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.4274509847164154f, 0.4274509847164154f, 0.4980392158031464f, 0.5f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.7490196228027344f, 0.7490196228027344f, 0.7490196228027344f, 0.7803921699523926f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.7490196228027344f, 0.7490196228027344f, 0.7490196228027344f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.9372549057006836f, 0.9372549057006836f, 0.9372549057006836f, 0.6705882549285889f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.9490196108818054f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.2235294133424759f, 0.2235294133424759f, 0.2235294133424759f, 0.8627451062202454f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.321568638086319f, 0.321568638086319f, 0.321568638086319f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.TabSelected] = new Vector4(0.2745098173618317f, 0.2745098173618317f, 0.2745098173618317f, 1.0f);
            style.Colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 0.9725490212440491f);
            style.Colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.4235294163227081f, 0.4235294163227081f, 0.4235294163227081f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.6078431606292725f, 0.6078431606292725f, 0.6078431606292725f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.4274509847164154f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.8980392217636108f, 0.6980392336845398f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.6000000238418579f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.3499999940395355f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 1.0f, 0.0f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.NavCursor] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }
        private static void SetLightTheme()
        {
            // AdobeInspired stylenexacopic from ImThemes
            var style = ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.None;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 4.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 4.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 20.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.6000000238418579f, 0.6000000238418579f, 0.6000000238418579f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.9372549057006836f, 0.9372549057006836f, 0.9372549057006836f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(1.0f, 1.0f, 1.0f, 0.9800000190734863f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.0f, 0.0f, 0.0f, 0.300000011920929f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.4000000059604645f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.95686274766922f, 0.95686274766922f, 0.95686274766922f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.8196078538894653f, 0.8196078538894653f, 0.8196078538894653f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.0f, 1.0f, 1.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.8588235378265381f, 0.8588235378265381f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.5299999713897705f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.686274528503418f, 0.686274528503418f, 0.686274528503418f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.4862745106220245f, 0.4862745106220245f, 0.4862745106220245f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.4862745106220245f, 0.4862745106220245f, 0.4862745106220245f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.7799999713897705f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.4588235318660736f, 0.5372549295425415f, 0.800000011920929f, 0.6000000238418579f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.4000000059604645f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.05882352963089943f, 0.529411792755127f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.3100000023841858f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 0.6200000047683716f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1372549086809158f, 0.4392156898975372f, 0.800000011920929f, 0.7799999713897705f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1372549086809158f, 0.4392156898975372f, 0.800000011920929f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.3490196168422699f, 0.3490196168422699f, 0.3490196168422699f, 0.1700000017881393f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.949999988079071f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.7607843279838562f, 0.7607843279838562f, 0.7607843279838562f, 0.9309999942779541f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.TabSelected] = new Vector4(0.9254902124404907f, 0.9254902124404907f, 0.9254902124404907f, 1.0f);
            //style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.5921568870544434f, 0.7254902124404907f, 0.8823529481887817f, 1.0f);
            //style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.9176470637321472f, 0.9254902005195618f, 0.9333333373069763f, 0.9861999750137329f);
            //style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.7411764860153198f, 0.8196078538894653f, 0.9137254953384399f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.4274509847164154f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.8980392217636108f, 0.6980392336845398f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.4470588266849518f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.7764706015586853f, 0.8666666746139526f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.5686274766921997f, 0.5686274766921997f, 0.6392157077789307f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.6784313917160034f, 0.6784313917160034f, 0.7372549176216125f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(0.2980392277240753f, 0.2980392277240753f, 0.2980392277240753f, 0.09000000357627869f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.3499999940395355f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.949999988079071f);
            //style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(0.6980392336845398f, 0.6980392336845398f, 0.6980392336845398f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 0.3499999940395355f);
        }

        public static void SetTheme(bool in_IsDark)
        {
            if (in_IsDark)
                SetDarkTheme();
            else
                SetLightTheme();
        }
    }
}
