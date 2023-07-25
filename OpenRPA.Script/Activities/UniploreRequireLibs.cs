using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

using OpenRPA.Interfaces;
using System.Diagnostics;

namespace OpenRPA.Script
{
    internal class UniploreRequireLibs
    {
        public static void checkEmbeddedLibs(string appDir)
        {
            string pyHome = Python.Included.Installer.EmbeddedPythonHome;
            string seleniumPath = pyHome + "\\Lib\\site-packages\\selenium";
            if (!Directory.Exists(seleniumPath)){
                var fi = new System.IO.FileInfo(appDir + @"\Resources\python-pip-and-uniplore-requirelibs.zip");
                if(!fi.Exists)
                {
                    Log.Warning("uniplore required libs not found: " + fi.FullName);
                }
                else
                {
                    try
                    {
                        // 解压依赖包到一个随机的临时目录
                        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        Directory.CreateDirectory(tempDirectory);
                        Log.Information("extract to: " + tempDirectory);

                        ZipFile.ExtractToDirectory(fi.FullName, tempDirectory);

                        // 批量安装
                        Process process = new Process();
                        process.StartInfo.WorkingDirectory = tempDirectory;
                        process.StartInfo.FileName = tempDirectory+ @"\install.bat";
                        process.StartInfo.Arguments = pyHome; // 使用/c参数执行命令并关闭CMD窗口
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string err = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        int code = process.ExitCode;

                        if (code != 0 )
                        {
                            Log.Warning("install failed: code=" + code);
                            Log.Information("stdout is:\n" + output);
                            Log.Information("stderr is:\n" + err);
                        }
                        else
                        {
                            Log.Information("install info: code=" + code + ", output=\n" + output);
                            Log.Information("install info: code=" + code + ", err=\n" + err);
                        }

                        Directory.Delete(tempDirectory, true);
                    }
                    catch(Exception ex)
                    {
                        Log.Warning("install uniplore python libs failed: "+ ex.ToString());
                    }
                    
                }
            }
        }
    }
}
