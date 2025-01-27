﻿using FcoEditor.ShurikenRenderer;
using System;
using System.Text;

public static class ExtensionKillMe
{
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
