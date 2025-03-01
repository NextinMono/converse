using ConverseEditor.Settings;
using ConverseEditor.ShurikenRenderer;
using Hexa.NET.ImGui;
using TeamSpettro.SettingsSystem;

namespace ConverseEditor
{
    public class SettingsWindow : Window
    {
        internal static SettingsWindow instance;
        public static SettingsWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsWindow();
                }
                return instance;
            }
        }
        public static bool Enabled = false;
        bool _themeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");

        public override void Render(ConverseProject in_Renderer)
        {
            if (Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Settings"))
                {
                    int currentTheme = _themeIsDark ? 1 : 0;
                    if (ImGui.Combo("Theme", ref currentTheme, ["Light", "Dark"], 2))
                    {
                        _themeIsDark = currentTheme == 1;
                        SettingsManager.SetBool("IsDarkThemeEnabled", _themeIsDark);
                        ImGuiThemeManager.SetTheme(_themeIsDark);
                    }
                    ImGui.End();
                }
            }
        }
    }
}