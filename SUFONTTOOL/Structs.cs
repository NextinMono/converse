namespace SUFontTool;

public static class Structs {
    public struct Group {
        public string groupName { get; set; }
        public List<Cell> cellList { get; set; }
    }

    public struct Cell {
        public string cellName { get; set; }
        public string cellMessage { get; set; }
        public byte[] cellMessageWrite { get; set; }
        public int messageCharAmount { get; set; }
        public Colour colourMain { get; set; }
        public Colour colourSub1 { get; set; }
        public Colour colourSub2 { get; set; }
        public string alignment { get; set; }
        public int highlightCount { get; set; }
        public List<Colour> highlightList { get; set; }
    }
    
    public struct Colour {
        public int colourStart { get; set; }
        public int colourEnd { get; set; }
        public int colourMarker { get; set; }
        public byte colourAlpha { get; set; }
        public byte colourRed { get; set; }
        public byte colourGreen { get; set; }
        public byte colourBlue { get; set; }
    }
    
    public enum TextAlign {
        left = 0,
        center = 1,
        right = 2,
        justified = 3
    }

    public struct Texture {
        public string textureName { get; set; }
        public int textureSizeX { get; set; }
        public int textureSizeY {get; set; }
    }

    public struct Character {
        public int textureIndex { get; set; }
        public string convID { get; set; }
        public float charPoint1X { get; set; }
        public float charPoint1Y { get; set; }
        public float charPoint2X { get; set; }
        public float charPoint2Y { get; set; }
    }
}