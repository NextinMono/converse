﻿using System.IO;
using System.Threading.Tasks;
namespace ConverseEditor
{
    class Program
    {
        public static string[] arguments;
        public static string? Directory
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        public static string? ResourcesDirectory
        {
            get
            {
                return Path.Combine(Directory, "Resources");
            }
        }

        static void Main(string[] args)
        {
            MainWindow wnd = new MainWindow();
            wnd.Title = MainWindow.applicationName;
            arguments = args;
            Task.Run(UpdateChecker.CheckUpdate);
            wnd.Run();
        }
    }
}