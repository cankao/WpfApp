using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp.Models
{
    public class Pessoa : INotifyPropertyChanged
    {
        private int _id;
        private string _nome;
        private string _cpf;
        private string _endereco;

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        public string Nome
        {
            get { return _nome; }
            set { _nome = value; OnPropertyChanged(); }
        }

        public string Cpf
        {
            get { return _cpf; }
            set { _cpf = value; OnPropertyChanged(); }
        }

        public string Endereco
        {
            get { return _endereco; }
            set { _endereco = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return Nome + " (" + Cpf + ")";
        }
    }
}
