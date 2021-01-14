using OpenBots.Agent.Core.Model;
using OpenBots.Core.Enums;
using OpenBots.Core.IO;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenBots.Agent.Client.Forms
{
    /// <summary>
    /// Interaction logic for AttendedExecution.xaml
    /// </summary>
    public partial class AttendedExecution : Window
    {
        private string _lastTask;
        private bool _isEngineBusy;
        private string[] _publishedProjects;
        private FileSystemWatcher _publishedProjectsWatcher;
        private ServerConnectionSettings _connectionSettings;

        public AttendedExecution(bool isEngineBusy)
        {
            InitializeComponent();
            _publishedProjectsWatcher = new FileSystemWatcher();
            _isEngineBusy = isEngineBusy;

            _connectionSettings = new ServerConnectionSettings()
            {
                SinkType = SinkType.File.ToString(),
                TracingLevel = LogEventLevel.Information.ToString(),
                LoggingValue1 = Path.Combine(new EnvironmentSettings().GetEnvironmentVariable(), "Logs", "Attended Execution", "log.txt"),
                DNSHost = Dns.GetHostName(),
                UserName = Environment.UserName
            };
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            var publishedProjectsDir = Folders.GetFolder(FolderType.PublishedFolder);

            _publishedProjectsWatcher.Path = publishedProjectsDir;
            _publishedProjectsWatcher.Filter = "*.nupkg";
            _publishedProjectsWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _publishedProjectsWatcher.Created += new FileSystemEventHandler(OnFileCreated);
            _publishedProjectsWatcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
            _publishedProjectsWatcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            _publishedProjectsWatcher.EnableRaisingEvents = true;

            lbl_ExecutionStatus.Visibility = Visibility.Collapsed;
            LoadPublishedProjects();
            OpenUpInBottomRight();
        }
        private void OpenUpInBottomRight()
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            LoadPublishedProjects();
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            LoadPublishedProjects();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            LoadPublishedProjects();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            LoadPublishedProjects();
        }

        private async void OnClick_RunBtn(object sender, RoutedEventArgs e)
        {
            _lastTask = cmb_PublishedProjects.SelectedItem.ToString();
            var selectedProjectPath = _publishedProjects.Where(x => x.EndsWith(_lastTask)).FirstOrDefault();
            PipeProxy.Instance.TaskFinishedEvent += OnAttendedTaskFinished;
            await Task.Run(()=>PipeProxy.Instance.ExecuteAttendedTask(selectedProjectPath, _connectionSettings));

            string executionStatus = "Running {0} . . .";
            lbl_ExecutionStatus.Content = string.Format(executionStatus, $"\"{_lastTask}\"");
            lbl_ExecutionStatus.Visibility = Visibility.Visible;
        }

        private void OnAttendedTaskFinished(object sender, bool isJobSuccessful)
        {
            Dispatcher.Invoke(() => 
            {
                string lastRunStatus = "Last Run: {0} - Status: {1}";
                if (isJobSuccessful)
                    lbl_ExecutionStatus.Content = string.Format(lastRunStatus, _lastTask, "Successful");
                else
                    lbl_ExecutionStatus.Content = string.Format(lastRunStatus, _lastTask, "Failed");
            });
        }

        private void LoadPublishedProjects()
        {
            Dispatcher.Invoke(() =>
            {
                cmb_PublishedProjects.ItemsSource = Enumerable.Empty<string>();

                var publishedProjectsDir = Folders.GetFolder(FolderType.PublishedFolder);
                _publishedProjects = Directory.GetFiles(publishedProjectsDir, "*.nupkg");
                var projectNames = from fileName in _publishedProjects select Path.GetFileName(fileName);

                cmb_PublishedProjects.ItemsSource = projectNames;
                cmb_PublishedProjects.SelectedIndex = 0;
            });
        }
    }
}
