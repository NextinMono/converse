using System.IO;

namespace FcoEditor
{
    class Program
    {
        public static string[] arguments;
        public static string? programDir
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        static void Main(string[] args)
        {
            MainWindow wnd = new MainWindow();
            arguments = args;
            wnd.Run();
        }
    }
}
