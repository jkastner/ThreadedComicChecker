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

    private static readonly IReadOnlyList<string> DefaultComics =
      new List<string>
      {
        "http://xkcd.com/"
      };

    public const int TimeoutInSeconds = 8;

    public TargetSiteViewModel()
    {
      Sites = new ObservableCollection<TargetSite>();
    }

    [DataMember]
    public ObservableCollection<TargetSite> Sites { get; private set; }

    public static TargetSiteViewModel InitializeFromFile(string fileName = "")
    {
      if (string.IsNullOrWhiteSpace(fileName))
      {
        fileName = DefaultFileName;
      }
      var ret = new TargetSiteViewModel();
      if (File.Exists(fileName))
      {
        ret = ReadFromXML<TargetSiteViewModel>(DefaultFileName);
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

    public async Task<FullCheckResult> Download()
    {
      
      if (!CheckForConnection())
      {
        return FullCheckResult.NotConnected;
      }
      var tasks = new List<Task>();
      var results = new List<Tuple<TargetSite, SiteResult>>();
      foreach (var cur in Sites)
      {
        tasks.Add(Task.Run(async () =>
        {
          results.Add(
            new Tuple<TargetSite, SiteResult>(cur, await cur.Download())
          );
        }));
      }
      var timeout = TimeSpan.FromSeconds(TimeoutInSeconds);
      await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(timeout));
      var remaining = new List<TargetSite>(Sites);
      foreach (var cur in results.Where(x => x.Item2.CompletedSuccessfully).ToList())
      {
        cur.Item1.CheckForNew(cur.Item2);
        remaining.Remove(cur.Item1);
      }
      var res = FullCheckResult.ConnectedAllSucceeded;
      foreach (var cur in remaining)
      {
        cur.Cancel();
        res = FullCheckResult.ConnectedButSomeFailed;
      }


      return res;
    }

    private bool CheckForConnection()
    {
      var attemptCount = 4;
      while (attemptCount > 0)
      {
        try
        {
          var connectionTest = new Ping();
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


    public void SaveToFile(string comicdataXml = "")
    {
      if (string.IsNullOrWhiteSpace(comicdataXml))
      {
        comicdataXml = DefaultFileName;
      }
      WriteToXML(this, comicdataXml);
    }

    private static void WriteToXML<T>(T someObject, string fileName)
    {
      var settings = new XmlWriterSettings {Indent = true};
      var ser = new DataContractSerializer(typeof(T), null, int.MaxValue, false, true, null);
      using (var w = XmlWriter.Create(fileName, settings))
      {
        ser.WriteObject(w, someObject);
      }
    }

    private static T ReadFromXML<T>(string p0) where T : class
    {
      using (var reader = new FileStream(p0, FileMode.Open, FileAccess.Read))
      {
        var ser = new DataContractSerializer(typeof(T));
        return ser.ReadObject(reader) as T;
      }
    }

    public void CleanEmptySites()
    {
      var allEmpty = Sites.Where(x => string.IsNullOrWhiteSpace(x.SiteURL)).ToList();
      foreach (var cur in allEmpty)
      {
        Sites.Remove(cur);
      }
    }


    public async Task<bool> RunAllTheNewSites()
    {
      var allNew = Sites.Where(x => x.IsNew).ToList();
      var sb = new StringBuilder();
      var procTasks = new List<Task>();
      var allSuccess = true;
      foreach (var cur in allNew)
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
      if (!string.IsNullOrWhiteSpace(sb.ToString()))
      {
        MessageBox.Show(sb.ToString());
      }
      return allSuccess;
    }
  }
}