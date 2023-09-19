﻿using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

using Python.Runtime;
using OpenRPA.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using OpenRPA.Script.Activities;

namespace OpenRPA.Script
{
    internal class UniploreRequireLibs
    {
        private static bool hasChecked = false;

        public static void ShowBalloonTip(string title, string content, int timeout=5000, ToolTipIcon icon= ToolTipIcon.Info)
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(timeout, title, content, icon);
            notifyIcon.Click += (sender, e) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            };

            notifyIcon.DoubleClick += (sender, e) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            };
        }

        /// <summary>
        /// 需要调用的地方：OpenRPA.Script/Plugin.cs
        /// </summary>
        /// <param name="appDir"></param>
        public static void CheckEmbeddedLibs(string appDir)
        {
            if (hasChecked)
            {
                return;
            }

            string pyHome = Python.Included.Installer.EmbeddedPythonHome;
            string seleniumPath = pyHome + "\\Lib\\site-packages\\selenium";
            if (Directory.Exists(seleniumPath))
            {
                hasChecked = true;
            }else
            {
                var fi = new System.IO.FileInfo(appDir + @"\Resources\python-pip-and-uniplore-requirelibs.zip");
                if(!fi.Exists)
                {
                    Log.Warning("uniplore required libs not found: " + fi.FullName);
                    ShowBalloonTip("Error", "uniplore required libs not found", 5000, ToolTipIcon.Warning);
                }
                else
                {
                    try
                    {
                        ShowBalloonTip("提示", "正在安装python依赖，请稍等片刻...", 10000);

                        // 解压依赖包到一个随机的临时目录
                        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        Directory.CreateDirectory(tempDirectory);
                        Log.Information("extract to: " + tempDirectory);

                        ZipFile.ExtractToDirectory(fi.FullName, tempDirectory);

                        // 批量安装
                        Process process = new Process();
                        process.StartInfo.WorkingDirectory = tempDirectory;
                        process.StartInfo.FileName = pyHome + @"\python.exe";
                        process.StartInfo.Arguments = tempDirectory + @"\install.py";
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


                        // 测试是否安装成功, 同时能解决首次安装后无法找到模块的问题
                        //PythonEngine.Shutdown();
                        //Python.Included.Installer.SetupPython(false).Wait();

                        // Python.Runtime.PythonEngine.Initialize();
                        //_ = Python.Runtime.PythonEngine.BeginAllowThreads();
                       
                        /*InvokeCode.InitPython();
                        using (Python.Runtime.Py.GIL())
                        {
                            //var scope = Python.Runtime.Py.CreateScope();
                            //scope.Reload();
                            //PythonEngine.RunSimpleString("import sys\n\nfor k in sys.modules.keys():\n  del sys.modules[k]\n");
                            var rpatest = Py.Import("uniplore_rpatest.webdriver");
                            Log.Information("uniplore_rpatest: " + rpatest);
                        }*/

                        ShowBalloonTip("提示", "python依赖安装完成");
                        hasChecked = true;
                    }
                    catch(Exception ex)
                    {
                        Log.Warning("install uniplore python libs failed: "+ ex.ToString());
                        ShowBalloonTip("错误", "python依赖安装失败！", 5000, ToolTipIcon.Error);
                    }
                    
                }
            }
        }
    }
}
