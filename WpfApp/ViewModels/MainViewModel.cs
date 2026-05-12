using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public PessoaViewModel PessoaVM { get; private set; }
        public ProdutoViewModel ProdutoVM { get; private set; }
        public PedidoViewModel PedidoVM { get; private set; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (SetProperty(ref _selectedTabIndex, value) && value == 0 && PessoaVM != null)
                    PessoaVM.RecarregarTela();
            }
        }

        public MainViewModel()
        {
            var pessoaService = new PessoaService();
            var produtoService = new ProdutoService();
            var pedidoService = new PedidoService();

            PedidoVM = new PedidoViewModel(pessoaService, produtoService, pedidoService);
            ProdutoVM = new ProdutoViewModel(produtoService);
            PessoaVM = new PessoaViewModel(pessoaService, pedidoService, this);
        }

        public void IrParaPedidosComPessoa(Models.Pessoa pessoa)
        {
            PedidoVM.IniciarNovoPedidoPara(pessoa);
            SelectedTabIndex = 2;
        }
    }
}
