using Kunai.ShurikenRenderer;

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
    public static System.Numerics.Vector4 ToVec4(this Color<byte> value)
    {
        return new System.Numerics.Vector4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, value.A / 255.0f);
    }
    public static System.Numerics.Vector4 Invert(this System.Numerics.Vector4 value)
    {
        return new System.Numerics.Vector4(value.W, value.Z, value.Y, value.X);
    }
    public static System.Numerics.Vector4 Invert_ToVec4(this Color<byte> value)
    {
        return new System.Numerics.Vector4(value.A / 255.0f, value.B / 255.0f, value.G / 255.0f, value.R / 255.0f);
    }
    public static Color<byte> Invert_ToColor(this System.Numerics.Vector4 value)
    {
        return new Color<byte>((byte)(value.W * 255), (byte)(value.Z * 255), (byte)(value.Y * 255), (byte)(value.X * 255));
    }
    public static double Magnitude(this Color<byte> value)
    {
        return Math.Sqrt(value.R * value.R + value.G * value.G + value.B * value.B + value.A * value.A);
    }
    public static Color<byte> ToSharpNeedleColor(this System.Numerics.Vector4 value)
    {
        return new Color<byte>((byte)(value.X * 255), (byte)(value.Y * 255), (byte)(value.Z * 255), (byte)(value.W * 255));
    }
    public static Color<byte> ToSharpNeedleColorInverted(this System.Numerics.Vector4 value)
    {
        return new Color<byte>((byte)(value.W * 255), (byte)(value.Z * 255), (byte)(value.Y * 255), (byte)(value.X * 255));
    }

}
public static class AnimationTypeMethods
{
    public static bool IsColor(this AnimationType type)
    {
        return new AnimationType[] {
                AnimationType.Color,
                AnimationType.GradientTL,
                AnimationType.GradientBL,
                AnimationType.GradientTR,
                AnimationType.GradientBR
            }.Contains(type);
    }
    public static int FindKeyframe(this SharpNeedle.Ninja.Csd.Motions.KeyFrameList list, float frame)
    {
        int min = 0;
        int max = list.Count - 1;

        while (min <= max)
        {
            int index = (min + max) / 2;

            if (frame < list[index].Frame)
                max = index - 1;
            else
                min = index + 1;
        }

        return min;
    }
    public static float GetSingle(this SharpNeedle.Ninja.Csd.Motions.KeyFrameList list, float frame)
    {
        if (list.Count == 0)
            return 0.0f;

        if (frame >= list[^1].Frame)
            return list[^1].Value.Float;

        int index = list.FindKeyframe(frame);

        if (index == 0)
            return list.Frames[index].Value.Float;

        var keyframe = list.Frames[index - 1];
        var nextKeyframe = list.Frames[index];

        float factor;

        if (nextKeyframe.Frame - keyframe.Frame > 0)
            factor = (frame - keyframe.Frame) / (nextKeyframe.Frame - keyframe.Frame);
        else
            factor = 0.0f;

        switch (keyframe.Interpolation)
        {
            case SharpNeedle.Ninja.Csd.Motions.InterpolationType.Linear:
                return (1.0f - factor) * keyframe.Value.Float + nextKeyframe.Value.Float * factor;

            case SharpNeedle.Ninja.Csd.Motions.InterpolationType.Hermite:
                float valueDelta = nextKeyframe.Value.Float - keyframe.Value.Float;
                float frameDelta = nextKeyframe.Frame - keyframe.Frame;

                float biasSquaric = factor * factor;
                float biasCubic = biasSquaric * factor;

                float valueCubic = (keyframe.OutTangent + keyframe.InTangent) * frameDelta - valueDelta * 2.0f;
                float valueSquaric = valueDelta * 3.0f - (keyframe.InTangent * 2.0f + keyframe.OutTangent) * frameDelta;
                float valueLinear = frameDelta * keyframe.InTangent;

                return valueCubic * biasCubic + valueSquaric * biasSquaric + valueLinear * factor + keyframe.Value.Float;

            default:
                return keyframe.Value.Float;
        }
    }
    public static Color<byte> Invert(this Color<byte> color)
    {
        return new Color<byte>(color.A, color.B, color.G, color.R);
    }
    public static Color<byte> GetColor(this SharpNeedle.Ninja.Csd.Motions.KeyFrameList list, float frame)
    {
        if (list.Count == 0)
            return new Color<byte>();

        if (frame >= list.Frames[^1].Frame)
            return list.Frames[^1].Value.Color.Invert();

        int index = list.FindKeyframe(frame);

        if (index == 0)
            return list.Frames[index].Value.Color.Invert();

        var keyframe = list.Frames[index - 1];
        var nextKeyframe = list.Frames[index];

        float factor;

        if (nextKeyframe.Frame - keyframe.Frame > 0)
            factor = (frame - keyframe.Frame) / (nextKeyframe.Frame - keyframe.Frame);
        else
            factor = 0.0f;

        // Color values always use linear interpolation regardless of the type.
        var swappedCurrent = keyframe.Value.Color.Invert();
        var swappedNext = nextKeyframe.Value.Color.Invert();
        return new Color<byte>
        {
            R = (byte)((1.0f - factor) * swappedCurrent.R + swappedNext.R * factor),
            G = (byte)((1.0f - factor) * swappedCurrent.G + swappedNext.G * factor),
            B = (byte)((1.0f - factor) * swappedCurrent.B + swappedNext.B * factor),
            A = (byte)((1.0f - factor) * swappedCurrent.A + swappedNext.A * factor)
        };
    }
    public static AnimationType ToShurikenAnimationType(this SharpNeedle.Ninja.Csd.Motions.KeyProperty test)
    {
        switch (test)
        {
            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.HideFlag:
                return AnimationType.HideFlag;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.PositionX:
                return AnimationType.XPosition;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.PositionY:
                return AnimationType.YPosition;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.Rotation:
                return AnimationType.Rotation;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.ScaleX:
                return AnimationType.XScale;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.ScaleY:
                return AnimationType.YScale;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.SpriteIndex:
                return AnimationType.SubImage;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.Color:
                return AnimationType.Color;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.GradientTopLeft:
                return AnimationType.GradientTL;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.GradientBottomLeft:
                return AnimationType.GradientBL;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.GradientTopRight:
                return AnimationType.GradientTR;

            case SharpNeedle.Ninja.Csd.Motions.KeyProperty.GradientBottomRight:
                return AnimationType.GradientBR;

        }
        return AnimationType.None;
    }
}