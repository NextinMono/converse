using Converse.ShurikenRenderer;

namespace Converse
{
    public static partial class ImConverse
    {
        public struct STextureSelectorResult
        {
            public int TextureIndex;
            public int SpriteIndex;
            public STextureSelectorResult(int in_TextureIndex, int in_SpriteIndex)
            {
                TextureIndex = in_TextureIndex;
                SpriteIndex = in_SpriteIndex;
            }
            public bool IsCropSelected()
            {
                return TextureIndex != -2 && SpriteIndex != -2;
            }
            public int GetSpriteIndex()
            {
                return SpriteHelper.Textures[TextureIndex].CropIndices[SpriteIndex] - 1;
            }
        }
    }
}
