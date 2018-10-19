using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ComicChecker;

namespace ComicCheckUI.ViewModels
{
    [Export(typeof (ComicDisplayViewModel))]
    internal class ComicDisplayViewModel : Screen
    {
        private readonly ToastDisplayViewModel _toastDisplayViewModel;
        private string _myText;
        private TargetSiteViewModel _comicInfo;
        private bool _canAddNewComic = true;
        private bool _canResetAll  = true;
        private bool _canCheckComics = true;
        private bool _canRunNew = true;
        private bool _enableDataGrid = true;
        private bool _isRunning;

        [ImportingConstructor]
        public ComicDisplayViewModel(ToastDisplayViewModel toastDisplayViewModel)
        {
            _toastDisplayViewModel = toastDisplayViewModel;
            _comicInfo = TargetSiteViewModel.InitializeFromFile();
            ComicData = _comicInfo.Sites;
        }

        public ObservableCollection<TargetSite> ComicData { get; set; }


        public async void AddNewComic()
        {
            _comicInfo.Sites.Add(new TargetSite(""));
        }

        public async void ResetAll()
        {
            ToggleInteraction(false);
            foreach (var cur in _comicInfo.Sites)
            {
                cur.ResetRequested = true;
            }
            await CheckComics();
            ToggleInteraction(true);
        }

        public async Task<bool> CheckComics()
        {
            ToggleInteraction(false);
            var success = await _comicInfo.Download();
            if (success==FullCheckResult.NotConnected)
            {
                _toastDisplayViewModel.DisplayMessage( "Failed to connect.");
            }
            else
            {
                _toastDisplayViewModel.DisplayMessage("Connected successfully.");
            }
            ToggleInteraction(true);
            return true;
        }

        public async void RunNew()
        {
            ToggleInteraction(false);
            await _comicInfo.RunAllTheNewSites();
            ToggleInteraction(true);
        }

        public bool CanAddNewComic
        {
            get { return _canAddNewComic; }
            set
            {
                if (value == _canAddNewComic) return;
                _canAddNewComic = value;
                NotifyOfPropertyChange(() => CanAddNewComic);
            }
        }

        public bool CanResetAll
        {
            get { return _canResetAll; }
            set
            {
                if (value == _canResetAll) return;
                _canResetAll = value;
                NotifyOfPropertyChange(() => CanResetAll);
            }
        }

        public bool CanCheckComics
        {
            get { return _canCheckComics; }
            set
            {
                if (value == _canCheckComics) return;
                _canCheckComics = value;
                NotifyOfPropertyChange(() => CanCheckComics);
            }
        }

        public bool CanRunNew
        {
            get { return _canRunNew; }
            set
            {
                if (value == _canRunNew) return;
                _canRunNew = value;
                NotifyOfPropertyChange(() => CanRunNew);
            }
        }


        private void ToggleInteraction(bool enable)
        {
            
            CanAddNewComic = enable;
            CanCheckComics = enable;
            CanResetAll = enable;
            CanRunNew = enable;
            EnableDataGrid = enable;

            _isRunning = !enable;
        }

        public bool EnableDataGrid
        {
            get { return _enableDataGrid; }
            set
            {
                if (value == _enableDataGrid) return;
                _enableDataGrid = value;
                NotifyOfPropertyChange(() => EnableDataGrid);
            }
        }

        public void Shutdown()
        {
            if (!_isRunning)
            {
                _comicInfo.SaveToFile();
            }
        }


    }
}
