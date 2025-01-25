namespace SonicUnleashedFCOConv {
    public class Program {
        public static string? fileDir, fileName, currentDir, tableArg;
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("SonicUnleashedFCOConv v1.0\nUsage: SonicUnleashedFCOConv <Path to .fte/.fco/.xml>");
                Console.ReadKey();
            }

            if (args.Length == 2) {
                currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                fileDir = Path.GetDirectoryName(args[0]);
                fileName = Path.GetFileName(args[0]);
                tableArg = args[1];
                string file = fileDir + "\\" + fileName;
                
                if (file.EndsWith(".fco")) {
                    FCO.ReadFCO(args[0]);
                    FCO.WriteXML(args[0]);
                }
            }
            else {
                currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                fileDir = Path.GetDirectoryName(args[0]);
                fileName = Path.GetFileName(args[0]);
                string file = fileDir + "\\" + fileName;

                if(File.Exists(file)) {
                    if (file.EndsWith(".fte")) {
                        FTE.ReadFTE(args[0]);
                        FTE.WriteXML(args[0]);
                    }
                    if (file.EndsWith(".fco")) {
                        FCO.ReadFCO(args[0]);
                        FCO.WriteXML(args[0]);
                    }
                    if (file.EndsWith(".xml")) {
                        XML.ReadXML(args[0]);
                        
                        if (XML.FCO) {
                            if (Common.ErrorCheck() == false) XML.WriteFCO(args[0]);
                        }
                        else {
                            if (Common.ErrorCheck() == false) XML.WriteFTE(args[0]);
                            Common.ExtractCheck();
                        }
                        
                    }
                }
                else {
                    Console.WriteLine($"Can't find file " + Path.GetFileName(args[0]) + ", aborting.");
                }
            }
        }
    }
}