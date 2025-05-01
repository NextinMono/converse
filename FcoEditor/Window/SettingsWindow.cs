using ConverseEditor.Settings;
using ConverseEditor.ShurikenRenderer;
using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using System.Numerics;
using TeamSpettro.SettingsSystem;

namespace ConverseEditor
{
    public class SettingsWindow : Singleton<MenuBarWindow>, IWindow
    {
        public static bool Enabled = false;
        bool _themeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");

        public void OnReset(IProgramProject in_Renderer)
        {
        }

        public void Render(IProgramProject in_Renderer)
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