using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ComicChecker.Annotations;

namespace ComicChecker
{
    [DataContract]
    public class TargetSite : INotifyPropertyChanged
    {
        private SiteResult _downloadResult;
        private string _endTag;
        private string _fullSiteContents;
        private bool _isNew;
        private DateTime _lastUpdated;
        private string _siteUrl;
        private string _startTag;
        private bool _tagsMissing;
        private bool _wasNew;
        private CancellationTokenSource _cancelToken;

        public TargetSite(String url)
        {
            SiteURL = url;
            CreateToken(new StreamingContext());
        }

        [OnDeserialized]
        private void CreateToken(StreamingContext c)
        {
            _cancelToken = new CancellationTokenSource();

        }

        [DataMember]
        public string SiteURL
        {
            get { return _siteUrl; }
            set
            {
                if (value == _siteUrl) return;
                _siteUrl = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public String StartTag
        {
            get { return _startTag; }
            set
            {
                if (value == _startTag) return;
                _startTag = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public String EndTag
        {
            get { return _endTag; }
            set
            {
                if (value == _endTag) return;
                _endTag = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public SiteResult DownloadResult
        {
            get { return _downloadResult; }
            set
            {
                if (Equals(value, _downloadResult)) return;
                _downloadResult = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                if (value.Equals(_lastUpdated)) return;
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        public bool WasNew
        {
            get { return _wasNew; }
            set
            {
                if (value.Equals(_wasNew)) return;
                _wasNew = value;
                OnPropertyChanged();
            }
        }

        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (value.Equals(_isNew)) return;
                _isNew = value;
                if (value)
                {
                    WasNew = true;
                }
                OnPropertyChanged();
            }
        }

        public String FullSiteContents
        {
            get { return _fullSiteContents; }
            set
            {
                if (value == _fullSiteContents) return;
                _fullSiteContents = value;
                OnPropertyChanged();
            }
        }

        public bool TagsMissing
        {
            get { return _tagsMissing; }
            set
            {
                if (value.Equals(_tagsMissing)) return;
                _tagsMissing = value;
                OnPropertyChanged();
            }
        }

        public bool ResetRequested { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        public async Task<SiteResult> Download()
        {
            
            TagsMissing = false;
            var newResult = new SiteResult(SiteURL);
            newResult.Result = SiteResult.SiteResultType.NotDefined;
            try
            {

                FullSiteContents = await DownloadPage(SiteURL);
                newResult.Result = SiteResult.SiteResultType.Successful;
                string oneLineContents = FullSiteContents.Replace("\n", "");
                if (!String.IsNullOrWhiteSpace(StartTag) && !String.IsNullOrWhiteSpace(EndTag)
                    && oneLineContents.Contains(StartTag))
                {
                    if (oneLineContents.Contains(StartTag) && oneLineContents.Contains(EndTag))
                    {
                        List<string> split1 = oneLineContents.Split(new[] {StartTag}, StringSplitOptions.None).ToList();
                        var matchesEnd = split1.Where(x => x.Contains(EndTag));
                        if (matchesEnd.Any())
                        {
                            List<Tuple<int, string>> sortedByIndex = new List<Tuple<int, string>>();
                            foreach (var cur in matchesEnd)
                            {
                                sortedByIndex.Add(new Tuple<int, string>(cur.IndexOf(EndTag), cur));
                            }
                            var curMin = int.MaxValue;
                            String matchEnd = "";
                            foreach (var cur in sortedByIndex)
                            {
                                if (cur.Item1 < curMin)
                                {
                                    matchEnd = cur.Item2;
                                    curMin = cur.Item1;
                                }
                            }

                            string[] split2 = matchEnd.Split(new[] {EndTag}, StringSplitOptions.None);
                            if (split2.Length >= 1)
                            {
                                newResult.DownloadComparison = split2[0];
                                TagsMissing = false;
                            }
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(newResult.DownloadComparison))
                {
                    newResult.DownloadComparison = oneLineContents;
                    if (!String.IsNullOrWhiteSpace(StartTag))
                    {
                        TagsMissing = true;
                    }
                }
            }
            catch (Exception e)
            {
                newResult.Result = SiteResult.SiteResultType.ExceptionThrown;
                newResult.ExceptionCaught = e;
            }
            newResult.CompletedSuccessfully = true;
            return newResult;
        }

        

        internal async Task<string> DownloadPage(string url)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(TargetSiteViewModel.TimeoutInSeconds);
                using (var r = await client.GetAsync(new Uri(url), _cancelToken.Token))
                {
                    string result = await r.Content.ReadAsStringAsync();
                    return result;
                }
            }
        }

        internal void CheckForNew(SiteResult newResult)
        {
            if (ResetRequested || DownloadResult == null || DownloadResult.DownloadComparison == null ||
                String.IsNullOrWhiteSpace(DownloadResult.DownloadComparison) ||
                !DownloadResult.DownloadComparison.Equals(newResult.DownloadComparison))
            {
                DownloadResult = newResult;
                IsNew = true;
                if (!ResetRequested)
                {
                    LastUpdated = DateTime.Now;
                }
                ResetRequested = false;
            }
            if (!IsNew)
            {
            }
        }



        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Cancel()
        {
            IsNew = true;
            _cancelToken.Cancel();
        }
    }
}