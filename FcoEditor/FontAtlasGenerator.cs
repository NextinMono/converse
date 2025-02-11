using Amicitia.IO.Binary;
using ConverseEditor.ShurikenRenderer;
using ConverseEditor.Utility;
using OpenTK.Mathematics;
using StbTrueTypeSharp;
using SUFcoTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConverseEditor
{
    class CharacterBitmapInfo
    {
        public int Letter;
        public System.Numerics.Vector2 Position;
        public System.Numerics.Vector2 Size;

    }
    public class FontAtlasGenerator
    {
        public static unsafe void TryCreateFteTexture(System.Numerics.Vector2 in_Size, List<TranslationTable.Entry> in_Entries, FontTexture in_FTE)
        {
            List<List<CharacterBitmapInfo>> lines = new List<List<CharacterBitmapInfo>>();
            var dialog1 = NativeFileDialogSharp.Dialog.FileOpen("otf, ttf");
            string fontPath = "";
            string ftePathNew = "";

            if (dialog1.IsOk)
                fontPath = dialog1.Path;
            else
                return;

            var dialog2 = NativeFileDialogSharp.Dialog.FileSave("fte", ConverseProject.config.WorkFilePath);
            if (dialog2.IsOk)
                ftePathNew = dialog2.Path;
            else
                return;

            int kerning = (int)(0.005f * in_Size.X);
            var texturePath = Path.Combine(Directory.GetParent(ftePathNew).FullName, in_FTE.Textures[2].Name + ".png");

            StbTrueType.stbtt_fontinfo font = null;
            // Load the font
            byte[] fontData = File.ReadAllBytes(fontPath);
            font = new StbTrueType.stbtt_fontinfo();

            fixed (byte* fontPtr = fontData)
            {
                StbTrueType.stbtt_InitFont(font, fontPtr, 0);
            }
            float fontSize = 32f;

            float fontSizeNormal = (fontSize / 512f) * in_Size.X;
            float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, fontSizeNormal);
            Vector2i spacing = new Vector2i((int)((7.0f / 512.0f) * in_Size.X), (int)((4.0f / 512.0f) * in_Size.Y));

            // Get some stats from the font
            // Ascent is the distance between the baseline and the top of the font
            // Descent is the recommended distance below the baseline for singled spaced text
            // LineGap is the distance between lines
            int fontAscent, fontDescent, fontLineSpace;
            StbTrueType.stbtt_GetFontVMetrics(font, &fontAscent, &fontDescent, &fontLineSpace);
            int fontBaseline = (int)(fontAscent * scale);

            // Character metrics
            int x = 0, y = 0, maxRowHeight = 0;

            lines.Add(new List<CharacterBitmapInfo>());
            using (Bitmap atlas = new Bitmap((int)in_Size.X, (int)in_Size.Y, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(atlas))
                {
                    g.Clear(Color.Black);
                    foreach (var c in in_Entries)
                    {
                        if (c.Letter.Length > 1 || c.Letter.Length == 0 || c.Letter == "\n")
                            continue;
                        var character = c.Letter[0];
                        Vector2i charaBmpTopLeft, charaBmpBottomRight;

                        //GetCodepointBitmapBox = how big the bitmap must be
                        StbTrueType.stbtt_GetCodepointBitmapBox(font, character, scale, scale, &charaBmpTopLeft.X, &charaBmpTopLeft.Y, &charaBmpBottomRight.X, &charaBmpBottomRight.Y);

                        // Get character dimensions
                        Vector2i size, offset;
                        byte* bitmap = StbTrueType.stbtt_GetCodepointBitmap(font, 0, scale, character, &size.X, &size.Y, &offset.X, &offset.Y);

                        if (bitmap == null)
                        {
                            //Space will always have a null bitmap, but we have to account for it
                            //cause otherwise text will have no spaces
                            if (character == ' ')
                            {
                                Console.Write("");
                                StbTrueType.stbtt_GetCodepointBitmap(font, 0, scale, '0', &size.X, &size.Y, &offset.X, &offset.Y);
                                //Make it 1 pixel high
                                size.Y = 1;
                            }
                            else
                                continue;
                        }

                        // Move to next row if necessary
                        if (x + size.X > in_Size.X)
                        {
                            x = 0;
                            y += maxRowHeight + (spacing.Y * 2);
                            maxRowHeight = 0;
                            lines.Add(new List<CharacterBitmapInfo>());
                        }
                        //Start of the glyph itself
                        int glyphY = y + fontBaseline + charaBmpTopLeft.Y;
                        maxRowHeight = Math.Max(maxRowHeight, size.Y);

                        // Copy glyph data into the bitmap
                        for (int j = 0; j < size.Y; j++)
                        {
                            for (int i = 0; i < size.X; i++)
                            {
                                byte alpha = bitmap != null ? bitmap[j * size.X + i] : (byte)0;
                                if (alpha != 0)
                                    atlas.SetPixel(x + i, glyphY + j, Color.FromArgb(alpha, 255, 255, 255));
                                else
                                    atlas.SetPixel(x + i, glyphY + j, Color.FromArgb(255, 0, 0, 0));
                            }
                        }
                        var newChara = new CharacterBitmapInfo
                        {
                            Letter = c.ConverseID,
                            Position = new System.Numerics.Vector2(x, y),
                            Size = new System.Numerics.Vector2(x + size.X + kerning, glyphY + size.Y)
                        };
                        lines[^1].Add(newChara);

                        // Add spacing
                        x += size.X + spacing.X + (kerning);
                        StbTrueType.stbtt_FreeBitmap(bitmap, null);
                    }
                    atlas.Save(@texturePath, ImageFormat.Png);
                    foreach (var sizeList in lines)
                    {
                        var highestLetter = sizeList.OrderByDescending(x => x.Size.Y).ToList()[0].Size.Y;
                        for (int j = 0; j < sizeList.Count; j++)
                        {
                            sizeList[j].Size.Y = highestLetter;
                        }
                    }
                    for (int i = 0; i < in_FTE.Characters.Count; i++)
                    {
                        if (in_FTE.Characters[i].TextureIndex == 2)
                        {
                            foreach (var sizeList in lines)
                            {
                                for (int j = 0; j < sizeList.Count; j++)
                                {
                                    if (in_FTE.Characters[i].CharacterID == sizeList[j].Letter)
                                    {
                                        var charaInfo = in_FTE.Characters[i];
                                        charaInfo.TopLeft = sizeList[j].Position / in_Size;
                                        charaInfo.BottomRight = sizeList[j].Size / in_Size;
                                        in_FTE.Characters[i] = charaInfo;
                                    }
                                }
                            }

                        }
                    }
                    // Save the texture atlas
                }
            }

            var tex = in_FTE.Textures[2];
            in_FTE.Textures[2] = tex;
            BinaryObjectWriter writer = new BinaryObjectWriter(ftePathNew, Endianness.Big, Encoding.UTF8);
            writer.WriteObject(in_FTE);
        }
    }
}
