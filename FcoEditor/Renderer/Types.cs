using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Numerics;
using ConverseEditor.ShurikenRenderer;

namespace ConverseEditor.Rendering
{
    public class SLineInfo
    {
        public float width;
        public int amount;

        public SLineInfo(float in_Width, int in_Amount)
        {
            this.width = in_Width;
            this.amount = in_Amount;
        }
    }
    public struct Vertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public Vector4 Color;
    }
    public class Quad
    {
        public Vertex TopLeft;
        public Vertex TopRight;
        public Vertex BottomLeft;
        public Vertex BottomRight;
        public Texture Texture;
        public int ZIndex;
        public bool Additive;
        public bool LinearFiltering;
    }
    public struct Crop
    {
        public uint TextureIndex;
        public Vector2 TopLeft;
        public Vector2 BottomRight;
    }
    public class Sprite
    {
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        public Crop Crop;

        public Vector2 RelativeStart
        {
            get
            {
                return Start / Texture.Size;
            }
            set
            {
                Start = value * Texture.Size;
            }
        }
        public Vector2 RelativeSize
        {
            get
            {
                return Dimensions / Texture.Size;
            }
            set
            {
                Dimensions = value * Texture.Size;
            }
        }
        public int X
        {
            get { return (int)Start.X; }
            set { Start = new Vector2(value, Start.Y); }
        }

        public int Y
        {
            get { return (int)Start.Y; }
            set { Start = new Vector2(Start.X, value); }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
            set
            {
                if (X + value <= Texture.Width)
                {
                    Dimensions = new Vector2(value, Dimensions.Y);
                }
            }
        }

        public int Height
        {
            get { return (int)Dimensions.Y; }
            set
            {
                if (Y + value <= Texture.Height)
                {
                    Dimensions = new Vector2(Dimensions.X, value);
                }
            }
        }
        public Vector2[] GetImGuiUv()
        {
            Vector2 uvTl = new Vector2(
                Start.X / Texture.Width,
                -(Start.Y / Texture.Height));

            Vector2 uvBr = uvTl + new Vector2(
            Dimensions.X / Texture.Width,
            -(Dimensions.Y / Texture.Height));

            return [uvTl, uvBr];
        }

        public void Recalculate()
        {
            var textureSize = Texture.Size;
            Crop.TopLeft.X = Start.X / textureSize.X;
            Crop.TopLeft.Y = Start.Y / textureSize.Y;
            Crop.BottomRight.X = (Start.X + Dimensions.X) / textureSize.X;
            Crop.BottomRight.Y = (Start.Y + Dimensions.Y) / textureSize.Y;
        }
        public void GenerateCoordinates(Vector2 in_TextureSize)
        {
            var oLeft = Crop.TopLeft.X;
            var oRight = Crop.BottomRight.X;
            var oTop = Crop.TopLeft.Y;
            var oBtm = Crop.BottomRight.Y;
            var start1X = MathF.Round(oLeft * in_TextureSize.X);
            var start1Y = MathF.Round(oTop * in_TextureSize.Y);
            Start = new Vector2(start1X, start1Y);
            Start = new Vector2(Math.Clamp(Start.X, 0, in_TextureSize.X), Math.Clamp(Start.Y, 0, in_TextureSize.Y));
            Dimensions = new Vector2(MathF.Round((oRight - oLeft) * in_TextureSize.X), MathF.Round((oBtm - oTop) * in_TextureSize.Y));
        }
        public Sprite(Texture in_Tex, float in_Top = 0.0f, float in_Left = 0.0f, float in_Bottom = 1.0f, float in_Right = 1.0f)
        {
            Texture = in_Tex;

            Crop = new Crop();
            Crop.TextureIndex = (uint)SpriteHelper.Textures.IndexOf(in_Tex);
            Crop.TopLeft = new Vector2(in_Left, in_Top);
            Crop.BottomRight = new Vector2(in_Right, in_Bottom);
            GenerateCoordinates(in_Tex.Size);
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
        }
    }
}