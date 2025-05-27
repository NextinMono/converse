using BrendanGrant.Helpers.FileAssociation;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace FileTypeRegisterService
{
    class Program
    {
        public static string path = System.Environment.ProcessPath;
        static void Main(string[] args)
        {
            AssociateFileTypes();
        }
        private static void AssociateFileTypeSingle(string in_Ext, string in_TypeDesc)
        {
            string parentPath = Directory.GetParent(in_Ext).FullName;
            string kunaiExecPath = Path.Combine(parentPath, "Converse.exe");
            FileAssociationInfo fai = new FileAssociationInfo($".{in_Ext}");
            if (fai.Exists)
                fai.Delete();
            if (!fai.Exists)
            {
                fai.Create($"Converse.{in_Ext}");

                //Specify MIME type (optional)
                fai.ContentType = "application/myfile";

                //Programs automatically displayed in open with list
                fai.OpenWithList = new string[] { "" };
            }
            ProgramAssociationInfo pai = new ProgramAssociationInfo(fai.ProgID);
            if (pai.Exists)
                pai.Delete();
            if (!pai.Exists)
            {
                string arg = "\"%1\"";
                pai.Create(in_TypeDesc, new ProgramVerb("Open", @$"{kunaiExecPath} {arg}"), new ProgramIcon(Path.Combine(parentPath, "Resources", "Icons", $"{in_Ext}.ico")));

                //optional
            }
        }


        public static void AssociateFileTypes()
        {
            AssociateFileTypeSingle("fco", "Font Converse file");
        }
    }
}
