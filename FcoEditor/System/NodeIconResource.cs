namespace Converse
{
    using IconFonts;

    public static class NodeIconResource
    {
        private static SIconData file = new SIconData(FontAwesome6.FolderClosed, ColorResource.File);
        private static SIconData group = new SIconData(FontAwesome6.Font, ColorResource.Group);
        private static SIconData highlight = new SIconData(FontAwesome6.Highlighter, ColorResource.File);
        private static SIconData subcell = new SIconData(FontAwesome6.Heading, ColorResource.File);
        private static SIconData extra = new SIconData(FontAwesome6.Plus, ColorResource.White);

        public static SIconData File => file;
        public static SIconData Group => group;
        public static SIconData Highlight => highlight;
        public static SIconData Extra => extra;
        public static SIconData Subcell => subcell;
    }
}