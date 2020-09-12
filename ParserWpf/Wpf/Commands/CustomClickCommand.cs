using System;
using System.Windows.Input;

namespace ParserWpf.Wpf.Commands
{
    public class CustomClickCommand : ICommand
    {
        public Func<object, bool> CanExecutePredicate { get; set; } = flag => true;

        public Action<object> ExecuteCommandAction { get; set; } = null;

        public bool CanExecute(object parameter) => CanExecutePredicate(parameter);

        public void Execute(object parameter) => ExecuteCommandAction?.Invoke(parameter);

        public event EventHandler CanExecuteChanged;
    }
}