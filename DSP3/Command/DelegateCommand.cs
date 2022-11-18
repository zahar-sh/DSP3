using System;
using System.Windows.Input;

namespace DSP3.Command
{
    public class DelegateCommand : ICommand
    {
        private static readonly Action<object> DoNothing = o => { };
        private static readonly Predicate<object> AnyTrue = o => true;

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? DoNothing;
            _canExecute = canExecute ?? AnyTrue;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}
