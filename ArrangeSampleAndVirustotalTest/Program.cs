using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ArrangeSampleAndVirustotalTest
{
    class Program
    {
        static String samplepath = "D:\\Android\\evaluation\\malware";
        static void Main(string[] args)
        {
            TraversalDInDirectory(samplepath);
        }


        public static int TraversalDInDirectory(String directory)
        {
            DirectoryInfo theFolder = new DirectoryInfo(directory);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            int num = 0;
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                
                    num += TraversalFInDirectory(NextFolder);
            }
            return num;
        }


        public static int TraversalFInDirectory(DirectoryInfo directory)
        {
            Console.WriteLine("--------work for  " + directory.FullName + "  -----------");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            int num = 0;
            FileInfo[] fileInfo = directory.GetFiles();
            foreach (FileInfo NextFile in fileInfo)  //遍历文件
            {
                
                FileOperation(NextFile);
                num++;
                if (num % 2 == 0)
                {
                    watch.Stop();
                    if (watch.ElapsedMilliseconds < 35000)
                        Thread.Sleep((int)(35000- watch.ElapsedMilliseconds));
                    watch.Restart();
                }
                
            }
            watch.Stop();
            return num;
        }

        public static int FileOperation(FileInfo fi)
        {
            /////////////////// init
            String singlepath = fi.DirectoryName + "\\" + fi.Name;

            File.Copy(fi.FullName, fi.FullName + ".apk");
            File.Delete(fi.FullName);
            if (!Directory.Exists(singlepath))
            {
                Directory.CreateDirectory(singlepath);
            }
            File.Copy(fi.FullName + ".apk", singlepath + "\\" + fi.Name+".apk");
            File.Delete(fi.FullName + ".apk");
            
            if (File.Exists(singlepath + "\\" + fi.Name + ".apk"))
                fi = new FileInfo(singlepath + "\\" + fi.Name + ".apk");
            else
                return 1;

            /////////////// virustotal
            VirustotalTest(fi);



            return 0;

        }

        public static int VirustotalTest(FileInfo sample)
        {
            String virustotalpath = sample.DirectoryName + "\\virustotal";
            if (!Directory.Exists(virustotalpath))
            {
                Directory.CreateDirectory(virustotalpath);
            }
            
            //RunCmd2("cmd.exe", "python "+ Environment.CurrentDirectory + "\\virt.py -v -s "+ sample.FullName + " -r " + sample.FullName +  " -l "+ virustotalpath+"\\out.txt");
            doPython("python", Environment.CurrentDirectory + "\\virt.py -v -s " + sample.FullName + " -r " + sample.FullName + " -l " + virustotalpath + "\\out.txt");
            Thread.Sleep(2000);

            String data;
            StreamReader sr = new StreamReader(virustotalpath + "\\out.txt", Encoding.Default);
            data = sr.ReadToEnd();
            while (data.Length<5000)
            {
                sr.Close();
                File.Delete(virustotalpath + "\\out.txt");
                Thread.Sleep(5000);
                doPython("python", Environment.CurrentDirectory + "\\virt.py -v -s " + sample.FullName + " -r " + sample.FullName + " -l " + virustotalpath + "\\out.txt");
                Thread.Sleep(2000);
                sr = new StreamReader(virustotalpath + "\\out.txt", Encoding.Default);
                data = sr.ReadToEnd();
            }

            sr.Close();
            FileStream fs = new FileStream(virustotalpath + "\\res.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            String used;
            String res_s;
            int b, e;
            b = 0;
            e = 0;
            //scan_id
            b = data.IndexOf("scan_id",e);
            b += 11;
            e = data.IndexOf("\"",b+1);
            res_s = data.Substring(b,e-b);
            sw.WriteLine(res_s);
            //sha1
            b = data.IndexOf("sha1", e);
            b += 8;
            e = data.IndexOf("\"", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            //resource
            b = data.IndexOf("resource", e);
            b += 12;
            e = data.IndexOf("\"", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            //sha256
            b = data.IndexOf("sha256", e);
            b += 10;
            e = data.IndexOf("\"", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            //permalink
            b = data.IndexOf("permalink", e);
            b += 13;
            e = data.IndexOf("\"", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            //md5
            b = data.IndexOf("md5", e);
            b += 7;
            e = data.IndexOf("\"", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);

            //"total"
            b = data.IndexOf("\"total\"", e);
            b += 9;
            e = data.IndexOf(",", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            //"positives"
            b = data.IndexOf("\"positives\"", e);
            b += 13;
            e = data.IndexOf(",", b + 1);
            res_s = data.Substring(b, e - b);
            sw.WriteLine(res_s);
            int num = int.Parse(res_s);
            FileStream numfile;
            if (!File.Exists(virustotalpath + "\\" + res_s))
            {
                numfile = new FileStream(virustotalpath + "\\" + res_s, FileMode.Create);
                numfile.Close();
            }

            sw.WriteLine();
            // virus 
            b = e = 0;
            
            try {
                while (e < data.Length&&b<data.Length&&num>0) {
                    b = data.IndexOf("\"result\"", e);
                    b += 11;
                    if (data.Substring(b - 1, 4).Equals("null"))
                    {
                        e = b + 1;
                        continue;
                    }
                    
                        e = data.IndexOf("\"", b + 1);
                    res_s = data.Substring(b, e - b);
                    //if (!res_s.Equals("ull, "))
                        sw.WriteLine(res_s);
                    num--;
                    //b = e;
                }
            }
            catch (Exception ex)
            { }


            sw.WriteLine();
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();

            return 0;
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
                    myPro.WaitForExit();

                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        private static void doPython(string StartFileName, string StartFileArg)
        {
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = StartFileName;      // 命令  
            CmdProcess.StartInfo.Arguments = StartFileArg;      // 参数  

            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口  
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入  
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出  
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;  



            CmdProcess.Start();
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。  
            CmdProcess.WaitForExit();
            CmdProcess.Close();
        }

    }
}
