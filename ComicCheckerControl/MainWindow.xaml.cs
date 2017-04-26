using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using ComicChecker;

namespace ComicCheckerControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TargetSiteViewModel _targetSiteViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _targetSiteViewModel = TargetSiteViewModel.InitializeFromFile();
            CheckedComics_DataGrid.ItemsSource = _targetSiteViewModel.Sites;
            _targetSiteViewModel.CleanEmptySites();
        }

        private void CheckComics_Click(object sender, RoutedEventArgs e)
        {
            CheckComics();
        }

        public void ToggleInteraction(bool enable)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                CheckedComics_DataGrid.IsEnabled = enable;
                AddNewComic_Button.IsEnabled = enable;
                CheckComics_Button.IsEnabled = enable;
                ResetAll_Button.IsEnabled = enable;
                RunNew_Button.IsEnabled = enable;

            }));

        }

        private void CheckComics()
        {
            ToggleInteraction(false);
            Task t = new Task(
                () =>
                {
                    _targetSiteViewModel.Download();
                    ToggleInteraction(true);
                }
            );
            t.Start();

        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _targetSiteViewModel.SaveToFile(System.IO.Path.GetFullPath(TargetSiteViewModel.DefaultFileName));

        }

        private void RunNew_Click(object sender, RoutedEventArgs e)
        {
            _targetSiteViewModel.RunAllTheNewSites();
            
            
        }

        private void AddNewComic_Click(object sender, RoutedEventArgs e)
        {
            _targetSiteViewModel.Sites.Add(new TargetSite(""));
        }

        private void MissingTag_IndicatorClick(object sender, MouseButtonEventArgs e)
        {
            var curSite = ((sender as Rectangle).DataContext as TargetSite);
            RunHTMLAsText(curSite);
            SelectHTMLWindow sw = new SelectHTMLWindow(curSite);
            sw.Show();
        }

        private void RunHTMLAsText(TargetSite curSite)
        {

            var curFile = Directory.GetCurrentDirectory() + "\\HTMLout.txt";
            File.WriteAllText(curFile, curSite.FullSiteContents);
            Process.Start(@"E:\Program Files (x86)\Notepad++\notepad++.exe", curFile);
            Process.Start(curSite.SiteURL);
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cur in _targetSiteViewModel.Sites)
            {
                cur.ResetRequested = true;
            }
            CheckComics();

        }
    }
}
