﻿using OpenRPA.Interfaces;
using OpenRPA.Interfaces.entity;
using OpenRPA.Interfaces.Selector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for WorkItemQueuesView.xaml
    /// </summary>
    public partial class WorkItemQueuesView : UserControl, INotifyPropertyChanged
    {
        private MainWindow main = null;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public System.Collections.ObjectModel.ObservableCollection<IWorkitemQueue> WorkItemQueues
        {
            get
            {
                return RobotInstance.instance.WorkItemQueues;
            }
        }
        private System.Collections.ObjectModel.ObservableCollection<Workflow> _Workflows = new System.Collections.ObjectModel.ObservableCollection<Workflow>();
        public System.Collections.ObjectModel.ObservableCollection<Workflow> Workflows
        {
            get
            {
                return _Workflows;
            }
        }
        private System.Collections.ObjectModel.ObservableCollection<IWorkitem> _WorkItems;
        public System.Collections.ObjectModel.ObservableCollection<IWorkitem> WorkItems
        {
            get
            {
                return _WorkItems;
            }
        }
        public System.Collections.ObjectModel.ObservableCollection<IProject> Projects
        {
            get
            {
                return OpenProject.Instance.Projects;
            }
        }
        public System.Collections.ObjectModel.ObservableCollection<apiuser> Robots { get; set; } = new System.Collections.ObjectModel.ObservableCollection<apiuser>();
        public WorkItemQueuesView(MainWindow main)
        {
            Log.FunctionIndent("WorkItemQueuesView", "WorkItemQueuesView");
            _WorkItems = new FilteredObservableCollection<IWorkitem>(RobotInstance.instance.Workitems, wifilter);
            InitializeComponent();
            this.main = main;
            DataContext = this;
            Log.FunctionOutdent("WorkItemQueuesView", "WorkItemQueuesView");
        }
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.FunctionIndent("WorkItemQueuesView", "Button_Click");
            try
            {
                var btn = sender as System.Windows.Controls.Button;
                var kv = (System.Collections.Generic.KeyValuePair<string, System.Type>)btn.DataContext;
                var d = new Detector(); d.Plugin = kv.Value.FullName;
                NotifyPropertyChanged("WorkItemQueues");
                d.name = kv.Value.Name;
                d.detectortype = "exchange";
                if (global.isConnected)
                {
                    var result = await global.webSocketClient.InsertOne("openrpa", 0, false, d, "", "");
                    d._id = result._id;
                    d._acl = result._acl;
                }
                else
                {
                    d._id = Guid.NewGuid().ToString();
                    d.isDirty = true;
                    d.isLocalOnly = true;
                }
                d.Start(true);
                var dexists = RobotInstance.instance.dbDetectors.FindById(d._id);
                if (dexists == null) RobotInstance.instance.dbDetectors.Insert(d);
                if (dexists != null) RobotInstance.instance.dbDetectors.Update(d);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            Log.FunctionOutdent("WorkItemQueuesView", "Button_Click");
        }
        private async void listWorkItemQueues_KeyUp(object sender, KeyEventArgs e)
        {
            Log.FunctionIndent("WorkItemQueuesView", "listWorkItemQueues_KeyUp");
            try
            {
                if (e.Key == Key.Delete)
                {
                    var index = listWorkItemQueues.SelectedIndex;
                    var items = new List<object>();
                    foreach (var item in listWorkItemQueues.SelectedItems) items.Add(item);
                    foreach (var item in items)
                    {
                        var wiq = item as OpenRPA.WorkitemQueue;
                        if (wiq != null)
                        {
                            await global.webSocketClient.DeleteWorkitemQueue(wiq, true, "", "");
                            RobotInstance.instance.dbWorkItemQueues.Delete(wiq._id);
                            WorkItemQueues.Remove(wiq);
                            var p = OpenProject.Instance.Projects.Where(x => x._id == wiq.projectid).FirstOrDefault() as Project;
                            reloadOpenProjects = true;
                        }
                    }
                    if (index > -1)
                    {
                        if (index >= listWorkItemQueues.Items.Count)
                        {
                            index = listWorkItemQueues.Items.Count - 1;
                        }
                        if (index > -1)
                        {
                            ((ListBoxItem)listWorkItemQueues.ItemContainerGenerator.ContainerFromIndex((listWorkItemQueues.Items.Count > 1 ? (index == 0 ? 1 : index - 1) : 0)))?.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show(ex.Message);
            }
            Log.FunctionOutdent("WorkItemQueuesView", "listWorkItemQueues_KeyUp");
        }
        public bool reloadOpenProjects = false;
        public async void LayoutDocument_IsSelectedChanged(object sender, EventArgs e)
        {
            var tab = sender as Xceed.Wpf.AvalonDock.Layout.LayoutContent;
            if (tab != null && !tab.IsSelected)
            {
                try
                {
                    foreach (OpenRPA.WorkitemQueue d in WorkItemQueues.ToList())
                    {
                        if(d.isDirty) await d.Save();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                if(reloadOpenProjects)
                {
                    reloadOpenProjects = false;
                }                
            }
        }
        private void listProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var WorkItemQueue = listWorkItemQueues.SelectedItem as OpenRPA.WorkitemQueue;
            //if (listProjects.IsDropDownOpen) return;
            var Workflows = new List<Workflow>();
            var p = (listProjects.SelectedItem as Project);
            Project oldp = null;
            if(WorkItemQueue!=null) oldp = OpenProject.Instance.Projects.Where(x=> x._id == WorkItemQueue.projectid).FirstOrDefault() as Project;

            if (p == null)
            {
                foreach (Project _p in Projects)
                {
                    foreach (var w in _p.Workflows) Workflows.Add(w as Workflow);
                }
                _Workflows.UpdateCollection(Workflows.ToList());
                reloadOpenProjects = true;
                return;
            }
            if (WorkItemQueue != null)
                if (WorkItemQueue.projectid != p._id)
                {
                    WorkItemQueue._acl = p._acl;
                }
            foreach (var w in p.Workflows) Workflows.Add(w as Workflow);
            _Workflows.UpdateCollection(Workflows.ToList());
            reloadOpenProjects = true;
        }
        async private void listWorkItemQueues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var wiq = listWorkItemQueues.SelectedItem as OpenRPA.WorkitemQueue;
                try
                {
                    if (wiq != null && RobotInstance.instance.Workflows != null)
                        foreach (var wf in RobotInstance.instance.Workflows)
                        {
                            if (wf._id == wiq.workflowid) wiq.workflowid = wf.ProjectAndName;
                            if (wf.RelativeFilename == wiq.workflowid) wiq.workflowid = wf.ProjectAndName;
                        }
                    foreach (OpenRPA.WorkitemQueue d in WorkItemQueues.ToList())
                    {
                        if (d.isDirty) await d.Save();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                if (wiq == null) return;
                if (global.webSocketClient == null || !global.webSocketClient.isConnected)
                {
                    //RobotInstance.instance.Workitems.Clear();
                    //RobotInstance.instance.Workitems.UpdateCollectionById(RobotInstance.instance.dbWorkitems.FindAll().OrderBy(x => x.name));
                    // _WorkItems.Refresh();
                    if (WorkItems != null) WorkItems.Clear();
                    _WorkItems = new FilteredObservableCollection<IWorkitem>(RobotInstance.instance.Workitems, wifilter);

                    return;
                }
                var server_workitems = await global.webSocketClient.Query<Workitem>("workitems", "{\"_type\": 'workitem',\"wiqid\": '" + wiq._id + "'}", "{\"name\": 1,\"state\": 1,\"_modified\": 1}", top: 100);
                if (WorkItems != null) WorkItems.Clear();
                foreach (var workitem in server_workitems)
                {
                    WorkItems.Add(workitem);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public bool wifilter(IWorkitem item)
        {
            if (item == null) return false;
            if (listWorkItemQueues == null) return true;
            var wiq = listWorkItemQueues.SelectedItem as OpenRPA.WorkitemQueue;
            if (wiq == null) return true;

            if (item.wiq == wiq.name || item.wiqid == wiq._id) return true;
            return false;
        }

        async private void Button_CreateWorkItemQueue(object sender, RoutedEventArgs e)
        {
            Log.Warning("Due to inconsistencies in how work item queues are managed in OpenFlow and OpenRPA, saving work item queues is disabled for now.");
            return;
            try
            {
                var dia = new OpenRPA.Views.WorkitemQueue();
                dia.item = new OpenRPA.WorkitemQueue();
                dia.ShowDialog();
                if (dia.DialogResult == true)
                {
                    await dia.item.Save(true);
                    reloadOpenProjects = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }
        async private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Robots.Count() > 0) return;
                if (global.webSocketClient == null || !global.webSocketClient.isConnected) return;
                var _users = await global.webSocketClient.Query<apiuser>("users", "{\"$or\":[ {\"_type\": \"user\"}, {\"_type\": \"role\", \"rparole\": true} ]}", top: 5000);
                // foreach (var u in _users) robots.Add(u);
                Robots.UpdateCollection(_users.ToList());
                NotifyPropertyChanged("Robots");
                //GenericTools.RunUI(() =>
                //{
                //});
                
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

        }

        private async void PurgeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var wiq = listWorkItemQueues.SelectedItem as OpenRPA.WorkitemQueue;
                if (wiq == null) return;
                var messageBoxResult = MessageBox.Show("Purge all workitems from " + wiq.name + " ?\n This action cannot be undone!!!", "Purge Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult != MessageBoxResult.Yes) return;
                await wiq.Purge();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
