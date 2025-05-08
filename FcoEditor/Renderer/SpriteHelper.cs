using libfco;
using System.Collections.Generic;
using System.Numerics;
using Sprite = Converse.Rendering.Sprite;
using Texture = Converse.Rendering.Texture;

namespace Converse.ShurikenRenderer
{
    public static class SpriteHelper
    {
        public static List<CharacterSprite> ConverseSprites = new List<CharacterSprite>();
        public static Dictionary<int, Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();
        public static List<Texture> Textures { get; set; } = new List<Texture>();

        public static Sprite GetSpriteFromConverseID(int converseID)
        {
            foreach (var v in ConverseSprites)
            {
                if (v.converseChara.CharacterID == converseID)
                {
                    return v.sprite;
                }
            }
            return null;
        }
        public static Sprite TryGetSprite(int id)
        {
            Sprites.TryGetValue(id, out Sprite sprite);
            return sprite;
        }
        public static int AppendSprite(Sprite spr)
        {
            Sprites.Add(Sprites.Count, spr);
            return Sprites.Count - 1;
        }
        public static void DeleteSprite(int in_SprIndex)
        {
            Sprites.TryGetValue(in_SprIndex, out Sprite sprite);
            sprite.Texture.CropIndices.Remove(in_SprIndex);
            Sprites.Remove(in_SprIndex);
        }
        public static int CreateSprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Sprite spr = new Sprite(tex, top, left, bottom, right);
            int newId = AppendSprite(spr);
            tex.CropIndices.Add(newId);
            return newId;
        }
        /// <summary>
        /// Converts all stored sprites back into FTE characters.
        /// </summary>
        /// <param name="in_SubImages"></param>
        /// <param name="in_TextureSizes"></param>
        public static void BuildCharaList(ref List<Character> in_SubImages)
        {
            in_SubImages = new();
            foreach (var entry in ConverseSprites)
            {
                Sprite sprite = entry.sprite;
                int textureIndex = Textures.IndexOf(sprite.Texture);

                var size = sprite.Texture.Size;
                //sprite.GenerateCoordinates(size);

                Character subImage = entry.converseChara;
                subImage.CharacterID = GetConverseIDFromSprite(sprite);
                subImage.TextureIndex = textureIndex;
                subImage.TopLeft = new Vector2((float)sprite.X / size.X, (float)sprite.Y / size.Y);
                subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / size.X, (float)(sprite.Y + sprite.Height) / size.Y);
                in_SubImages.Add(subImage);
            }
        }

        public static int GetConverseIDFromSprite(Sprite in_Spr)
        {
            foreach (var v in ConverseSprites)
            {
                if (v.sprite == in_Spr)
                {
                    return v.converseChara.CharacterID;
                }
            }
            return -1;
        }        

        public static void LoadTextures(List<Character> in_Characters)
        {
            Sprites.Clear();
            ConverseSprites.Clear();
            GetSubImages(in_Characters);
        }
        public static void GetSubImages(List<Character> in_Characters)
        {
            foreach (Character item in in_Characters)
            {
                //if (!CharSprites.Contains(x item))
                //{
                int textureIndex = (int)item.TextureIndex;
                int id = CreateSprite(Textures[textureIndex], item.TopLeft.Y, item.TopLeft.X,
                    item.BottomRight.Y, item.BottomRight.X);
                ConverseSprites.Add(new CharacterSprite(item, id));
                //}
            }
        }

        internal static void ClearTextures()
        {
            foreach (var f in Textures)
            {
                f.Destroy();
            }
            Textures.Clear();
            Sprites.Clear();
        }
    }
}