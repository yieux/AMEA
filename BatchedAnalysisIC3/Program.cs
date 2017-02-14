using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchedAnalysisIC3
{

    class Program
    {
        static String sourcepath = "D:/Android/evaluation/malware";

        //remote server
        static String puttypath = "d:/putty-0.65cn";
        static String remoteusername = "";
        static String remotepassword = "";
        static String remoteip = "192.168.130.28";
        static String remoteport = "22";
        static String remotepath = "/home/yangxy/example";
        static String remotedare = "/home/yangxy/tool/ic3/dare-1.1.0-linux/dare";
        static String remoteic3 = "/home/yangxy/tool/ic3/ic3-0.2.0/ic3-0.2.0-full.jar";
        static String remoteandroidjar = "/home/yangxy/tool/ic3/ic3-0.2.0/android.jar";
        
        static String analysisname = "IC3";
        static String filepostfix = ".apk";
        static Boolean TESTMODE = true;//just test one app in test mode
        static void Main(string[] args)
        {
            Console.WriteLine("remote user name:");
            remoteusername = Console.ReadLine();
            Console.WriteLine("remote password:");
            remotepassword = Console.ReadLine();


            Console.WriteLine(Distribute(sourcepath));


            Console.ReadKey();
        }

        public static int Distribute(String directory)
        {
            DirectoryInfo theFolderClass = new DirectoryInfo(directory);
            DirectoryInfo[] dirInfoClass = theFolderClass.GetDirectories();
            //int[] dirNum = new int[dirInfo.Length];
            //int k = 0;
            int num = 0;
            //遍历文件夹

            foreach (DirectoryInfo NextFolderClass in dirInfoClass)
            {

                DirectoryInfo[] dirInfoApp = NextFolderClass.GetDirectories();

                foreach (DirectoryInfo NextFolderApp in dirInfoApp)
                {
                    FileInfo[] fileAPKApp = NextFolderApp.GetFiles();

                    foreach(FileInfo NextFileInfo in fileAPKApp)
                    {
                        OperationForOneApp(NextFileInfo);
                        break;//just one apk file in one folder
                    }
                    if (TESTMODE)
                        break;
                }
                if (TESTMODE)
                    break;

                //num += Directory.GetFiles(NextFolder.FullName).Length;
                //String filenum = Directory.GetFiles(NextFolder.FullName).Length.ToString("0000");
                //Directory.Move(NextFolder.FullName, NextFolder.Parent.FullName + "\\" + filenum + "_" + NextFolder.Name);
                //k++;

            }
            return num;
        }

        public static int OperationForOneApp(FileInfo fileAPKInfo)
        {
            AnalysisForOneApp(fileAPKInfo);
            TidyResultForOneApp();
            return 0;
        }

        public static int AnalysisForOneApp(FileInfo fileAPKInfo)
        {

            string path = fileAPKInfo.DirectoryName + "\\" + analysisname;
            Directory.CreateDirectory(path);
            //send apk file. such as "d:/putty-0.65cn/pscp -l yangxy -pw 123456 D:/Android/Analysis/00/app20160927mx/app20160927mx.apk yangxy@192.168.130.28:/home/yangxy/example"
            RunCmd2(puttypath + "/pscp", " -l " + remoteusername + " -pw " + remotepassword + " " + fileAPKInfo.FullName + " " + remoteusername + "@" + remoteip + ":" + remotepath);

            //generate .sh that used for analysis.
            String appname = fileAPKInfo.Name.Remove(fileAPKInfo.Name.LastIndexOf("."));
            String classname = Directory.GetParent(fileAPKInfo.Directory.FullName).Name;
            GenerateShell(path, appname, classname);

            //excute analysis. such as "d:/putty-0.65cn/putty -l yangxy -pw 123456 -P 22 -m D:/Android/Analysis/2/1.sh  yangxy@192.168.130.28"
            RunCmd2(puttypath + "/putty", " -l " + remoteusername + " -pw " + remotepassword + " -P " + remoteport + " -m " + path + "/cool.sh" + " " + remoteusername + "@" + remoteip);


            //obtain result files. such as "d:/putty-0.65cn/pscp -r -l yangxy -pw 123456 yangxy@192.168.130.28:/home/yangxy/example/00/app20160927mx/IC3 D:/Android/Analysis/ic3-0.2.0-bin/1/"
            RunCmd2(puttypath + "/pscp", " -r -l " + remoteusername + " -pw " + remotepassword + " " + remoteusername + "@" + remoteip + ":" + remotepath + "/"+classname+"/"+appname+"/"+analysisname + " " + fileAPKInfo.DirectoryName);
            
            return 0;
        }

        public static int TidyResultForOneApp()
        {
            return 0;
        }


        public static int GenerateShell(String ic3Path, String appName, String className)
        {
            String dareout = "dareout";
            String ic3result = "ic3result";

            String remoteapppath = remotepath + "/" + className + "/" + appName;
            String content = "";
            content += "if [ ! -d \"" + remoteapppath+ "\" ]; then" + "\n";
            content += "  mkdir -p " + remoteapppath + "\n";
            content += "fi" + "\n";
            content += "cd " + remotepath + "/" + className + "/" + appName + "\n";
            content += "mv ../../" + appName + filepostfix + " ./" + appName + filepostfix + "\n";
            content += "if [ ! -d \"./" + analysisname + "\" ]; then" + "\n";
            content += "  rm -rf ./" + analysisname + "\n";
            content += "fi" + "\n";
            content += "mkdir ./" + analysisname + "\n";
            content += "cd ./" + analysisname + "\n";
            content += remotedare + " -d " + remoteapppath + "/" + analysisname + "/" + dareout + " " + remoteapppath + "/" + appName + filepostfix + "\n";
            content += "mkdir ./" + ic3result + "\n";
            content += "java -jar " + remoteic3 + " -apkormanifest ../" + appName + filepostfix + " -input ./" + dareout + "/retargeted/" + appName + " -cp " + remoteandroidjar + " -out ./" + ic3result + " -protobuf ./" + ic3result + "\n";

            FileStream fs = new FileStream(ic3Path+"/cool.sh",FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(content);
            sw.Flush();
            sw.Close();
            fs.Close();
            return 0;
            //if [ ! -d "/home/yangxy/example/00/app20160927mx" ]; then
            //  mkdir -p /home/yangxy/example/00/app20160927mx
            //fi
            //cd /home/yangxy/example/00/app20160927mx
            //mv ../../app20160927mx.apk ./app20160927mx.apk
            //if [ ! -d "./IC3" ]; then
            //  rm -rf ./IC3
            //fi
            //mkdir ./IC3
            //cd ./IC3
            ///home/yangxy/tool/ic3/dare-1.1.0-linux/dare -d /home/yangxy/example/00/app20160927mx/IC3/dareout /home/yangxy/example/00/app20160927mx/app20160927mx.apk
            //mkdir ./ic3result
            //java -jar /home/yangxy/tool/ic3/ic3-0.2.0/ic3-0.2.0-full.jar -apkormanifest ../app20160927mx.apk -input ./dareout/retargeted/app20160927mx -cp /home/yangxy/tool/ic3/ic3-0.2.0/android.jar -out ./ic3result -protobuf ./ic3result
        }


                /// <summary>
        /// 运行cmd命令
        /// 会显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        static bool RunCmd2(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit(10000);
                    
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }
    }
}
