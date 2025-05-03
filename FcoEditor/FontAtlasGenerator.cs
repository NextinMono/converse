using Amicitia.IO.Binary;
using Converse.ShurikenRenderer;
using Converse.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbTrueTypeSharp;
using libfco;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp.Drawing.Processing;
using System.Runtime.CompilerServices;
using HekonrayBase;
namespace Converse
{
    class CharacterBitmapInfo
    {
        public int Letter;
        public System.Numerics.Vector2 Position;
        public System.Numerics.Vector2 Size;

    }
    public class FontAtlasSettings
    {
        public System.Numerics.Vector2 FontAtlasSize = new System.Numerics.Vector2(2048, 2048);
        public string FontPath = "";
        public string FtePath = "";
        public float FontSize = 32f;
        public float Kerning = 0.005f;
        public System.Numerics.Vector2 InterCharacterSpacing = new System.Numerics.Vector2(7, 4);
    }
    public class FontAtlasGenerator
    {
        public static unsafe byte[] TryCreateFteTexture(FontAtlasSettings in_Settings, List<TranslationTable.Entry> in_Entries, FontTexture in_FTE)
        {

            List<List<CharacterBitmapInfo>> lines = new List<List<CharacterBitmapInfo>>();
            string fontPath = in_Settings.FontPath;
            string ftePathNew = Path.Combine(Directory.GetParent(in_Settings.FtePath).FullName, "fte_ConverseMain_Generated.fte");
            if (string.IsNullOrEmpty(fontPath))
                return null;
            int kerning = (int)(in_Settings.Kerning * in_Settings.FontAtlasSize.X);
            var texturePath = Path.Combine(Directory.GetParent(ftePathNew).FullName, in_FTE.Textures[2].Name + ".png");

            StbTrueType.stbtt_fontinfo font = null;
            // Load the font
            byte[] fontData = File.ReadAllBytes(fontPath);
            font = new StbTrueType.stbtt_fontinfo();

            fixed (byte* fontPtr = fontData)
            {
                StbTrueType.stbtt_InitFont(font, fontPtr, 0);
            }

            float fontSizeNormal = (in_Settings.FontSize / 512f) * in_Settings.FontAtlasSize.X;
            float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, fontSizeNormal);
            Vector2Int spacing = new Vector2Int((int)((in_Settings.InterCharacterSpacing.X / 512.0f) * in_Settings.FontAtlasSize.X), (int)((in_Settings.InterCharacterSpacing.Y / 512.0f) * in_Settings.FontAtlasSize.Y));

            // Get some stats from the font
            // Ascent is the distance between the baseline and the top of the font
            // Descent is the recommended distance below the baseline for singled spaced text
            // LineGap is the distance between lines
            int fontAscent, fontDescent, fontLineSpace;
            StbTrueType.stbtt_GetFontVMetrics(font, &fontAscent, &fontDescent, &fontLineSpace);
            int fontBaseline = (int)(fontAscent * scale);

            // Character metrics
            int x = 0, y = 0, maxRowHeight = 0;

            byte[] stream = new byte[(int)(in_Settings.FontAtlasSize.X * in_Settings.FontAtlasSize.Y * Unsafe.SizeOf<Rgba32>())];
            lines.Add(new List<CharacterBitmapInfo>());
            using (var atlas = new Image<Rgba32>((int)in_Settings.FontAtlasSize.X, (int)in_Settings.FontAtlasSize.Y))
            {
                atlas.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.Black));

                foreach (var c in in_Entries)
                {
                    if (c.Letter.Length > 1 || c.Letter.Length == 0 || c.Letter == "\n")
                        continue;

                    var character = c.Letter[0];
                    Vector2Int charaBmpTopLeft = new();
                    Vector2Int charaBmpBottomRight = new();

                    // Get character dimensions
                    Vector2Int size, offset;
                    byte* bitmap = StbTrueType.stbtt_GetCodepointBitmap(font, 0, scale, character, &size.X, &size.Y, &offset.X, &offset.Y);

                    if (bitmap == null)
                    {
                        if (character == ' ')
                        {
                            StbTrueType.stbtt_GetCodepointBitmap(font, 0, scale, '0', &size.X, &size.Y, &offset.X, &offset.Y);
                            size.Y = 1;
                        }
                        else
                            continue;
                    }

                    if (x + size.X > in_Settings.FontAtlasSize.X)
                    {
                        x = 0;
                        y += maxRowHeight + (spacing.Y * 2);
                        maxRowHeight = 0;
                        lines.Add(new List<CharacterBitmapInfo>());
                    }

                    int glyphY = y + fontBaseline + charaBmpTopLeft.Y;
                    maxRowHeight = Math.Max(maxRowHeight, size.Y);

                    // Copy glyph data into the bitmap
                    for (int j = 0; j < size.Y; j++)
                    {
                        for (int i = 0; i < size.X; i++)
                        {
                            byte alpha = bitmap != null ? bitmap[j * size.X + i] : (byte)0;
                            var color = alpha != 0 ? new Rgba32(255, 255, 255, alpha) : new Rgba32(0, 0, 0, 255);
                            atlas[x + i, glyphY + j] = color;
                        }
                    }

                    var newChara = new CharacterBitmapInfo
                    {
                        Letter = c.ConverseID,
                        Position = new System.Numerics.Vector2(x, y),
                        Size = new System.Numerics.Vector2(x + size.X + kerning, glyphY + size.Y)
                    };
                    lines[^1].Add(newChara);

                    x += size.X + spacing.X + (kerning);
                    StbTrueType.stbtt_FreeBitmap(bitmap, null);
                }

                foreach (var sizeList in lines)
                {
                    var highestLetter = sizeList.Max(x => x.Size.Y);
                    foreach (var chara in sizeList)
                    {
                        chara.Size.Y = highestLetter;
                    }
                }

                for(int a = 0; a < in_FTE.Characters.Count; a++)
                {
                    if (in_FTE.Characters[a].TextureIndex >= 2)
                        continue;
                    for (int b = 0; b < lines.Count; b++)
                    {
                        List<CharacterBitmapInfo> sizeList = lines[b];
                        for (int c = 0; c < sizeList.Count; c++)
                        {
                            CharacterBitmapInfo sizeItem = sizeList[c];
                            if (in_FTE.Characters[a].CharacterID == sizeItem.Letter)
                            {
                                var crop = in_FTE.Characters[a];
                                crop.TopLeft = sizeItem.Position / in_Settings.FontAtlasSize;
                                crop.BottomRight = sizeItem.Size / in_Settings.FontAtlasSize;
                                in_FTE.Characters[a] = crop;
                            }
                        }
                    }
                }

                atlas.CopyPixelDataTo(stream);
            }

            var tex = in_FTE.Textures[2];
            in_FTE.Textures[2] = tex;
            BinaryObjectWriter writer = new BinaryObjectWriter(ftePathNew, Endianness.Big, Encoding.UTF8);
            writer.WriteObject(in_FTE);
            return stream;
        }
    }
}
