using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ComicCheckUI.ViewModels
{
    [Export(typeof(ShellViewModel))]
    internal class ShellViewModel : Screen
    {
        [ImportingConstructor]
        public ShellViewModel()
        {
            ComicDisplay = IoC.Get<ComicDisplayViewModel>();
            ToastDisplay = IoC.Get<ToastDisplayViewModel>();
            DisplayName = "Comic Checker";
        }


        public ComicDisplayViewModel ComicDisplay { get; private set; }

        public ToastDisplayViewModel ToastDisplay { get; private set; }

        protected override void OnDeactivate(bool close)
        {
            ComicDisplay.Shutdown();
        }

        public override void CanClose(Action<bool> allowClose)
        {
            allowClose(true);
        }
    }
}
