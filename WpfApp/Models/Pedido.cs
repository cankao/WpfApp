using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace WpfApp.Models
{
    public class Pedido : INotifyPropertyChanged
    {
        private int _id;
        private int _pessoaId;
        private string _pessoaNome;
        private ObservableCollection<ItemPedido> _itens;
        private decimal _valorTotal;
        private DateTime _dataVenda;
        private FormaPagamento _formaPagamento;
        private StatusPedido _status;
        private DateTime? _dataPagamento;

        public Pedido()
        {
            _itens = new ObservableCollection<ItemPedido>();
            _dataVenda = DateTime.Now;
            _status = StatusPedido.Pendente;
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        public int PessoaId
        {
            get { return _pessoaId; }
            set { _pessoaId = value; OnPropertyChanged(); }
        }

        public string PessoaNome
        {
            get { return _pessoaNome; }
            set { _pessoaNome = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ItemPedido> Itens
        {
            get { return _itens; }
            set { _itens = value; OnPropertyChanged(); }
        }

        public decimal ValorTotal
        {
            get { return _valorTotal; }
            set { _valorTotal = value; OnPropertyChanged(); }
        }

        public DateTime DataVenda
        {
            get { return _dataVenda; }
            set { _dataVenda = value; OnPropertyChanged(); }
        }

        public FormaPagamento FormaPagamento
        {
            get { return _formaPagamento; }
            set { _formaPagamento = value; OnPropertyChanged(); }
        }

        public StatusPedido Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); OnPropertyChanged("StatusTexto"); }
        }

        public DateTime? DataPagamento
        {
            get { return _dataPagamento; }
            set { _dataPagamento = value; OnPropertyChanged(); OnPropertyChanged("EstaPago"); }
        }

        [JsonIgnore]
        public bool EstaPago
        {
            get { return _dataPagamento.HasValue; }
        }

        [JsonIgnore]
        public string StatusTexto
        {
            get { return _status.ToString(); }
        }

        public void RecalcularTotal()
        {
            ValorTotal = Itens != null ? Itens.Sum(i => i.Subtotal) : 0m;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
