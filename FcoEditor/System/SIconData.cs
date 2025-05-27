using System.Numerics;

namespace Converse
{
    public struct SIconData
    {
        public string Icon;
        public string Name;
        public Vector4 Color = Vector4.One;
        public SIconData(string in_Icon)
        {
            Icon = in_Icon;
        }
        public SIconData(string in_Icon, Vector4 in_Color)
        {
            Icon = in_Icon;
            Color = in_Color;
        }
        public SIconData(string in_Icon, string in_Name, Vector4 in_Color)
        {
            Name = in_Name;
            Icon = in_Icon;
            Color = in_Color;
        }
        public bool IsNull() => string.IsNullOrEmpty(Icon);
    }
}
