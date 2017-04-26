using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;

namespace ComicCheckUI.ViewModels
{
    [Export(typeof(ToastDisplayViewModel))]
    class ToastDisplayViewModel : Screen
    {
        private string _toastMessage;
        private DispatcherTimer _textTimer;
        [ImportingConstructor]
        public ToastDisplayViewModel()
        {
            _textTimer = new DispatcherTimer();
            _textTimer.Interval = TimeSpan.FromSeconds(3);
            _textTimer.Tick += ClearText;
        }

        private void ClearText(object sender, EventArgs e)
        {
            _textTimer.Stop();
            ToastMessage = "";

        }

        public String ToastMessage
        {
            get { return _toastMessage; }
            private set
            {
                if (value == _toastMessage) return;
                _toastMessage = value;
                NotifyOfPropertyChange(() => ToastMessage);
            }
        }

        public void DisplayMessage(string text)
        {
            ToastMessage = text;
            _textTimer.Start();


        }
    }

}