using System.Windows;
using WpfApp.ViewModels;

namespace WpfApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var main = DataContext as MainViewModel;
            if (main != null && main.PedidoVM != null)
                main.PedidoVM.DescartarSeNaoFinalizado();
            base.OnClosing(e);
        }
    }
}
