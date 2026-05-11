using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace WpfApp.Models
{
    public class ItemPedido : INotifyPropertyChanged
    {
        private int _produtoId;
        private string _produtoNome;
        private string _produtoCodigo;
        private decimal _valorUnitario;
        private int _quantidade;

        public int ProdutoId
        {
            get { return _produtoId; }
            set { _produtoId = value; OnPropertyChanged(); }
        }

        public string ProdutoNome
        {
            get { return _produtoNome; }
            set { _produtoNome = value; OnPropertyChanged(); }
        }

        public string ProdutoCodigo
        {
            get { return _produtoCodigo; }
            set { _produtoCodigo = value; OnPropertyChanged(); }
        }

        public decimal ValorUnitario
        {
            get { return _valorUnitario; }
            set { _valorUnitario = value; OnPropertyChanged(); OnPropertyChanged("Subtotal"); }
        }

        public int Quantidade
        {
            get { return _quantidade; }
            set { _quantidade = value; OnPropertyChanged(); OnPropertyChanged("Subtotal"); }
        }

        [JsonIgnore]
        public decimal Subtotal
        {
            get { return ValorUnitario * Quantidade; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
