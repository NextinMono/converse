
using Converse.Rendering;
using libfco;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Sprite = Converse.Rendering.Sprite;
using Texture = Converse.Rendering.Texture;
namespace Converse.ShurikenRenderer
{
    public struct Crop
    {
        public Character Character;
        public uint TextureIndex;
        public Vector2 TopLeft;
        public Vector2 BottomRight;
    }
    public class TextureList
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public List<Texture> Textures { get; set; } = new List<Texture>();

        public TextureList(string listName)
        {
            name = listName;
            Textures = new List<Texture>();
        }
    }
    public class UIFont
    {
        public int ID { get; private set; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public List<CharacterMapping> Mappings { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public UIFont(string name, int id)
        {
            ID = id;
            Name = name;
            Mappings = new List<CharacterMapping>();
        }
    }
    public class CharacterMapping
    {
        private char character;
        public char Character
        {
            get => character;
            set
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                    character = value;
            }
        }

        public int Sprite { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CharacterMapping(char c, int sprID)
        {
            Character = c;
            Sprite = sprID;
        }

        public CharacterMapping()
        {
            Sprite = -1;
        }
    }
    public static class SpriteHelper
    {
        public static Dictionary<Character, int> CharSprites = new Dictionary<Character, int>();
        public static Dictionary<int, Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();
        private static int NextSpriteID = 0;
        private static List<Crop> ncpSubimages = new List<Crop>();
        public static List<Texture> Textures { get; set; } = new List<Texture>();

        public static Sprite GetSpriteFromConverseID(int converseID)
        {
            foreach (var v in CharSprites)
            {
                if (v.Key.CharacterID == converseID)
                {
                    return TryGetSprite(v.Value);
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
            Sprites.Add(NextSpriteID, spr);
            return NextSpriteID++;
        }
        public static void DeleteSprite(int in_SprIndex)
        {
            Sprites.TryGetValue(in_SprIndex, out Sprite sprite);
            sprite.Texture.CropIndices.Remove(in_SprIndex);
            Sprites.Remove(in_SprIndex);
            NextSpriteID--;
        }
        public static int CreateSprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Sprite spr = new Sprite(tex, top, left, bottom, right);
            int newId = AppendSprite(spr);
            tex.CropIndices.Add(newId);
            return newId;
        }
        /// <summary>
        /// Create a list of Csd Crops from Kunai sprites.
        /// </summary>
        /// <param name="in_SubImages"></param>
        /// <param name="in_TextureSizes"></param>
        public static void BuildCropList(ref List<Character> in_SubImages)
        {
            in_SubImages = new();
            foreach (var entry in CharSprites)
            {
                Sprite sprite = Sprites[entry.Value];
                int textureIndex = Textures.IndexOf(sprite.Texture);

                var size = sprite.Texture.Size;
                //sprite.GenerateCoordinates(size);

                Character subImage = entry.Key;
                subImage.CharacterID = GetConverseIDFromSprite(sprite);
                subImage.TextureIndex = textureIndex;
                subImage.TopLeft = new Vector2((float)sprite.X / size.X, (float)sprite.Y / size.Y);
                subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / size.X, (float)(sprite.Y + sprite.Height) / size.Y);
                in_SubImages.Add(subImage);
            }
        }

        public static int GetConverseIDFromSprite(Sprite in_Spr)
        {
            foreach (var v in CharSprites)
            {
                if (Sprites[v.Value] == in_Spr)
                {
                    return v.Key.CharacterID;
                }
            }
            return -1;
        }
        

        public static void LoadTextures(List<Character> in_CsdProject)
        {
            ncpSubimages.Clear();
            Sprites.Clear();
            CharSprites.Clear();
            GetSubImages(in_CsdProject);
            LoadSubimages(ncpSubimages);
        }
        public static void GetSubImages(List<Character> node)
        {
            //foreach (var scene in node.Scenes)
            //{


            foreach (var item in node)
            {
                var i = new Crop();
                i.Character = item;
                i.TextureIndex = (uint)item.TextureIndex;
                i.TopLeft = new Vector2(item.TopLeft.X, item.TopLeft.Y);
                i.BottomRight = new Vector2(item.BottomRight.X, item.BottomRight.Y);
                ncpSubimages.Add(i);
            }
            //}

            //foreach (KeyValuePair<string, SceneNode> child in node.Children)
            //{
            //    if (ncpSubimages.Count > 0)
            //        return;
            //
            //    GetSubImages(child.Value);
            //}
        }
        private static void LoadSubimages(List<Crop> subimages)
        {
            foreach (var image in subimages)
            {
                int textureIndex = (int)image.TextureIndex;
                //if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                //{
                if (!CharSprites.ContainsKey(image.Character))
                {
                    int id = CreateSprite(Textures[textureIndex], image.TopLeft.Y, image.TopLeft.X,
                        image.BottomRight.Y, image.BottomRight.X);
                    CharSprites.Add(image.Character, id);
                }
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
            ncpSubimages.Clear();
            Sprites.Clear();
            NextSpriteID = 1;
        }
    }
}