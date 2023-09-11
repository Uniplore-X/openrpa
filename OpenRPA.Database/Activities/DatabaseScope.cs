﻿using OpenRPA.Interfaces;
using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Database
{
    [Designer(typeof(DatabaseScopeDesigner), typeof(System.ComponentModel.Design.IDesigner))]
    [System.Drawing.ToolboxBitmap(typeof(DatabaseScope), "Resources.toolbox.databasescope.png")]
    [System.Windows.Markup.ContentProperty("Body")]
    [LocalizedToolboxTooltip("activity_databasescope_tooltip", typeof(Resources.strings))]
    [LocalizedDisplayName("activity_databasescope", typeof(Resources.strings))]
    [LocalizedHelpURL("activity_databasescope_helpurl", typeof(Resources.strings))]
    public class DatabaseScope : BreakableLoop, System.Activities.Presentation.IActivityTemplateFactory
    {
        [Browsable(false)]
        public ActivityAction<Connection> Body { get; set; }
        [Browsable(false)]
        public InArgument<TimeSpan> Timeout { get; set; }
        [RequiredArgument]
        public InArgument<string> DataProvider { get; set; }
        [RequiredArgument]
        public InArgument<string> DataSource { get; set; }
        [RequiredArgument]
        public InArgument<string> ConnectionString { get; set; }
        private readonly Variable<Connection> Connection = new Variable<Connection>("Connection");
        protected override void StartLoop(NativeActivityContext context)
        {
            var dataprovider = DataProvider.Get(context);
            var datasource = DataSource.Get(context);
            var connectionstring = ConnectionString.Get(context);
            var timeout = Timeout.Get(context);
            Connection connection = new Connection(dataprovider, datasource, connectionstring);

            connection.Open();
            context.SetValue(Connection, connection);
            IncIndex(context);
            SetTotal(context, 1);
            context.ScheduleAction(Body, connection, OnBodyComplete);
        }
        private void OnBodyComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (breakRequested) return;
            Connection connection = Connection.Get(context);
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(Body);
            Interfaces.Extensions.AddCacheArgument(metadata, "ConnectionString", ConnectionString);
            Interfaces.Extensions.AddCacheArgument(metadata, "Timeout", Timeout);
            metadata.AddImplementationVariable(Connection);
            base.CacheMetadata(metadata);
        }
        public Activity Create(System.Windows.DependencyObject target)
        {
            var fef = new DatabaseScope();
            fef.Variables.Add(new Variable<int>("Index", 0));
            fef.Variables.Add(new Variable<int>("Total", 0));
            var aa = new ActivityAction<Connection>();
            var da = new DelegateInArgument<Connection>();
            da.Name = "conn";
            fef.Body = aa;
            aa.Argument = da;
            return fef;
        }
        [LocalizedDisplayName("activity_displayname", typeof(Resources.strings)), LocalizedDescription("activity_displayname_help", typeof(Resources.strings))]
        public new string DisplayName
        {
            get
            {
                var displayName = base.DisplayName;
                if (displayName == this.GetType().Name)
                {
                    var displayNameAttribute = this.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                    if (displayNameAttribute != null) displayName = displayNameAttribute.DisplayName;
                }
                return displayName;
            }
            set
            {
                base.DisplayName = value;
            }
        }
    }
}