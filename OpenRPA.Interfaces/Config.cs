﻿using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA
{
    public class Config
    {

        //添加对接配置开始：
        // enable: 是否启用，若为false将使用OpenRPA默认的路径，默认true
        // base_url: 基础地址，若为空、则从wsurl里获取（不含path路径）
        // login_path: 浏览器登录地址路径，与base_url拼接生成
        // add_token_request_path: 添加token接口路径，与base_url拼接生成
        // get_token_request_path: 获取token接口路径，与base_url拼接生成
        public Dictionary<string, object> uniplore_properties { get { return GetProperty(null, new Dictionary<string, object>()); } }
        public bool  uni_enable { get { return GetUniploreProperty(null, true); } }
        public string uni_base_url { get { return GetUniploreProperty(null, ""); } }
        public string uni_login_path { get { return !uni_enable ? "/Login" : GetUniploreProperty(null, "/uniplore-va/rpa/openRpaLogin"); } }
        public string uni_add_token_request_path { get { return !uni_enable ? "/AddTokenRequest" : GetUniploreProperty(null, "/uniplore-va/rpa/openRpaAddTokenRequest"); } }
        public string uni_get_token_request_path { get { return !uni_enable ? "/GetTokenRequest" : GetUniploreProperty(null, "/uniplore-va/rpa/openRpaGetTokenRequest"); } }

        private T GetUniploreProperty<T>(string pluginname, T mydefault, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }

                if (propertyName.StartsWith("uni_"))
                {
                    propertyName= propertyName.Substring(4);
                }

                object value = null;
                uniplore_properties.TryGetValue(propertyName, out value);

                if(value == null)
                {
                    value = mydefault;
                }
                else
                {
                    if (typeof(T) == typeof(int) && value is long)
                    {
                        value = int.Parse(value.ToString());
                    }else if (typeof(T) == typeof(bool))
                    {
                        value = bool.Parse(value.ToString());
                    }else if (typeof(T) == typeof(string[]) && value != null)
                    {
                        object o = null;
                        if (value.GetType() == typeof(string[]))
                        {
                            o = value;
                        }else if (value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                        {
                            o = ((Newtonsoft.Json.Linq.JArray)value).ToObject<string[]>();
                        }
                        value=o;
                    }else if(typeof(T) == typeof(double))
                    {
                        value = double.Parse(value.ToString());
                    }
                }

                return (T)value;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        //添加对接配置结束。
        ///////////////////////////////////////////////////////////////////

        public Dictionary<string, object> settings = new Dictionary<string, object>();
        public Dictionary<string, object> _properties = null;
        public Dictionary<string, object> properties { get { return GetProperty(null, new Dictionary<string, object>()); } set { SetProperty(null, value); } }

        public string wsurl { get { return GetProperty(null, "wss://app.openiap.io/"); } set { SetProperty(null, value); } }
        public string username { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public byte[] jwt { get { return GetProperty<byte[]>(null, null); } set { SetProperty(null, value); } }
        public byte[] password { get { return GetProperty<byte[]>(null, null); } set { SetProperty(null, value); } }
        public string unsafepassword { get { return GetProperty<string>(null, null); } set { SetProperty(null, value); } }
        public byte[] entropy { get { return GetProperty<byte[]>(null, null); } set { SetProperty(null, value); } }
        public string cancelkey { get { return GetProperty(null, "{ESCAPE}"); } set { SetProperty(null, value); } }
        public bool isagent { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public string culture { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public string ocrlanguage { get { return GetProperty(null, "eng"); } set { SetProperty(null, value); } }
        // public string[] openworkflows { get { return GetProperty(null, new string[] { }); } set { SetProperty(null, value); } }
        public string[] files_pending_deletion { get { return GetProperty(null, new string[] { }); } set { SetProperty(null, value); } }
        public System.Drawing.Rectangle mainwindow_position { get { return GetProperty(null, System.Drawing.Rectangle.Empty); } set { SetProperty(null, value); } }
        public string designerlayout { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public bool record_overlay { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public int querypagesize { get { return GetProperty(null, 50); } set { SetProperty(null, value); } }
        public DateTime lastupdatecheck { get { return GetProperty(null, DateTime.Now); } set { SetProperty(null, value); } }
        public TimeSpan updatecheckinterval { get { return GetProperty(null, TimeSpan.FromDays(1)); } set { SetProperty(null, value); } }
        public bool doupdatecheck { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool autoupdateupdater { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_to_file { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public int log_file_level_minimum { get { return GetProperty(null, NLog.LogLevel.Info.Ordinal); } set { SetProperty(null, value); } }
        public int log_file_level_maximum { get { return GetProperty(null, NLog.LogLevel.Fatal.Ordinal); } set { SetProperty(null, value); } }
        public bool log_verbose { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_network { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_activity { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_debug { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_selector { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_selector_verbose { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_information { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool log_output { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool log_warning { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool log_error { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool use_sendkeys { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool use_virtual_click { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool use_animate_mouse { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public TimeSpan use_postwait { get { return GetProperty(null, TimeSpan.Zero); } set { SetProperty(null, value); } }
        public bool minimize { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool recording_add_to_designer { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public TimeSpan reloadinterval { get { return GetProperty(null, TimeSpan.FromMinutes(5)); } set { SetProperty(null, value); } }
        public TimeSpan move_animation_run_time { get { return GetProperty(null, TimeSpan.FromMilliseconds(500)); } set { SetProperty(null, value); } }
        public int move_animation_steps { get { return GetProperty(null, 20); } set { SetProperty(null, value); } }
        public bool remote_allow_multiple_running { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool remote_allowed { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool remote_allowed_killing_self { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool remote_allowed_killing_any { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public int remote_allow_multiple_running_max { get { return GetProperty(null, 2); } set { SetProperty(null, value); } }
        public string cef_useragent { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public string getting_started_url { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public bool notify_on_workflow_remote_start { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool notify_on_workflow_end { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public bool notify_on_workflow_remote_end { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool log_busy_warning { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public string openflow_uniqueid { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public bool enable_analytics { get { return GetProperty(null, true); } set { SetProperty(null, value); } }
        public string otel_trace_url { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public string otel_metric_url { get { return GetProperty(null, ""); } set { SetProperty(null, value); } }
        public int otel_trace_interval { get { return GetProperty(null, 2); } set { SetProperty(null, value); } }
        public int otel_metric_interval { get { return GetProperty(null, 2); } set { SetProperty(null, value); } }
        public int max_projects { get { return GetProperty(null, 100); } set { SetProperty(null, value); } }
        public int max_workflows { get { return GetProperty(null, 500); } set { SetProperty(null, value); } }
        public int max_trace_lines { get { return GetProperty(null, 250); } set { SetProperty(null, value); } }
        public int max_output_lines { get { return GetProperty(null, 250); } set { SetProperty(null, value); } }
        public TimeSpan network_message_timeout { get { return GetProperty(null, TimeSpan.FromSeconds(15)); } set { SetProperty(null, value); } }
        public bool disable_instance_store { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public bool skip_online_state { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        public int thread_lock_timeout_seconds { get { return GetProperty(null, 10); } set { SetProperty(null, value); } }
        public bool skip_child_session_check { get { return GetProperty(null, false); } set { SetProperty(null, value); } }
        private void loadEntropy()
        {
            if (entropy == null || entropy.Length == 0)
            {
                entropy = new byte[20];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(entropy);
                }
            }
        }
        public byte[] ProtectString(string data)
        {
            loadEntropy();
            // Data to protect.
            byte[] plaintext = Encoding.UTF8.GetBytes(data);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] ciphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);
            return ciphertext;
        }
        public SecureString UnprotectString(byte[] data)
        {
            loadEntropy();
            // Data to protect.
            // byte[] plaintext = Encoding.UTF8.GetBytes(data);

            SecureString SecureData = new SecureString();
            byte[] ciphertext = ProtectedData.Unprotect(data, entropy, DataProtectionScope.CurrentUser);
            foreach (var c in Encoding.Default.GetString(ciphertext))
            {
                SecureData.AppendChar(c);
            }
            return SecureData;
        }
        private static Config _local = null;
        public static string SettingsFile
        {
            get
            {
                string filename = "settings.json";
                var fi = new System.IO.FileInfo(filename);
                var _fileName = System.IO.Path.GetFileName(filename);
                var di = fi.Directory;
                var MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var MyDocumentsOpenRPA = System.IO.Path.Combine(MyDocuments, "OpenRPA");
                var AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var AppDataOpenRPA = System.IO.Path.Combine(AppData, "OpenRPA");

                if (System.IO.File.Exists(System.IO.Path.Combine(AppDataOpenRPA, "settings.json")))
                {
                    filename = System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json");
                } else if (System.IO.File.Exists(System.IO.Path.Combine(MyDocumentsOpenRPA, "settings.json")))
                {
                    filename = System.IO.Path.Combine(MyDocumentsOpenRPA, "settings.json");
                }
                else if (System.IO.File.Exists(filename))
                {
                }
                else if (System.IO.File.Exists(System.IO.Path.Combine(di.Parent.FullName, "settings.json")))
                {
                    filename = System.IO.Path.Combine(di.Parent.FullName, "settings.json");
                }
                else
                {
                    // Will create a new file in ProjectsDirectory
                    filename = System.IO.Path.Combine(MyDocumentsOpenRPA, "settings.json");
                }
                return filename;
            }
        }
        public static Config local
        {
            get
            {
                if (_local == null)
                {
                    try
                    {
                        string filename = SettingsFile;
                        _local = new Config();
                        if (System.IO.File.Exists(filename))
                        {
                            var json = System.IO.File.ReadAllText(filename);
                            _local.settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        }
                        // _local = Load(filename);
                        // Hack to force updating old clients for new domain names
                        if (_local.wsurl == "wss://demo1.openrpa.dk/" || _local.wsurl == "wss://demo1.openrpa.dk")
                        {
                            _local.wsurl = "wss://app.openiap.io/";
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
                return _local;
            }
        }
        public static void Save()
        {
            try
            {
                _local.Save(System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json"));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public void Save(string filename)
        {
            try
            {
                _ = wsurl;
                _ = username;
                _ = jwt;
                _ = password;
                _ = entropy;
                _ = cancelkey;
                _ = isagent;
                _ = culture;
                _ = ocrlanguage;
                //_ = openworkflows;
                _ = mainwindow_position;
                _ = designerlayout;
                _ = record_overlay;
                _ = querypagesize;
                _ = lastupdatecheck;
                _ = updatecheckinterval;
                _ = doupdatecheck;
                _ = autoupdateupdater;
                _ = log_to_file;
                _ = log_file_level_minimum;
                _ = log_file_level_maximum;
                _ = log_verbose;
                _ = log_activity;
                _ = log_debug;
                _ = log_selector;
                _ = log_selector_verbose;
                _ = log_information;
                _ = log_output;
                _ = log_warning;
                _ = log_error;
                _ = use_sendkeys;
                _ = use_virtual_click;
                _ = use_animate_mouse;
                _ = use_postwait;
                _ = minimize;
                _ = recording_add_to_designer;
                _ = reloadinterval;
                _ = move_animation_run_time;
                _ = move_animation_steps;
                _ = remote_allow_multiple_running;
                _ = remote_allow_multiple_running_max;
                _ = remote_allowed;
                _ = cef_useragent;
                _ = getting_started_url;
                _ = notify_on_workflow_remote_start;
                _ = notify_on_workflow_end;
                _ = notify_on_workflow_remote_end;
                _ = log_busy_warning;
                _ = disable_instance_store;
                _ = skip_online_state;
                _ = thread_lock_timeout_seconds;
                _ = skip_child_session_check;

                _ = max_trace_lines;
                _ = max_output_lines;
                // settings
                // _properties
                var p = this.settings.OrderByDescending(kvp => kvp.Key);
                var d = new Dictionary<string, object>();
                foreach (var k in p) if (k.Key != "properties") d.Add(k.Key, k.Value);
                d.Add("properties", properties);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(d, Newtonsoft.Json.Formatting.Indented);
                var oldjson = "";
                if (System.IO.File.Exists(filename)) oldjson = System.IO.File.ReadAllText(filename);
                if(json != oldjson) System.IO.File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public static void Reload()
        {
            _local = null;
        }
        private bool? hasLocalMachine = null;
        private bool? hasCurrentUser = null;
        public bool GetRegistryProperty(string propertyname, out object value)
        {
            value = null;
            try
            {
                Microsoft.Win32.RegistryKey rk = null;
                if (hasLocalMachine == null)
                {
                    hasLocalMachine = false;
                    try
                    {
                        rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\OpenRPA", false);
                        if (rk != null) hasLocalMachine = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (hasLocalMachine == true)
                {
                    try
                    {
                        rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\OpenRPA", false);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (rk != null)
                {
                    var tempvalue = rk.GetValue(propertyname);
                    if (tempvalue != null)
                    {
                        value = tempvalue;
                        return true;
                    }

                }
                if (hasCurrentUser == null)
                {
                    hasCurrentUser = false;
                    try
                    {
                        rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\OpenRPA", false);
                        if (rk != null) hasCurrentUser = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (hasCurrentUser == true)
                {
                    rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\OpenRPA", false);
                }
                if (rk != null)
                {
                    var tempvalue = rk.GetValue(propertyname);
                    if (tempvalue != null)
                    {
                        value = tempvalue;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
        public T GetProperty<T>(string pluginname, T mydefault, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }
                string _propertyName = pluginname + "_" + propertyName;
                if (string.IsNullOrEmpty(pluginname)) _propertyName = propertyName;
                object value;
                if (GetRegistryProperty(_propertyName, out value))
                {
                }
                else if (propertyName != "properties")
                {
                    if (properties != null && !properties.TryGetValue(pluginname + "_" + propertyName, out value))
                    {
                    }
                }
                if (string.IsNullOrEmpty(pluginname) && value == null)
                {
                    if (settings != null && settings.TryGetValue(propertyName, out value))
                    {
                    }
                }
                if (value != null)
                {
                    if (typeof(T) == typeof(Dictionary<string, object>))
                    {
                        if (value.GetType() == typeof(Dictionary<string, object>))
                        {
                            return (T)value;
                        }
                        else if (!string.IsNullOrEmpty(value.ToString()))
                        {
                            value = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                        }
                        else
                        {
                            return default(T);
                        }
                    }
                    if (typeof(T) == typeof(byte[]))
                    {
                        if (value is string && !string.IsNullOrEmpty(value.ToString()))
                        {
                            value = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>("\"" + value.ToString() + "\"");
                        }
                        else if (value is byte[])
                        {
                            return (T)value;
                        }
                        else
                        {
                            return default(T);
                        }
                    }
                    if (typeof(T) == typeof(int) && value is long) value = int.Parse(value.ToString());
                    if (typeof(T) == typeof(bool)) value = bool.Parse(value.ToString());
                    if (typeof(T) == typeof(System.Drawing.Rectangle))
                    {
                        if (value.GetType() == typeof(System.Drawing.Rectangle))
                        {
                        }
                        else
                        {
                            try
                            {
                                var c = new System.Drawing.RectangleConverter();
                                value = c.ConvertFromString(value.ToString());
                                return (T)value;
                            }
                            catch (Exception)
                            {
                            }
                            try
                            {
                                var c = new System.Drawing.RectangleConverter();
                                value = c.ConvertFromString(null, new System.Globalization.CultureInfo("en-US"), value.ToString());
                                return (T)value;
                            }
                            catch (Exception)
                            {
                                return (T)mydefault;
                            }
                        }
                    }
                    if (typeof(T) == typeof(TimeSpan) && value != null)
                    {
                        TimeSpan ts = TimeSpan.Zero;
                        if (TimeSpan.TryParse(value.ToString(), out ts))
                        {
                            return (T)(object)ts;
                        }
                    }
                    if (typeof(T) == typeof(string[]) && value != null)
                    {
                        object o = null;
                        if (value.GetType() == typeof(string[])) o = value;
                        if (value.GetType() == typeof(Newtonsoft.Json.Linq.JArray)) o = ((Newtonsoft.Json.Linq.JArray)value).ToObject<string[]>();
                        return (T)o;
                    }
                    return (T)value;
                }
                SetProperty(pluginname, mydefault, propertyName);
                return mydefault;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
        public bool SetProperty<T>(string pluginname, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }
                if (string.IsNullOrEmpty(pluginname))
                {
                    if (settings == null) settings = new Dictionary<string, object>();
                    settings[propertyName] = newValue;
                }
                else
                {
                    if (properties == null) properties = new Dictionary<string, object>();
                    properties[pluginname + "_" + propertyName] = newValue;
                    properties = properties;
                }
                Type typeParameterType = typeof(T);
                if (typeParameterType.Name.ToLower().Contains("readonly"))
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
        private bool IsEqual<T>(T field, T newValue)
        {
            // Alternative: EqualityComparer<T>.Default.Equals(field, newValue);
            return Equals(field, newValue);
        }
        private string GetNameFromExpression<T>(Expression<Func<T>> selectorExpression)
        {
            var body = (MemberExpression)selectorExpression.Body;
            var propertyName = body.Member.Name;
            return propertyName;
        }
    }
}

