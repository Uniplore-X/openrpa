﻿using OpenRPA.Interfaces.Selector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Interfaces
{
    public class Plugins
    {
        public static ObservableCollection<IRecordPlugin> recordPlugins = new ObservableCollection<IRecordPlugin>();
        public static ObservableCollection<IDetectorPlugin> detectorPlugins = new ObservableCollection<IDetectorPlugin>();
        public static Dictionary<string, Type> detectorPluginTypes = new Dictionary<string, Type>();
        public static ObservableCollection<IRunPlugin> runPlugins = new ObservableCollection<IRunPlugin>();
        public static ObservableCollection<ISnippet> Snippets = new ObservableCollection<ISnippet>();
        public static ObservableCollection<IStorage> Storages = new ObservableCollection<IStorage>();
        public static ICollection<Type> WorkflowExtensionsTypes = new List<Type>();
        public static IDetectorPlugin AddDetector(IOpenRPAClient client, IDetector entity)
        {
            foreach(var d in detectorPluginTypes)
            {
                if(d.Key == entity.Plugin)
                {
                    try
                    {
                        IDetectorPlugin plugin = (IDetectorPlugin)Activator.CreateInstance(d.Value);
                        if(string.IsNullOrEmpty(entity.name)) entity.name = plugin.Name;
                        plugin.Initialize(client, entity);
                        IDetectorPlugin exists = Plugins.detectorPlugins.Where(x => x.Entity._id == entity._id).FirstOrDefault();
                        if(exists == null) Plugins.detectorPlugins.Add(plugin);
                        return plugin;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("OpenRPA.Interfaces.Plugins.AddDetector: " + ex.ToString());
                    }
                }
            }
            return null;
        }
        public static IDetectorPlugin UpdateDetector(IOpenRPAClient client, IDetector entity)
        {
            foreach (var d in detectorPluginTypes)
            {
                if (d.Key == entity.Plugin)
                {
                    IDetectorPlugin plugin = Plugins.detectorPlugins.Where(x => x.Entity._id == entity._id).FirstOrDefault();
                    if (plugin == null) return AddDetector(client, entity);
                    try
                    {
                        plugin.Stop();
                        plugin.Entity = entity;
                        if (string.IsNullOrEmpty(entity.name)) entity.name = plugin.Name;
                        plugin.Start();
                        return plugin;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("OpenRPA.Interfaces.Plugins.AddDetector: " + ex.ToString());
                    }
                }
            }
            return null;
        }
        public static void LoadPlugins(IOpenRPAClient client)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            IEnumerable<System.Reflection.Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name);
            ICollection<Type> alltypes = new List<Type>();
            ICollection<Type> pluginTypes = new List<Type>();
            ICollection<Type> snippetTypes = new List<Type>();
            ICollection<Type> runPluginTypes = new List<Type>();
            ICollection<Type> IDetectorPluginTypes = new List<Type>();
            ICollection<Type> StorageTypes = new List<Type>();
            Log.Information("Begin loading plugins");
            Log.Debug("LoadPlugins::Get types " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            foreach (var a in assemblies)
            {
                try
                {
                    foreach (var s in a.GetTypes())
                    {
                        alltypes.Add(s);
                    }
                }
                catch (Exception) { }
            }

            Log.Debug("LoadPlugins::Get all IRecordPlugins " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var IRecordPlugintype = typeof(IRecordPlugin);
            foreach (var p in alltypes)
            {
                try
                {
                    if (IRecordPlugintype.IsAssignableFrom(p) && p.IsInterface == false) pluginTypes.Add(p);
                }
                catch (Exception) { }
            }
            Log.Debug("LoadPlugins::Get all IDetectorPlugin " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var IDetectorPlugintype = typeof(IDetectorPlugin);
            foreach (var p in alltypes)
            {
                try
                {
                    if (IDetectorPlugintype.IsAssignableFrom(p) && p.IsInterface == false) IDetectorPluginTypes.Add(p);
                }
                catch (Exception) { }
            }

            Log.Debug("LoadPlugins::Get all ISnippet " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var ISnippettype = typeof(ISnippet);
            foreach (var p in alltypes)
            {
                try
                {
                    if (ISnippettype.IsAssignableFrom(p) && p.IsInterface == false) snippetTypes.Add(p);
                }
                catch (Exception) { }
            }

            Log.Debug("LoadPlugins::Get all IRunPlugin " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var IRunPlugintype = typeof(IRunPlugin);
            foreach (var p in alltypes)
            {
                try
                {
                    if (IRunPlugintype.IsAssignableFrom(p) && p.IsInterface == false) runPluginTypes.Add(p);
                }
                catch (Exception) { }
            }
            Log.Debug("LoadPlugins::Get all ICustomWorkflowExtension " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var WorkflowExtensiontype = typeof(ICustomWorkflowExtension);
            foreach (var p in alltypes)
            {
                try
                {
                    if (WorkflowExtensiontype.IsAssignableFrom(p) && p.IsInterface == false)
                    {
                        if(!WorkflowExtensionsTypes.Contains(p)) WorkflowExtensionsTypes.Add(p);
                    }
                }
                catch (Exception) { }
            }

            Log.Debug("LoadPlugins::Get all IStorage " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            var IStoragetype  = typeof(IStorage);
            foreach (var p in alltypes)
            {
                try
                {
                    if (IStoragetype.IsAssignableFrom(p) && p.IsInterface == false) StorageTypes.Add(p);
                }
                catch (Exception) { }
            }

            foreach (var type in IDetectorPluginTypes)
                if (!detectorPluginTypes.ContainsKey(type.FullName)) detectorPluginTypes.Add(type.FullName, type);
            foreach (Type type in pluginTypes)
            {
                try
                {
                    IRecordPlugin plugin = null;
                    foreach (var p in recordPlugins)
                    {
                        if(p.GetType() == type)
                        {
                            plugin = p;
                            break;
                        }
                    }                    
                    if(plugin== null)
                    {
                        plugin = (IRecordPlugin)Activator.CreateInstance(type);
                        Log.Debug("LoadPlugins::Initialize plugin " + plugin.Name + " " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
                        // SetStatus("Initialize plugin " + plugin.Name);
                        plugin.Initialize(client);
                        GenericTools.RunUI(() => recordPlugins.Add(plugin), 10000);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            foreach (Type type in snippetTypes)
            {
                try
                {
                    ISnippet plugin = null;
                    foreach (var p in Snippets)
                    {
                        if (p.GetType() == type)
                        {
                            plugin = p;
                            break;
                        }
                    }
                    if(plugin == null)
                    {
                        plugin = (ISnippet)Activator.CreateInstance(type);
                        Log.Debug("LoadPlugins::Initialize snippet " + plugin.Name + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
                        Snippets.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            foreach (Type type in StorageTypes)
            {
                try
                {
                    IStorage plugin = null;
                    foreach (var p in Storages)
                    {
                        if (p.GetType() == type)
                        {
                            plugin = p;
                            break;
                        }
                    }
                    if (plugin == null)
                    {
                        plugin = (IStorage)Activator.CreateInstance(type);
                        Log.Debug("LoadPlugins::Initialize storage " + plugin.Name + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
                        plugin.Initialize();
                        Storages.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            foreach (Type type in runPluginTypes)
            {
                try
                {
                    IRunPlugin plugin = null;
                    foreach (var p in runPlugins)
                    {
                        if (p.GetType() == type)
                        {
                            plugin = p;
                            break;
                        }
                    }

                    if (plugin == null)
                    {
                        plugin = (IRunPlugin)Activator.CreateInstance(type);
                        Log.Debug("LoadPlugins::Initialize RunPlugin " + plugin.Name + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
                        plugin.Initialize(client);
                        GenericTools.RunUI(() => runPlugins.Add(plugin), 10000);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            Log.Debug("LoadPlugins::end " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
            Log.Information("Load plugins ended after " + string.Format("{0:mm\\:ss\\.fff}", sw.Elapsed));
        }
        public static void LoadPlugins(IOpenRPAClient client, string projectsDirectory, bool recursive)
        {
            Log.Debug("LoadPlugins::begin ");
            List<string> dllFileNames = new List<string>();
            if(recursive)
            {
                foreach (var path in System.IO.Directory.GetFiles(projectsDirectory, "*.dll", System.IO.SearchOption.AllDirectories)) dllFileNames.Add(path);
            } else
            {
                foreach (var path in System.IO.Directory.GetFiles(projectsDirectory, "*.dll")) dllFileNames.Add(path);
            }
            try
            {
                var extensions = System.IO.Path.Combine(Extensions.ProjectsDirectory, "extensions");
                if(System.IO.Directory.Exists(extensions))
                {
                    foreach (var path in System.IO.Directory.GetFiles(extensions, "*.dll")) dllFileNames.Add(path);
                }
            }
            catch (Exception)
            {
            }
            
            // ICollection<Assembly> assemblies = new List<Assembly>();
            foreach (string dllFile in dllFileNames)
            {
                try
                {
                    // if (dllFile.Contains("OpenRPA.Interfaces.")) continue;
                    if (dllFile.Contains("DotNetProjects.")) continue;
                    if (dllFile.Contains("Emgu.")) continue;
                    if (dllFile.Contains("Microsoft.CodeAnalysis.")) continue;
                    if (dllFile.Contains("Microsoft.Office.")) continue;
                    if (dllFile.Contains("NuGet.")) continue;
                    if (dllFile.Contains("System.Collections.")) continue;
                    if (dllFile.Contains("System.ComponentModel.")) continue;
                    if (dllFile.Contains("System.Composition.")) continue;
                    if (dllFile.Contains("System.Data.")) continue;
                    if (dllFile.Contains("System.Diagnostics.")) continue;
                    if (dllFile.Contains("System.Globalization.")) continue;
                    if (dllFile.Contains("System.IO.")) continue;
                    if (dllFile.Contains("System.Linq.")) continue;
                    if (dllFile.Contains("System.Net.")) continue;
                    if (dllFile.Contains("System.Reflection.")) continue;
                    if (dllFile.Contains("System.Resources.")) continue;
                    if (dllFile.Contains("System.Runtime.")) continue;
                    if (dllFile.Contains("System.Security.")) continue;
                    if (dllFile.Contains("System.Text.")) continue;
                    if (dllFile.Contains("System.Threading.")) continue;
                    if (dllFile.Contains("System.Xml.")) continue;
                    if (dllFile.Contains("System.Windows.")) continue;
                    if (dllFile.Contains("ToastNotifications.")) continue;
                    if (dllFile.Contains("Xceed.Wpf.")) continue;
                    if (dllFile.Contains("ControlzEx.")) continue;
                    if (dllFile.Contains("MahApps.")) continue;
                    if (dllFile.Contains("Snippets.")) continue;
                    if (dllFile.Contains("Interop.SAPFEWSELib")) continue;
                    if (dllFile.Contains("Interop.SapROTWr")) continue;
                    if (dllFile.Contains("grpc_csharp_ext")) continue;
                    if (dllFile.Contains("chrome_elf.")) continue;
                    if (dllFile.Contains("d3dcompiler_")) continue;
                    if (dllFile.EndsWith("libcef.dll")) continue;
                    if (dllFile.EndsWith("libEGL.dll")) continue;
                    if (dllFile.EndsWith("libGLESv2.dll")) continue;
                    if (dllFile.EndsWith("WindowsAccessBridge-32.dll")) continue;
                    if (dllFile.EndsWith("WindowsAccessBridge-64.dll")) continue;
                    if (dllFile.EndsWith("WindowsAccessBridge.dll")) continue;
                    if (dllFile.EndsWith("vk_swiftshader.dll")) continue;
                    if (dllFile.EndsWith("vulkan-1.dll")) continue;
                    if (dllFile.EndsWith("cvextern.dll")) continue;
                    if (dllFile.EndsWith("opencv_videoio_ffmpeg455_64.dll")) continue;
                    if (dllFile.EndsWith("BouncyCastle.Crypto.dll")) continue;
                    if (dllFile.EndsWith("MailKit.dll")) continue;
                    if (dllFile.EndsWith("MimeKit.dll")) continue;
                    
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    // assemblies.Add(assembly);
                }
                catch (System.BadImageFormatException)
                {
                    // don't care
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            LoadPlugins(client);
        }
    }
}
