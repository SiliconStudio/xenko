// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using SiliconStudio.BuildEngine.Monitor.Service;
using SiliconStudio.BuildEngine.Monitor.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private BuildMonitorRemote buildMonitorRemote;
        public ObservableCollection<BuildSessionViewModel> BuildSessions { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            BuildSessions = new ObservableCollection<BuildSessionViewModel>();
            DataContext = BuildSessions;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializePipe();
        }

        private void InitializePipe()
        {
            string pipeName = Program.MonitorPipeName;
            buildMonitorRemote = new BuildMonitorRemote(BuildSessions);
            var host = new ServiceHost(buildMonitorRemote);
            host.AddServiceEndpoint(typeof(IBuildMonitorRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, pipeName);
            host.Open();
        }
    }
}
