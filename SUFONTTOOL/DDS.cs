using System.Drawing;
using System.Drawing.Imaging;
using SUFontTool;

namespace SonicUnleashedFCOConv {
    public static class DDS {
        public static int texCount = XML.texCount, charaCount = XML.charaCount, spriteIndex = 0;
        public static string outputFolder;
        public static List<Structs.Texture> textures = XML.textures;
        public static List<Structs.Character> characters = XML.characters;

        public static void Process() {
            outputFolder = Program.fileDir + "/sprites";

            if (Directory.Exists(outputFolder) == false) {
                Directory.CreateDirectory(outputFolder);
            }
            if (Directory.Exists(outputFolder)) {
                foreach (string file in Directory.GetFiles(outputFolder)) {
                    File.Delete(file);
                }
            }

            Console.WriteLine("WARNING: This feature is still Experimental!");
            Console.WriteLine("Provide the following files as PNG images");

            foreach(Structs.Texture texture in textures) {
                Console.WriteLine("Please provide the path of " + texture.textureName + ".");
                string ddsPath = Console.ReadLine().Replace("\"", "");

                if (ddsPath != null && Path.GetFileNameWithoutExtension(ddsPath) != null) {
                    int index = textures.FindIndex(0, texCount, texture => texture.textureName == Path.GetFileNameWithoutExtension(ddsPath));
                    ReadDDS(ddsPath, index);
                }
            }
        }

        public static void ReadDDS(string filePath, int extTexIndex) {
            Bitmap sourceImage = new Bitmap(filePath);
            
            foreach (var chara in characters) {
                // If the Character count has been hit/exceeded
                if (spriteIndex == charaCount) {
                    break;
                }

                int textureIndex = characters[spriteIndex].textureIndex;

                // If our grabbed index for the character matches our input from Process
                if (textureIndex == extTexIndex) {
                    string ConverseID = characters[spriteIndex].convID.ToString();
                    int p1_x = (int)characters[spriteIndex].charPoint1X;
                    int p1_y = (int)characters[spriteIndex].charPoint1Y;
                    int p2_x = (int)characters[spriteIndex].charPoint2X;
                    int p2_y = (int)characters[spriteIndex].charPoint2Y;

                    int width = p2_x - p1_x;
                    int height = p2_y - p1_y;

                    // Extract the sprite from the image
                    // Normal Sized Sprite
                    if (width > 0 && height > 0) {
                        Rectangle spriteRect = new Rectangle(p1_x, p1_y, width, height);
                        Bitmap sprite = sourceImage.Clone(spriteRect, sourceImage.PixelFormat);

                        string outputFilePath = Path.Combine(outputFolder, $"{ConverseID}.png");

                        sprite.Save(outputFilePath, ImageFormat.Png);
                        spriteIndex++;
                        sprite.Dispose();
                    }
                    // Blank Sprite or Broken Sprite
                    else {
                        Console.WriteLine($"Character {ConverseID} may be blank or non-existant.");

                        Rectangle spriteRect = new Rectangle(p1_x, p1_y, 1, 1);
                        Bitmap sprite = sourceImage.Clone(spriteRect, sourceImage.PixelFormat);

                        string outputFilePath = Path.Combine(outputFolder, $"{ConverseID}.png");

                        sprite.Save(outputFilePath, ImageFormat.Png);
                        spriteIndex++;
                        sprite.Dispose();
                    }
                }
                else {
                    /*  The Loop will continue until the correct index is found
                        However this shouldn't be possible with the new code..
                        Unless someone somehow finds a way to cause this lol    */
                }
            }

            Console.WriteLine("Sprites extracted successfully.");
        }
    }
}