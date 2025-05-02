namespace ConverseEditor
{
    using IconFonts;

    public static class NodeIconResource
    {
        private static SIconData file = new SIconData(FontAwesome6.FolderClosed, ColorResource.File);
        private static SIconData group = new SIconData(FontAwesome6.Font, ColorResource.Group);

        public static SIconData File => file;
        public static SIconData Group => group;
    }
}