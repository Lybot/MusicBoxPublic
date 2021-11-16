using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Behaviors.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicBox.ViewModels
{
    class SettingsTabVm:BindableBase
    {
        public DelegateCommand Start { get; set; }
        public Action CloseWindow;
        public static event EventHandler<string> CloseSettings;
        public SettingsTabVm()
        {

            Start = new DelegateCommand(delegate
            {
                CloseSettings?.Invoke("kek","kekw");
                new MainWindow().Show();
                CloseWindow?.Invoke();
            });
        }
    }
}
