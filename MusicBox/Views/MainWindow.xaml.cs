using System.ComponentModel;
using System.Windows;
using MusicBox.ViewModels;

namespace MusicBox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowVm();
            Closing += vm.Closing;
            KeyDown += (s,e) => { vm.KeyDown(s, e); };
            vm.Close = Close;
            DataContext = vm;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
