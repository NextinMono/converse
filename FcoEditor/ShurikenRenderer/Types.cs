﻿using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Numerics;

namespace Shuriken.Rendering
{
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
    public class Sprite
    {
        public readonly int ID;
        public ConverseEditor.ShurikenRenderer.Vector2 Start { get; set; }
        public ConverseEditor.ShurikenRenderer.Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        // Used for saving to avoid corruption in un-edited values
        public float OriginalTop { get; set; }
        public float OriginalBottom { get; set; }
        public float OriginalLeft { get; set; }
        public float OriginalRight { get; set; }
        public bool HasChanged { get; set; }

        public int X
        {
            get { return (int)Start.X; }
            set { Start.X = value; CreateCrop(); HasChanged = true; }
        }

        public int Y
        {
            get { return (int)Start.Y; }
            set { Start.Y = value; CreateCrop(); HasChanged = true; }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
            set
            {
                if (X + value <= Texture.Width)
                {
                    Dimensions.X = value;
                    CreateCrop();
                    HasChanged = true;
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
                    Dimensions.Y = value;
                    CreateCrop();
                    HasChanged = true;
                }
            }
        }

        public CroppedBitmap Crop { get; set; }

        private void CreateCrop()
        {
            if (X + Width <= Texture.Width && Y + Height <= Texture.Height)
            {
                if (Width > 0 && Height > 0)
                    Crop = new CroppedBitmap(Texture.ImageSource, new Int32Rect(X, Y, Width, Height));
            }
        }

        public Sprite(int id, Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            ID = id;
            Texture = tex;

            Start = new Vector2(MathF.Round(left * tex.Width), MathF.Round(top * tex.Height));
            Start.X = Math.Clamp(Start.X, 0, Texture.Width);
            Start.Y = Math.Clamp(Start.Y, 0, Texture.Height);

            Dimensions = new Vector2(MathF.Round((right - left) * tex.Width), MathF.Round((bottom - top) * tex.Height));
            CreateCrop();

            OriginalTop = top;
            OriginalLeft = left;
            OriginalBottom = bottom;
            OriginalRight = right;
            HasChanged = false;
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
            HasChanged = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}