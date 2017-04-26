using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComicChecker
{
    internal class Program
    {
        private static TargetSiteViewModel _targetSiteViewModel;

        private static void Main(string[] args)
        {
            //_targetSiteViewModel = new TargetSiteViewModel();
            //_targetSiteViewModel.ReadFromSimpleFile(
            //    @"E:\Google Drive\Documents\Programming\Macros\ComicChecker2\DailyWebsites.txt");
            _targetSiteViewModel = TargetSiteViewModel.InitializeFromFile();
            _targetSiteViewModel.CleanEmptySites();
            Task.Run(async () =>
            {
                await _targetSiteViewModel.Download();
            }).Wait();
            Console.WriteLine("New:");
            foreach (var cur in _targetSiteViewModel.Sites.Where(x=>x.IsNew))
            {
                Console.WriteLine(cur.SiteURL);
            }
            Task.Run(async () =>
            {
                await _targetSiteViewModel.RunAllTheNewSites();
            }).Wait();
            _targetSiteViewModel.SaveToFile(System.IO.Path.GetFullPath(TargetSiteViewModel.DefaultFileName));

        }
    }
}

