using System;
using MahApps.Metro.Controls;
using AddonUpdaterLogic;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace AddonUpdaterGUI
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private AddonUpdater addon_updater;
        private ObservableCollection<Addon> addons = new ObservableCollection<Addon>();

        public MainWindow()
        {
            InitializeComponent();

#if RELEASE
            var cnf_path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.txt");
#else
            var cnf_path = Path.Combine(Environment.CurrentDirectory, "dev_config.txt");
#endif

            addon_updater = new AddonUpdater(cnf_path);
            addon_updater.AddonProgressUpdated += Addon_updater_AddonProgressUpdated;
            grid_addons.ItemsSource = addon_updater.Addons;
        }

        private void Addon_updater_AddonProgressUpdated(object sender, object addon)
        {
            //var a = (Addon)addon;

            //if (a.Progress == AddonProgress.Starting && !addons.Contains(a)) { addons.Add(a); }
            //else
            //{
            //    var old = addons.FirstOrDefault(x => x.Guid == a.Guid);
            //    var old_index = addons.IndexOf(old);
            //    addons[old_index] = a;
            //}
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //addon_updater.UpdateAll().Wait();
            await Task.Run(async () => await addon_updater.UpdateAll());
        }
    }
}