using libfco;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sprite = Converse.Rendering.Sprite;
using Texture = Converse.Rendering.Texture;

namespace Converse.ShurikenRenderer
{
    public static class SpriteHelper
    {
        /// <summary>
        /// List of converse characters with their respective sprite
        /// </summary>
        public static List<CharacterSprite> ConverseSprites = new List<CharacterSprite>();

        /// <summary>
        /// List of all loaded sprites with their ID
        /// </summary>
        public static Dictionary<int, Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();

        /// <summary>
        /// List of all loaded texture files
        /// </summary>
        public static List<Texture> Textures { get; set; } = new List<Texture>();

        /// <summary>
        /// Fetch a sprite based only on the Character ID.
        /// </summary>
        /// <param name="converseID"></param>
        /// <returns>Sprite</returns>
        public static Sprite GetSpriteFromConverseID(int converseID)
        {
            foreach (CharacterSprite v in ConverseSprites)
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
        private static int AppendSprite(Sprite spr)
        {
            Sprites.Add(Sprites.Count, spr);
            return Sprites.Count - 1;
        }
        public static void DeleteCharacter(int in_SprID)
        {
            for (int i = 0; i < ConverseSprites.Count; i++)
            {
                if (ConverseSprites[i].spriteId == in_SprID)
                {
                    DeleteSprite(ConverseSprites[i].spriteId);
                    return;
                }
            }
        }
        public static void DeleteCharacter(Character in_Chara)
        {
            for (int i = 0; i < ConverseSprites.Count; i++)
            {
                if (ConverseSprites[i].converseChara.CharacterID == in_Chara.CharacterID)
                {
                    DeleteSprite(ConverseSprites[i].spriteId);
                    return;
                }
            }
        }
        public static void DeleteSprite(int in_SprIndex)
        {
            Sprites.TryGetValue(in_SprIndex, out Sprite sprite);
            sprite.Texture.CropIndices.Remove(in_SprIndex);
            Sprites.Remove(in_SprIndex);
        }

        /// <summary>
        /// Create a new cropped Sprite from a texture.
        /// </summary>
        /// <param name="tex">Texture</param>
        /// <param name="top">Top UV coordinate</param>
        /// <param name="left">Left UV coordinate</param>
        /// <param name="bottom">Bottom UV coordinate</param>
        /// <param name="right">Right UV coordinate</param>
        /// <returns>Sprite ID</returns>
        public static int CreateSprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Sprite spr = new Sprite(tex, top, left, bottom, right);
            int newId = AppendSprite(spr);
            tex.CropIndices.Add(newId);
            return newId;
        }

        public static int CreateCharacter(Texture in_Tex, int in_CharacterID = -1, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            var res = CreateSprite(in_Tex, top, left, bottom, right);
            if(in_CharacterID == -1)
            {
                var highestID = ConverseSprites.Max(x => x.converseChara.CharacterID);
                in_CharacterID = highestID + 1;
            }
            Character character = new Character();
            character.CharacterID = in_CharacterID;
            ConverseSprites.Add(new CharacterSprite(character, res));
            return ConverseSprites.Count - 1;
        }
        /// <summary>
        /// Converts all stored sprites back into FTE characters.
        /// </summary>
        /// <param name="in_SubImages"></param>
        /// <param name="in_TextureSizes"></param>
        public static void BuildCharaList(ref List<Character> in_SubImages)
        {
            in_SubImages = new();
            foreach (CharacterSprite entry in ConverseSprites)
            {
                Sprite sprite = entry.sprite;
                int textureIndex = Textures.IndexOf(sprite.Texture);

                var size = sprite.Texture.Size;
                //sprite.GenerateCoordinates(size);

                Character subImage = entry.converseChara;
                subImage.CharacterID = entry.converseChara.CharacterID;
                subImage.TextureIndex = textureIndex;
                subImage.TopLeft = new Vector2((float)sprite.X / size.X, (float)sprite.Y / size.Y);
                subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / size.X, (float)(sprite.Y + sprite.Height) / size.Y);
                in_SubImages.Add(subImage);
            }
        }
        /// <summary>
        /// Fetch the correct Character ID from a Sprite.
        /// </summary>
        /// <param name="in_Spr"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load necessary textures from FTE.
        /// </summary>
        /// <param name="in_Characters"></param>
        public static void LoadTextures(List<Character> in_Characters)
        {
            Sprites.Clear();
            ConverseSprites.Clear();
            CreateCropsFromFte(in_Characters);
        }
        public static void CreateCropsFromFte(List<Character> in_Characters)
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