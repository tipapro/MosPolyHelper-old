namespace MosPolytechHelper.Common.Interfaces
{
    using System;

    public interface ICommand
    {
        event EventHandler CanExecuteChanged;
        bool CanExecute(object parameter);
        void Execute(object parameter);
    }
}