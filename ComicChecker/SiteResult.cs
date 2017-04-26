using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ComicChecker.Annotations;

namespace ComicChecker
{
    [DataContract]
    public class SiteResult : INotifyPropertyChanged

    {
        public enum SiteResultType
        {
            NotDefined,
            Successful,
            ExceptionThrown
        }

        public String SiteURL;
        private string _downloadComparison = "";

        public SiteResult(String siteURL)
        {
            SiteURL = siteURL;
        }

        public Exception ExceptionCaught { get; set; }

        public SiteResultType Result { get; set; }

        [DataMember]
        public string DownloadComparison
        {
            get { return _downloadComparison; }
            set
            {
                if (value == _downloadComparison) return;
                _downloadComparison = value;
                OnPropertyChanged();
            }
        }

        public bool CompletedSuccessfully { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}