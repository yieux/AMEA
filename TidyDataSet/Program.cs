using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TidyDataSet
{
    class Program
    {
        static String sourcepath = "D:\\Android\\Drebin";
        static String csvpath = "D:\\Android\\Drebin\\sha256_family.csv";
        static String outpath = "D:\\Android\\Drebin\\Dity";
        static DataTable dt;
        static void Main(string[] args)
        {

            //ReadCSV(csvpath);
            //Console.WriteLine( TraversalDrebinDirectory(sourcepath));


            Console.WriteLine( DityOut(outpath));


            Console.ReadKey();


        }

        public static int ReadCSV(String path)
        {
            dt = new DataTable();
            dt = CSVFileHelper.OpenCSV(path);
            dt.PrimaryKey = new DataColumn[] { dt.Columns["sha256"] };
            return 0;

        }

        public static int TraversalDrebinDirectory(String directory)
        {
            DirectoryInfo theFolder = new DirectoryInfo(directory);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            int num = 0;
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                if ((new Regex("^drebin.*")).IsMatch(NextFolder.Name))
                {
                    num += TraversalDirectory(NextFolder);
                        }
            }
            return num;
        }


        public static int TraversalDirectory(DirectoryInfo directory)
        {
            Console.WriteLine("--------work for  "+directory.FullName+"  -----------");
            int num = 0;
                FileInfo[] fileInfo = directory.GetFiles();
                foreach (FileInfo NextFile in fileInfo)  //遍历文件
                {
                    FileOperation(NextFile);
                    num++;
                }
            
            return num;
        }

        public static int FileOperation(FileInfo fi)
        {
            String oneoutpath = outpath + "\\" + dt.Rows.Find(fi.Name).ItemArray.GetValue(1).ToString(); 

            if (!Directory.Exists(oneoutpath))
            {
                Directory.CreateDirectory(oneoutpath);
            }
            File.Copy(fi.FullName, oneoutpath + "\\" + fi.Name);
            return 0;

        }



        public static int DityOut(String directory)
        {
            DirectoryInfo theFolder = new DirectoryInfo(directory);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            //int[] dirNum = new int[dirInfo.Length];
            //int k = 0;
            int num = 0;
            //遍历文件夹
            
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                num += Directory.GetFiles(NextFolder.FullName).Length;
                String filenum = Directory.GetFiles(NextFolder.FullName).Length.ToString("0000");
                Directory.Move(NextFolder.FullName,NextFolder.Parent.FullName+"\\"+filenum+"_"+NextFolder.Name);
                //k++;
                
            }
            return num;
        }



    }
}
