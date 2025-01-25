using System.IO;

namespace Kunai
{
    class Program
    {
        public static string[] arguments;
        static void Main(string[] args)
        {
            MainWindow wnd = new MainWindow();
            arguments = args;
            wnd.Run();
        }
    }
}
