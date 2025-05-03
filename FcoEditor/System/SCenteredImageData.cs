using System.Numerics;

namespace Converse
{
    public struct SCenteredImageData
    {
        public Vector2 Position;
        public Vector2 WindowPosition;
        public Vector2 ImageSize;
        public Vector2 ImagePosition;

        public SCenteredImageData(Vector2 in_CursorPos2, Vector2 in_WindowPos, Vector2 in_ScaledViewportSize, Vector2 in_FixedViewportPosition)
        {
            Position = in_CursorPos2;
            WindowPosition = in_WindowPos;
            ImageSize = in_ScaledViewportSize;
            ImagePosition = in_FixedViewportPosition;
        }
    }
}
