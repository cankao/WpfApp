using System;
using System.Collections.ObjectModel;
using System.Windows;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class ProdutoViewModel : ViewModelBase
    {
        private readonly ProdutoService _service;

        public ObservableCollection<Produto> Produtos { get; private set; }

        private Produto _selecionado;
        public Produto Selecionado
        {
            get { return _selecionado; }
            set
            {
                if (SetProperty(ref _selecionado, value))
                {
                    CopiarParaEdicao(value);
                }
            }
        }

        private Produto _emEdicao;
        public Produto EmEdicao
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

        private string _filtroCodigo;
        public string FiltroCodigo
        {
            get { return _filtroCodigo; }
            set { if (SetProperty(ref _filtroCodigo, value)) Filtrar(); }
        }

        private decimal? _valorMin;
        public decimal? ValorMin
        {
            get { return _valorMin; }
            set { if (SetProperty(ref _valorMin, value)) Filtrar(); }
        }

        private decimal? _valorMax;
        public decimal? ValorMax
        {
            get { return _valorMax; }
            set { if (SetProperty(ref _valorMax, value)) Filtrar(); }
        }

        public RelayCommand IncluirCommand { get; private set; }
        public RelayCommand EditarCommand { get; private set; }
        public RelayCommand SalvarCommand { get; private set; }
        public RelayCommand ExcluirCommand { get; private set; }

        public ProdutoViewModel(ProdutoService service)
        {
            _service = service;
            Produtos = new ObservableCollection<Produto>();
            EmEdicao = new Produto();
            CarregarTodos();

            IncluirCommand = new RelayCommand(o => Incluir());
            EditarCommand = new RelayCommand(o => Editar(), () => Selecionado != null);
            SalvarCommand = new RelayCommand(o => Salvar());
            ExcluirCommand = new RelayCommand(o => Excluir(), () => Selecionado != null);
        }

        private void CarregarTodos()
        {
            Produtos.Clear();
            foreach (var p in _service.GetAll()) Produtos.Add(p);
        }

        private void Filtrar()
        {
            Produtos.Clear();
            foreach (var p in _service.Search(FiltroNome, FiltroCodigo, ValorMin, ValorMax))
                Produtos.Add(p);
        }

        private void CopiarParaEdicao(Produto origem)
        {
            if (origem == null) { EmEdicao = new Produto(); return; }
            EmEdicao = new Produto
            {
                Id = origem.Id,
                Nome = origem.Nome,
                Codigo = origem.Codigo,
                Valor = origem.Valor
            };
        }

        private void Incluir()
        {
            Selecionado = null;
            EmEdicao = new Produto();
        }

        private void Editar()
        {
            CopiarParaEdicao(Selecionado);
        }

        private void Salvar()
        {
            try
            {
                if (EmEdicao.Id == 0)
                {
                    _service.Add(EmEdicao);
                    MessageBox.Show("Produto cadastrado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _service.Update(EmEdicao);
                    MessageBox.Show("Produto atualizado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                CarregarTodos();
                EmEdicao = new Produto();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Excluir()
        {
            if (Selecionado == null) return;
            var result = MessageBox.Show(
                "Confirma exclusão de " + Selecionado.Nome + "?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                _service.Delete(Selecionado.Id);
                CarregarTodos();
                EmEdicao = new Produto();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
