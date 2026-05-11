using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class PedidoViewModel : ViewModelBase
    {
        private readonly PessoaService _pessoaService;
        private readonly ProdutoService _produtoService;
        private readonly PedidoService _pedidoService;

        public ObservableCollection<Pessoa> Pessoas { get; private set; }
        public ObservableCollection<Produto> Produtos { get; private set; }
        public ObservableCollection<Pedido> Pedidos { get; private set; }

        public Array FormasPagamento { get { return Enum.GetValues(typeof(FormaPagamento)); } }

        private Pedido _pedidoAtual;
        public Pedido PedidoAtual
        {
            get { return _pedidoAtual; }
            set { SetProperty(ref _pedidoAtual, value); }
        }

        private Pessoa _pessoaSelecionada;
        public Pessoa PessoaSelecionada
        {
            get { return _pessoaSelecionada; }
            set
            {
                if (SetProperty(ref _pessoaSelecionada, value) && PedidoAtual != null && !Finalizado)
                {
                    PedidoAtual.PessoaId = value != null ? value.Id : 0;
                    PedidoAtual.PessoaNome = value != null ? value.Nome : null;
                }
            }
        }

        private Produto _produtoSelecionado;
        public Produto ProdutoSelecionado
        {
            get { return _produtoSelecionado; }
            set { SetProperty(ref _produtoSelecionado, value); }
        }

        private int _quantidadeAdicionar = 1;
        public int QuantidadeAdicionar
        {
            get { return _quantidadeAdicionar; }
            set { SetProperty(ref _quantidadeAdicionar, value); }
        }

        private bool _finalizado;
        public bool Finalizado
        {
            get { return _finalizado; }
            set
            {
                if (SetProperty(ref _finalizado, value))
                {
                    OnPropertyChanged("PodeEditar");
                }
            }
        }

        public bool PodeEditar { get { return !Finalizado; } }

        public RelayCommand AdicionarProdutoCommand { get; private set; }
        public RelayCommand RemoverItemCommand { get; private set; }
        public RelayCommand FinalizarCommand { get; private set; }
        public RelayCommand NovoPedidoCommand { get; private set; }

        public PedidoViewModel(PessoaService pessoaService, ProdutoService produtoService, PedidoService pedidoService)
        {
            _pessoaService = pessoaService;
            _produtoService = produtoService;
            _pedidoService = pedidoService;

            Pessoas = new ObservableCollection<Pessoa>();
            Produtos = new ObservableCollection<Produto>();
            Pedidos = new ObservableCollection<Pedido>();

            CarregarListas();
            IniciarNovoPedido();

            AdicionarProdutoCommand = new RelayCommand(
                o => AdicionarProduto(),
                () => PodeEditar && ProdutoSelecionado != null && QuantidadeAdicionar > 0);
            RemoverItemCommand = new RelayCommand(
                i => RemoverItem(i as ItemPedido),
                () => PodeEditar);
            FinalizarCommand = new RelayCommand(
                o => Finalizar(),
                () => PodeEditar);
            NovoPedidoCommand = new RelayCommand(o => IniciarNovoPedido());
        }

        public void RecarregarDados()
        {
            CarregarListas();
        }

        private void CarregarListas()
        {
            Pessoas.Clear();
            foreach (var p in _pessoaService.GetAll()) Pessoas.Add(p);
            Produtos.Clear();
            foreach (var p in _produtoService.GetAll()) Produtos.Add(p);
            Pedidos.Clear();
            foreach (var p in _pedidoService.GetAll()) Pedidos.Add(p);
        }

        public void IniciarNovoPedido()
        {
            CarregarListas();
            PedidoAtual = new Pedido();
            PessoaSelecionada = null;
            ProdutoSelecionado = null;
            QuantidadeAdicionar = 1;
            Finalizado = false;
        }

        public void IniciarNovoPedidoPara(Pessoa pessoa)
        {
            IniciarNovoPedido();
            if (pessoa == null) return;
            var match = Pessoas.FirstOrDefault(p => p.Id == pessoa.Id);
            PessoaSelecionada = match;
        }

        private void AdicionarProduto()
        {
            if (ProdutoSelecionado == null || QuantidadeAdicionar <= 0) return;
            var existente = PedidoAtual.Itens.FirstOrDefault(i => i.ProdutoId == ProdutoSelecionado.Id);
            if (existente != null)
            {
                existente.Quantidade += QuantidadeAdicionar;
            }
            else
            {
                PedidoAtual.Itens.Add(new ItemPedido
                {
                    ProdutoId = ProdutoSelecionado.Id,
                    ProdutoNome = ProdutoSelecionado.Nome,
                    ProdutoCodigo = ProdutoSelecionado.Codigo,
                    ValorUnitario = ProdutoSelecionado.Valor,
                    Quantidade = QuantidadeAdicionar
                });
            }
            PedidoAtual.RecalcularTotal();
            QuantidadeAdicionar = 1;
        }

        private void RemoverItem(ItemPedido item)
        {
            if (item == null || PedidoAtual == null) return;
            PedidoAtual.Itens.Remove(item);
            PedidoAtual.RecalcularTotal();
        }

        private void Finalizar()
        {
            try
            {
                _pedidoService.Finalizar(PedidoAtual);
                Finalizado = true;
                Pedidos.Clear();
                foreach (var p in _pedidoService.GetAll()) Pedidos.Add(p);
                MessageBox.Show(
                    "Pedido finalizado. Total: R$ " + PedidoAtual.ValorTotal.ToString("N2"),
                    "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DescartarSeNaoFinalizado()
        {
            if (PedidoAtual != null && !Finalizado)
            {
                PedidoAtual = new Pedido();
                PessoaSelecionada = null;
            }
        }
    }
}
