using Newtonsoft.Json.Linq;
using OpenRPA.Interfaces;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenRPA.Views
{
    /// <summary>
    /// Interaction logic for wfToolbox.xaml
    /// </summary>
    public partial class WFToolbox : UserControl
    {
        private JObject customerDisplayNames = null;

        // public ToolboxControl toolbox { get; set; } = null;
        public WFToolbox()
        {
            Log.FunctionIndent("WFToolbox", "WFToolbox");
            InitializeComponent();
            DataContext = this;
            // toolborder.Child = InitializeActivitiesToolbox();
            InitializeActivitiesToolbox();
            Log.FunctionOutdent("WFToolbox", "WFToolbox");
            Instance = this;
        }
        public static WFToolbox Instance = null;

        private void LoadCustomerDisplayNames(IEnumerable<System.Reflection.Assembly> appAssemblies)
        {
            Type uniploreRequireLibsType = null;
            foreach (System.Reflection.Assembly activityLibrary in appAssemblies.Where(p => !p.IsDynamic))
            {
                if (activityLibrary.GetName().Name == "OpenRPA.Script")
                {
                    foreach (var type in activityLibrary.GetExportedTypes())
                    {
                        if (type.Name == "UniploreRequireLibs")
                        {
                            uniploreRequireLibsType = type;
                            break;
                        }
                    }
                }

                if (uniploreRequireLibsType != null)
                {
                    break;
                }
            }

            if (uniploreRequireLibsType != null)
            {
                customerDisplayNames = (JObject)uniploreRequireLibsType.GetMethod("GetCustomerDisplayNameConfig").Invoke(null, new object[] { });
            }
        }

        private string getDisplayName(Type type, string libraryName)
        {
            string displayName = type.Name;
            string[] splitName = displayName.Split('`');
            displayName = splitName[0];
            var displayNameAttribute = type.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;
            if (displayNameAttribute != null) displayName = displayNameAttribute.DisplayName;
            if (splitName.Length > 1) displayName = string.Format("{0}<>", displayName);

            displayName = GetCustomerName(displayName, libraryName);

            return displayName;
        }

        private string GetCustomerName(string originName, string libraryName = null, string partName = "toolbox")
        {
            string displayName = null;
            bool log = false;

            if (customerDisplayNames != null && customerDisplayNames.ContainsKey(partName))
            {
                if (customerDisplayNames.ContainsKey("log"))
                {
                    log = (bool)customerDisplayNames["log"];
                }

                JObject toolbox = (JObject)customerDisplayNames[partName];

                if (libraryName != null && toolbox.ContainsKey(libraryName))
                {
                    JObject names = (JObject)toolbox[libraryName];
                    if (names.ContainsKey(originName))
                    {
                        string customerName = (string)names[originName];
                        if (customerName?.Length > 0)
                        {
                            displayName = customerName;
                        }
                    }
                }

                if (displayName == null && libraryName != "*" && toolbox.ContainsKey("*"))
                {
                    JObject names = (JObject)toolbox["*"];
                    if (names.ContainsKey(originName))
                    {
                        string customerName = (string)names[originName];
                        if (customerName?.Length > 0)
                        {
                            displayName = customerName;
                        }
                    }
                }
            }

            if (displayName == null)
            {
                displayName = originName;
            }

            if (log)
            {
                Log.Information($"libraryName={libraryName}, originName={originName}, displayName={displayName}");
            }


            return displayName;
        }

        public void InitializeActivitiesToolbox()
        {
            Log.FunctionIndent("WFToolbox", "InitializeActivitiesToolbox");
            try
            {
                // var tb = new ToolboxControl();
                // get all loaded assemblies
                IEnumerable<System.Reflection.Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name)
                    .Where(a => a.GetName().Name != "System.ServiceModel.Activities");

                LoadCustomerDisplayNames(appAssemblies);

                // check if assemblies contain activities
                int activitiesCount = 0;
                Type scriptActivitiesType = null;
                foreach (System.Reflection.Assembly activityLibrary in appAssemblies.Where(p => !p.IsDynamic))
                {
                    try
                    {
                        string[] excludeActivities = { "AddValidationError", "AndAlso", "AssertValidation", "CreateBookmarkScope", "DeleteBookmarkScope", "DynamicActivity",
                            "CancellationScope", "CompensableActivity", "Compensate", "Confirm", "GetChildSubtree", "GetParentChain", "GetWorkflowTree", "Add`3",  "And`3", "As`2", "Cast`2",
                        "Cast`2", "ArgumentValue`1", "ArrayItemReference`1", "ArrayItemValue`1", "Assign`1", "Constraint`1","CSharpReference`1", "CSharpValue`1", "DelegateArgumentReference`1",
                            "DelegateArgumentValue`1", "Divide`3", "DynamicActivity`1", "Equal`3", "FieldReference`2", "FieldValue`2", "ForEach`1", "InvokeAction", "InvokeDelegate",
                        "ArgumentReference`1", "VariableReference`1", "VariableValue`1", "VisualBasicReference`1", "VisualBasicValue`1", "InvokeMethod`1",
                        "StateMachineWithInitialStateFactory", "ParallelForEach`1", "ForEachWithBodyFactory`1"
                        };
                        // , "ParallelForEach", "ParallelForEachWithBodyFactory", "ForEachWithBodyFactory"

                        var wfToolboxCategory = new ToolboxCategory(GetCustomerName(activityLibrary.GetName().Name, activityLibrary.GetName().Name));
                        var actvities = from
                                            activityType in activityLibrary.GetExportedTypes()
                                        where
                                            (activityType.IsSubclassOf(typeof(Activity))
                                            || activityType.IsSubclassOf(typeof(NativeActivity))
                                            || activityType.IsSubclassOf(typeof(DynamicActivity))
                                            || activityType.IsSubclassOf(typeof(ActivityWithResult))
                                            || activityType.IsSubclassOf(typeof(AsyncCodeActivity))
                                            || activityType.IsSubclassOf(typeof(CodeActivity))
                                            || activityType.IsSubclassOf(typeof(FlowNode))
                                            || activityType == typeof(State)
                                            || activityType == typeof(FinalState)
                                            || activityType.GetInterfaces().Contains(typeof(IActivityTemplateFactory))
                                            )
                                            && !activityType.Assembly.CodeBase.Contains("Snippets.dll")
                                            && activityType.IsVisible
                                            && activityType.IsPublic
                                            && !activityType.IsNested
                                            && !activityType.IsAbstract
                                            && (activityType.GetConstructor(Type.EmptyTypes) != null)
                                            && !excludeActivities.Contains(activityType.Name)
                                            && !activityType.Name.StartsWith("InvokeAction`")
                                            && !activityType.Name.StartsWith("InvokeFunc`")
                                            && !activityType.Name.StartsWith("Subtract`")
                                            && !activityType.Name.StartsWith("GreaterThan`")
                                            && !activityType.Name.StartsWith("GreaterThanOrEqual`")
                                            && !activityType.Name.StartsWith("LessThan`")
                                            && !activityType.Name.StartsWith("LessThanOrEqual`")
                                            && !activityType.Name.StartsWith("Literal`")
                                            && !activityType.Name.StartsWith("MultidimensionalArrayItemReference`")
                                            && !activityType.Name.StartsWith("Multiply`")
                                            && !activityType.Name.StartsWith("New`")
                                            && !activityType.Name.StartsWith("NewArray`")
                                            && !activityType.Name.StartsWith("Or`")
                                            && !activityType.Name.StartsWith("OrElse")
                                            && !activityType.Name.EndsWith("`2")
                                            && !activityType.Name.EndsWith("`3")
                                            && activityType.Name != "ExcelActivity"
                                            && activityType.Name != "ExcelActivityOf`1"
                                            && !activityType.FullName.EndsWith("Statements.DoWhile")
                                            && !activityType.FullName.EndsWith("Statements.While")
                                        orderby
                                            activityType.Name
                                        select
                                            new ToolboxItemWrapper(activityType, getDisplayName(activityType, activityLibrary.GetName().Name));


                        // , activityType.Name.Replace("`1", "")
                        actvities.ToList().ForEach(wfToolboxCategory.Add);

                        if (wfToolboxCategory.Tools.Count > 0)
                        {
                            tb.Categories.Add(wfToolboxCategory);
                            activitiesCount += wfToolboxCategory.Tools.Count;
                            //if(wfToolboxCategory.CategoryName == "System.Activities")
                            //{
                            //    wfToolboxCategory.Tools.Add(new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<>), "ForEach"));
                            //    wfToolboxCategory.Tools.Add(new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<>), "ParallelForEach"));
                            //}
                        }

                        if (scriptActivitiesType == null && activityLibrary.GetName().Name == "OpenRPA.Script")
                        {
                            foreach (var type in activityLibrary.GetExportedTypes())
                            {
                                if (type.Name == "ScriptActivities")
                                {
                                    scriptActivitiesType = type;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                if (scriptActivitiesType != null)
                {// load uniplore dynamic script activities
                    List<ToolboxCategory> finalList = new List<ToolboxCategory>();
                    List<ToolboxCategory> saCategories = (List<ToolboxCategory>)scriptActivitiesType.GetMethod("LoadScriptActivities").Invoke(null, new object[] { });
                    foreach (var saCategory in saCategories)
                    {
                        if (saCategory.Tools.Count > 0)
                        {
                            finalList.Add(saCategory);
                            activitiesCount += saCategory.Tools.Count;
                        }
                    }

                    tb.Categories.ToList().ForEach(finalList.Add);
                    tb.Categories.Clear();
                    finalList.ForEach(tb.Categories.Add);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show("InitializeActivitiesToolbox: " + ex.Message);
            }
            Log.FunctionOutdent("WFToolbox", "InitializeActivitiesToolbox");
        }
    }
}
