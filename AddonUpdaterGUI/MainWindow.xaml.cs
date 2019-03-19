using System;
using MahApps.Metro.Controls;
using AddonUpdaterLogic;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace AddonUpdaterGUI
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private AddonUpdater addon_updater;
        private ObservableCollection<AddonObj> addons = new ObservableCollection<AddonObj>();

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _btn_enabled { get; set; } = true;
        public bool btn_enabled { get { return _btn_enabled; } set { _btn_enabled = value; OnPropertyChanged(nameof(btn_enabled)); } }

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

            addon_updater.Addons.ForEach(x => addons.Add(new AddonObj(x.AddonName, x.ProgressVerbose, x.Guid)));
            grid_addons.ItemsSource = addons;
        }

        private void Addon_updater_AddonProgressUpdated(object sender, object addon)
        {
            var a = (Addon)addon;
            var old = addons.FirstOrDefault(x => x.Guid == (a.Guid));
            old.Update(a.AddonName, a.ProgressVerbose);
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            btn_enabled = false;
            await Task.Run(() => addon_updater.UpdateAll());
            btn_enabled = true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class AddonObj : INotifyPropertyChanged
    {
        private string _name;
        private string _progress;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Progress { get { return _progress; } set { _progress = value; OnPropertyChanged(nameof(Progress)); } }
        public Guid Guid { get; set; }

        public AddonObj(string name, string progress, Guid guid)
        {
            Guid = guid;
            Update(name, progress);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Update(string name, string progress)
        {
            Name = name;
            Progress = progress;
        }
    }
}