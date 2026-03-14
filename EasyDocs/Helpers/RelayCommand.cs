using System;
using System.Windows.Input;
#pragma warning disable
namespace EasyDocs.Helpers
{
    /// <summary>
    /// A simple implementation of the <see cref="ICommand"/> interface
    /// that relays its functionality to specified delegates.
    /// </summary>
    /// <remarks>
    /// This class is commonly used in the MVVM (Model–View–ViewModel) pattern
    /// to bind UI actions to ViewModel methods without requiring code-behind event handlers.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="newExecute">
        /// The <see cref="Action"/> to invoke when the command is executed.
        /// </param>
        /// <param name="newCanExecute">
        /// An optional <see cref="Func{TResult}"/> that determines whether the command
        /// can currently be executed. If <see langword="null"/>, the command is always executable.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="newExecute"/> is <see langword="null"/>.
        /// </exception>
        public RelayCommand(Action newExecute, Func<bool>? newCanExecute = null)
        {
            execute = newExecute ?? throw new ArgumentNullException(nameof(newExecute));
            canExecute = newCanExecute;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// An optional parameter passed from the command source. This implementation ignores it.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanExecute(object? parameter) => canExecute == null || canExecute();

        /// <summary>
        /// Executes the command’s associated action.
        /// </summary>
        /// <param name="parameter">
        /// An optional parameter passed from the command source. This implementation ignores it.
        /// </param>
        public void Execute(object? parameter) => execute();

        /// <summary>
        /// Occurs when the conditions that determine whether the command can execute have changed.
        /// </summary>
        /// <remarks>
        /// To refresh command availability in UI frameworks such as WPF or Avalonia,
        /// call <see cref="CommandManager.InvalidateRequerySuggested"/> or raise this event manually.
        /// </remarks>
        public event EventHandler? CanExecuteChanged;
    }
}
