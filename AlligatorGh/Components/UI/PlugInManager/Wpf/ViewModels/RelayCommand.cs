using System;
using System.Windows.Input;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels
{
    /// <summary>
    /// A minimal <see cref="ICommand"/> backed by delegates, so the view-models need no MVVM
    /// framework dependency.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="execute">The action to run.</param>
        /// <param name="canExecute">Optional guard; always executable when null.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <inheritdoc />
        public void Execute(object parameter) => _execute();
    }
}
