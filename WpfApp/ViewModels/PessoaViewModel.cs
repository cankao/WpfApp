using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class PessoaViewModel : ViewModelBase
    {
        private readonly PessoaService _pessoaService;
        private readonly PedidoService _pedidoService;
        private readonly MainViewModel _main;

        public ObservableCollection<Pessoa> Pessoas { get; private set; }
        public ObservableCollection<Pedido> PedidosDaPessoa { get; private set; }

        private Pessoa _pessoaSelecionada;
        public Pessoa PessoaSelecionada
        {
            get { return _pessoaSelecionada; }
            set
            {
                if (SetProperty(ref _pessoaSelecionada, value))
                {
                    CopiarParaEdicao(value);
                    CarregarPedidosDaPessoa();
                    EmModoEdicao = false;
                }
            }
        }

        private bool _emModoEdicao;
        public bool EmModoEdicao
        {
            get { return _emModoEdicao; }
            set
            {
                if (SetProperty(ref _emModoEdicao, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private Pessoa _emEdicao;
        public Pessoa EmEdicao
        {
            get { return _emEdicao; }
            set { SetProperty(ref _emEdicao, value); }
        }

        private string _filtroNome;
        public string FiltroNome
        {
            get { return _filtroNome; }
            set { if (SetProperty(ref _filtroNome, value)) Filtrar(); }
        }

        private string _filtroCpf;
        public string FiltroCpf
        {
            get { return _filtroCpf; }
            set { if (SetProperty(ref _filtroCpf, value)) Filtrar(); }
        }

        public System.Collections.Generic.List<FiltroPedidoOpcao> OpcoesFiltroPedido { get; private set; }

        private FiltroPedidoOpcao _filtroPedidoSelecionado;
        public FiltroPedidoOpcao FiltroPedidoSelecionado
        {
            get { return _filtroPedidoSelecionado; }
            set { if (SetProperty(ref _filtroPedidoSelecionado, value)) CarregarPedidosDaPessoa(); }
        }

        public RelayCommand IncluirCommand { get; private set; }
        public RelayCommand EditarCommand { get; private set; }
        public RelayCommand SalvarCommand { get; private set; }
        public RelayCommand ExcluirCommand { get; private set; }
        public RelayCommand IncluirPedidoCommand { get; private set; }
        public RelayCommand MarcarPagoCommand { get; private set; }
        public RelayCommand MarcarEnviadoCommand { get; private set; }
        public RelayCommand MarcarRecebidoCommand { get; private set; }

        public PessoaViewModel(PessoaService pessoaService, PedidoService pedidoService, MainViewModel main)
        {
            _pessoaService = pessoaService;
            _pedidoService = pedidoService;
            _main = main;
            Pessoas = new ObservableCollection<Pessoa>();
            PedidosDaPessoa = new ObservableCollection<Pedido>();
            EmEdicao = new Pessoa();

            OpcoesFiltroPedido = new System.Collections.Generic.List<FiltroPedidoOpcao>
            {
                new FiltroPedidoOpcao("Todos", null),
                new FiltroPedidoOpcao("Pendentes de pagamento", StatusPedido.Pendente),
                new FiltroPedidoOpcao("Pagos", StatusPedido.Pago),
                new FiltroPedidoOpcao("Entregues (recebidos)", StatusPedido.Recebido)
            };
            _filtroPedidoSelecionado = OpcoesFiltroPedido[0];

            CarregarTodas();
            PessoaSelecionada = Pessoas.FirstOrDefault();

            IncluirCommand = new RelayCommand(o => Incluir(), () => !EmModoEdicao);
            EditarCommand = new RelayCommand(o => Editar(), () => !EmModoEdicao && PessoaSelecionada != null);
            SalvarCommand = new RelayCommand(o => Salvar(), () => EmModoEdicao);
            ExcluirCommand = new RelayCommand(o => Excluir(), () => false);
            IncluirPedidoCommand = new RelayCommand(o => IncluirPedido(), () => PessoaSelecionada != null);
            MarcarPagoCommand = new RelayCommand(p => AlternarPago(p as Pedido));
            MarcarEnviadoCommand = new RelayCommand(p => AlterarStatus(p as Pedido, StatusPedido.Enviado));
            MarcarRecebidoCommand = new RelayCommand(p => AlterarStatus(p as Pedido, StatusPedido.Recebido));
        }

        private void CarregarTodas()
        {
            Pessoas.Clear();
            foreach (var p in _pessoaService.GetAll()) Pessoas.Add(p);
        }

        private void Filtrar()
        {
            Pessoas.Clear();
            foreach (var p in _pessoaService.Search(FiltroNome, FiltroCpf)) Pessoas.Add(p);
        }

        private void CopiarParaEdicao(Pessoa origem)
        {
            if (origem == null)
            {
                EmEdicao = new Pessoa();
                return;
            }
            EmEdicao = new Pessoa
            {
                Id = origem.Id,
                Nome = origem.Nome,
                Cpf = origem.Cpf,
                Endereco = origem.Endereco
            };
        }

        private void Incluir()
        {
            PessoaSelecionada = null;
            EmEdicao = new Pessoa();
            EmModoEdicao = true;
        }

        private void Editar()
        {
            CopiarParaEdicao(PessoaSelecionada);
            EmModoEdicao = true;
        }

        private void Salvar()
        {
            try
            {
                var idSalvo = EmEdicao.Id;
                if (idSalvo == 0)
                {
                    _pessoaService.Add(EmEdicao);
                    idSalvo = EmEdicao.Id;
                    MessageBox.Show("Pessoa cadastrada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _pessoaService.Update(EmEdicao);
                    MessageBox.Show("Pessoa atualizada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                CarregarTodas();
                PessoaSelecionada = Pessoas.FirstOrDefault(p => p.Id == idSalvo);
                EmModoEdicao = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Excluir()
        {
            if (PessoaSelecionada == null) return;
            var result = MessageBox.Show(
                "Confirma exclusão de " + PessoaSelecionada.Nome + "?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                _pessoaService.Delete(PessoaSelecionada.Id);
                CarregarTodas();
                EmEdicao = new Pessoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void IncluirPedido()
        {
            if (PessoaSelecionada == null) return;
            _main.IrParaPedidosComPessoa(PessoaSelecionada);
        }

        private void CarregarPedidosDaPessoa()
        {
            PedidosDaPessoa.Clear();
            if (PessoaSelecionada == null) return;
            var pedidos = _pedidoService.GetByPessoa(PessoaSelecionada.Id).AsEnumerable();
            if (FiltroPedidoSelecionado != null && FiltroPedidoSelecionado.Status.HasValue)
            {
                var status = FiltroPedidoSelecionado.Status.Value;
                pedidos = pedidos.Where(p => p.Status == status);
            }
            foreach (var ped in pedidos) PedidosDaPessoa.Add(ped);
        }

        private void AlterarStatus(Pedido pedido, StatusPedido novoStatus)
        {
            if (pedido == null) return;
            try
            {
                _pedidoService.AtualizarStatus(pedido.Id, novoStatus);
                pedido.Status = novoStatus;
                CarregarPedidosDaPessoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AlternarPago(Pedido pedido)
        {
            if (pedido == null) return;
            try
            {
                if (pedido.DataPagamento.HasValue)
                {
                    _pedidoService.DesfazerPagamento(pedido.Id);
                    pedido.Status = StatusPedido.Pendente;
                    pedido.DataPagamento = null;
                }
                else
                {
                    var dt = _pedidoService.MarcarComoPago(pedido.Id);
                    pedido.Status = StatusPedido.Pago;
                    pedido.DataPagamento = dt;
                }
                CarregarPedidosDaPessoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RecarregarPedidos()
        {
            CarregarPedidosDaPessoa();
        }
    }

    public class FiltroPedidoOpcao
    {
        public string Texto { get; private set; }
        public StatusPedido? Status { get; private set; }

        public FiltroPedidoOpcao(string texto, StatusPedido? status)
        {
            Texto = texto;
            Status = status;
        }

        public override string ToString() { return Texto; }
    }
}
