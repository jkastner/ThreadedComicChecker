using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ComicChecker
{
    [DataContract]
    public class TargetSiteViewModel
    {
        public const string DefaultFileName = "ComicDataConfig.xml";

        private static readonly ReadOnlyCollection<String> DefaultComics = new ReadOnlyCollection<string>(new List<string>
        {
            //Sample population removed for public comic
        });

        [DataMember]
        public ObservableCollection<TargetSite> Sites = new ObservableCollection<TargetSite>();

        public static TargetSiteViewModel InitializeFromFile(String fileName = "")
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = DefaultFileName;
            }
            TargetSiteViewModel ret = new TargetSiteViewModel();
            if (File.Exists(fileName))
            {
                ret = TargetSiteViewModel.ReadFromXML<TargetSiteViewModel>(TargetSiteViewModel.DefaultFileName);
            }
            else
            {
                foreach (var curUrl in DefaultComics)
                {
                    ret.Sites.Add(new TargetSite(curUrl));
                }
            }
            
            return ret;
        }

        public async Task<bool> Download()
        {
            if (!CheckForConnection())
            {
                return false;
            }
            List<Task> tasks = new List<Task>();
            List<Tuple<TargetSite, SiteResult>> results = new List<Tuple<TargetSite, SiteResult>>();
            foreach (TargetSite cur in Sites)
            {
                tasks.Add(Task.Run(async () =>
                {
                    results.Add(
                        new Tuple<TargetSite, SiteResult>(cur, await cur.Download())
                        );
                }));
            }
            var timeout = TimeSpan.FromSeconds(5);
            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(timeout));
            List<TargetSite> remaining = new List<TargetSite>(Sites);
            foreach (var cur in results.Where(x => x.Item2.CompletedSuccessfully).ToList())
            {
                cur.Item1.CheckForNew(cur.Item2);
                remaining.Remove(cur.Item1);
            }
            foreach (var cur in remaining)
            {
                cur.Cancel();
            }


            return true;
        }

        private bool CheckForConnection()
        {
            int attemptCount = 4;
            while (attemptCount > 0)
            {
                try
                {
                    Ping connectionTest = new Ping();
                    //String simpleMessage = "dummyText";
                    //Byte[] encodedMessage = Encoding.ASCII.GetBytes(simpleMessage);
                    var result = connectionTest.Send("www.google.com", 1000);
                    if (result?.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    attemptCount--;
                }
            }
            return false;
        }


        private bool CheckForConnection2()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            foreach (var curInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (curInterface.OperationalStatus != OperationalStatus.Up ||
                    curInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    curInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback
                )
                {
                    continue;
                }
                if (curInterface.Description.ToLower().Contains("virtual") ||
                    curInterface.Name.ToLower().Contains("virtual"))
                {
                    continue;
                }
                if (curInterface.Description.ToLower().Equals("microsoft loopback adapter"))
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        public void SaveToFile(string comicdataXml = "")
        {
            if (String.IsNullOrWhiteSpace(comicdataXml))
            {
                comicdataXml = DefaultFileName;
            }
            WriteToXML(this, comicdataXml);
        }

        private static void WriteToXML<T>(T someObject, String fileName)
        {
            var settings = new XmlWriterSettings {Indent = true};
            var ser = new DataContractSerializer(typeof (T), null, int.MaxValue, false, true, null);
            using (XmlWriter w = XmlWriter.Create(fileName, settings))
                ser.WriteObject(w, someObject);
        }

        private static T ReadFromXML<T>(string p0) where T : class
        {
            using (var reader = new FileStream(p0, FileMode.Open, FileAccess.Read))
            {
                var ser = new DataContractSerializer(typeof (T));
                return ser.ReadObject(reader) as T;
            }
        }

        public void CleanEmptySites()
        {
            List<TargetSite> allEmpty = Sites.Where(x => String.IsNullOrWhiteSpace(x.SiteURL)).ToList();
            foreach (TargetSite cur in allEmpty)
            {
                Sites.Remove(cur);
            }
        }




        public async Task<bool> RunAllTheNewSites()
        {
            List<TargetSite> allNew = Sites.Where(x => x.IsNew).ToList();
            StringBuilder sb = new StringBuilder();
            List<Task> procTasks = new List<Task>();
            bool allSuccess = true;
            foreach (TargetSite cur in allNew)
            {
                procTasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Process.Start(cur.SiteURL);
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("Could not load " + cur + "\nException:\n" + ex.Message);
                        allSuccess = false;
                    }

                    cur.IsNew = false;
                }));
            }
            await Task.WhenAll(procTasks);
            if (!String.IsNullOrWhiteSpace(sb.ToString()))
            {
                MessageBox.Show(sb.ToString());
            }
            return allSuccess;

        }
    }
}