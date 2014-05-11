using System;
using System.Windows.Input;

namespace ssh_tunnel_agent.Classes {
    // based on http://mvvmlight.codeplex.com/
    public class RelayCommand<T> : ICommand {
        readonly Action<T> _execute;
        readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute) : this(execute, null) { }
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute) {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged {
            add {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void RaiseCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter) {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default(T));

            return _canExecute((T)parameter);
        }

        public void Execute(object parameter) {
            if (_execute == null || !CanExecute(parameter))
                return;

            if (parameter == null && typeof(T).IsValueType)
                _execute(default(T));
            else
                _execute((T)parameter);
        }
    }
}
