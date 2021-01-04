using OpenBots.Agent.Core.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenBots.Agent.Client.Forms
{
    /// <summary>
    /// Interaction logic for NugetFeedManager.xaml
    /// </summary>
    public partial class NugetFeedManager : Window
    {
        public NugetFeedManager(List<NugetPackageSource> packageSources)
        {
            InitializeComponent();
            dtGrd_NugetSources.ItemsSource = packageSources;
        }

        public List<NugetPackageSource> GetPackageSourcesData()
        {
            var packageSources = dtGrd_NugetSources.ItemsSource.Cast<NugetPackageSource>().ToList();
            return packageSources;
        }
    }
}
