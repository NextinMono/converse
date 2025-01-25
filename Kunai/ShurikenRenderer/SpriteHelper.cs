using SharpNeedle.Ninja.Csd;
using SharpNeedle.SurfRide.Draw;
using Shuriken.Rendering;
using SUFontTool;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Sprite = Shuriken.Rendering.Sprite;
using Texture = Shuriken.Rendering.Texture;
namespace Kunai.ShurikenRenderer
{
    public struct Crop
    {
        public Structs.Character Character;
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
        public static Dictionary<Structs.Character, int> CharSprites = new Dictionary<Structs.Character, int>();
        public static Dictionary<int, Shuriken.Rendering.Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();
        private static int NextSpriteID = 0;
        private static List<Crop> ncpSubimages = new List<Crop>();
        public static TextureList textureList;
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
        public static int CreateSprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Sprite spr = new Sprite(NextSpriteID, tex, top, left, bottom, right);
            return AppendSprite(spr);
        }
        public static void LoadTextures(List<Structs.Character> in_CsdProject)
        {
            ncpSubimages.Clear();
            Sprites.Clear();
            GetSubImages(in_CsdProject);
            LoadSubimages(textureList, ncpSubimages);
        }
        public static void GetSubImages(List<Structs.Character> node)
        {
            //foreach (var scene in node.Scenes)
            //{


            foreach (var item in node)
            {
                var i = new Crop();
                i.Character = item;
                i.TextureIndex = (uint)item.textureIndex;
                i.TopLeft = new Vector2(item.charPoint1X / textureList.Textures[item.textureIndex].Width, item.charPoint1Y / textureList.Textures[item.textureIndex].Height);
                i.BottomRight = new Vector2(item.charPoint2X / textureList.Textures[item.textureIndex].Width, item.charPoint2Y / textureList.Textures[item.textureIndex].Height);
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
        private static void LoadSubimages(Kunai.ShurikenRenderer.TextureList texList, List<Crop> subimages)
        {
            foreach (var image in subimages)
            {
                int textureIndex = (int)image.TextureIndex;
                if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                {
                    int id = CreateSprite(texList.Textures[textureIndex], image.TopLeft.Y, image.TopLeft.X,
                        image.BottomRight.Y, image.BottomRight.X);
                    CharSprites.Add(image.Character, id);
                    texList.Textures[textureIndex].Sprites.Add(id);
                }
            }
        }

        internal static void ClearTextures()
        {
            if (textureList == null)
                return;
            foreach(var f in textureList.Textures)
            {
                f.Destroy();
            }
            textureList.Textures.Clear();
            ncpSubimages.Clear();
            Sprites.Clear();
            NextSpriteID = 1;
        }
    }
}
