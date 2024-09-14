using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

using OpenRPA.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;

namespace OpenRPA.Script
{
    public class UniploreRequireLibs
    {
        private static bool hasChecked = false;
        private static JObject customerDisplayConfig = null;
        private static ISet<string> hiddens = null;
        private static bool logDisplayConfig = false;


        public static void LoadCustomerDisplayConfig()
        {
            if (customerDisplayConfig == null)
            {
                customerDisplayConfig = GetCustomerDisplayConfig();
                ISet<string> hiddenSet = new HashSet<string>();

                try
                {
                    if (customerDisplayConfig.ContainsKey("instructs"))
                    {
                        JObject instructs = (JObject)customerDisplayConfig["instructs"];
                        if (instructs.ContainsKey("hiddens"))
                        {
                            JArray hiddensArr = (JArray)instructs["hiddens"];
                            foreach (var v in hiddensArr)
                            {
                                hiddenSet.Add((string)v);
                            }
                        }
                    }

                    if (customerDisplayConfig.ContainsKey("log"))
                    {
                        logDisplayConfig = (bool)customerDisplayConfig["log"];
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("加载指令配置信息失败: " + ex.ToString());
                }

                hiddens = hiddenSet;
            }
        }

        public static string GetCustomerName(string libraryName, string typeName, string originName, string partName = "toolbox")
        {
            string finalName = null;

            if (customerDisplayConfig != null && customerDisplayConfig.ContainsKey("customerNames"))
            {
                JObject customerNames = (JObject)customerDisplayConfig["customerNames"];

                JObject toolbox = (JObject)customerNames[partName];

                if (libraryName != null && toolbox.ContainsKey(libraryName))
                {
                    JObject names = (JObject)toolbox[libraryName];
                    if (names.ContainsKey(typeName))
                    {
                        string customerName = (string)names[typeName];
                        if (customerName?.Length > 0)
                        {
                            finalName = customerName;
                        }
                    }
                }

                if (finalName == null && libraryName != "*" && toolbox.ContainsKey("*"))
                {
                    JObject names = (JObject)toolbox["*"];
                    if (names.ContainsKey(typeName))
                    {
                        string customerName = (string)names[typeName];
                        if (customerName?.Length > 0)
                        {
                            finalName = customerName;
                        }
                    }
                }
            }

            if (finalName == null)
            {
                finalName = originName?.Length > 0 ? originName : typeName;
            }

            if (logDisplayConfig)
            {
                Log.Information($"libraryName={libraryName}, typeName={typeName}, originName={originName}, finalName={finalName}");
            }


            return finalName;
        }

        public static bool HideActivityMethod(string libraryName, string activityName)
        {
            bool hide = false;

            if (hiddens?.Count > 0 && hiddens.Contains(activityName))
            {
                hide = true;
            }

            if (logDisplayConfig)
            {
                Log.Information($"Hide activity method: libraryName={libraryName}, activityName={activityName}, hide={hide}");
            }

            return hide;
        }

        /// <summary>
        /// 配置文件格式：
        /// <code>
        /// {
        ///   "log": false,
        ///   "customerNames": {
        ///     "toolbox": {
        ///       "*": {},
        ///       "OpenRPA": {}
        ///     }
        ///   },
        ///   "instructs": {
        ///     "hiddens": ["Name"]
        ///   }
        /// }
        /// </code>
        /// <list type="bullet">
        /// <item>
        /// log：表示是否输出处理日志
        /// </item>
        /// <item>
        /// customerNames.toolbox：用于设置工具箱指令显示名称，key为模块名称，value为配置对象（对象的key为指令名称， value为显示名称）
        /// </item>
        /// <item>
        /// instructs.hiddens：用于隐藏默认指令（非自定义）
        /// </item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public static JObject GetCustomerDisplayConfig()
        {
            string host;
            if (string.IsNullOrEmpty(Config.local.wsurl))
            {
                host = "offline";
            }
            else
            {
                var uri = new Uri(Config.local.wsurl);
                host = uri.Host;
            }
            var fi = new FileInfo(Config.SettingsFile);
            var baseDir = Path.Combine(fi.Directory.FullName, "script-activities", host);
            if (!Directory.Exists(baseDir))
            {
                Log.Information("create script-activities dir: " + baseDir);
                Directory.CreateDirectory(baseDir);
            }

            var configFile = Path.Combine(baseDir, "customer-display.json");
            JObject configObject = null;
            if (File.Exists(configFile))
            {
                Log.Information("load customer names config: " + configFile);
                try
                {
                    var content = ReadFileContent(configFile);
                    configObject = JObject.Parse(content);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "load customer names config error: " + configFile);
                }
            }

            if (configObject == null)
            {
                configObject = new JObject();
            }

            return configObject;
        }

        public static string ReadFileContent(string file)
        {
            string content;
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
            }

            return content;
        }

        public static void ShowBalloonTip(string title, string content, int timeout = 5000, ToolTipIcon icon = ToolTipIcon.Info)
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
            }
            else
            {
                var fi = new System.IO.FileInfo(appDir + @"\Resources\python-pip-and-uniplore-requirelibs.zip");
                if (!fi.Exists)
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

                        if (code != 0)
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

                        //InvokeCode.InitPython();
                        //using (Python.Runtime.Py.GIL())
                        //{
                        //    PythonEngine.RunSimpleString("import importlib\n\nimportlib.invalidate_caches()\n");
                        //}

                        ShowBalloonTip("提示", "python依赖安装完成");
                        hasChecked = true;
                        Log.Information("python home: " + pyHome);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("install uniplore python libs failed: " + ex.ToString());
                        ShowBalloonTip("错误", "python依赖安装失败！", 5000, ToolTipIcon.Error);
                    }

                }
            }
        }
    }
}
