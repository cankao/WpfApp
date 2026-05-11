using System;
using System.Windows.Input;

namespace WpfApp.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, (Predicate<object>)null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute) : this(o => execute(), (Predicate<object>)null) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(o => execute(), canExecute == null ? (Predicate<object>)null : o => canExecute()) { }

        public RelayCommand(Action<object> execute, Func<bool> canExecute)
            : this(execute, canExecute == null ? (Predicate<object>)null : o => canExecute()) { }

        public RelayCommand(Action execute, Predicate<object> canExecute)
            : this(o => execute(), canExecute) { }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
