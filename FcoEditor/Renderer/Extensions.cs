using Converse.Rendering;
using Converse.ShurikenRenderer;
using libfco;
using System;
using System.Text;

public static class ExtensionKillMe
{

    /// <summary>
    /// Get the highlight color based on the character position in the cell.
    /// </summary>
    /// <param name="in_Cell"></param>
    /// <param name="in_PosIdx"></param>
    /// <returns>Color of the highlight, if there is none, it'll return the cell's normal color.</returns>
    public static CellColor? FindHighlightFromPos(this Cell in_Cell, int in_PosIdx)
    {
        foreach (var highlight in in_Cell.Highlights)
        {
            if (in_PosIdx >= highlight.Start && in_PosIdx <= highlight.End)
                return highlight;
        }
        return in_Cell.MainColor;
    }
    public static bool IsNull(this Sprite spr)
    {
        return (spr == null || spr.Texture.GlTex == null);
    }
    public unsafe static byte* StringToBytePointer(this string str)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        // Convert the string to a byte array
        byte[] byteArray = Encoding.UTF8.GetBytes(str + "\0"); // Add null-terminator

        // Pin the byte array in memory
        fixed (byte* bytePointer = byteArray)
        {
            return bytePointer; // This pointer is valid only within the fixed block!
        }
    }
    
}
