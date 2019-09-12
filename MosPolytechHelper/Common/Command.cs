namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using System;
    using System.Threading.Tasks;


    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private bool isExecuting;
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        private readonly IErrorHandler errorHandler;

        public Command(
            Action execute,
            Func<bool> canExecute = null,
            IErrorHandler errorHandler = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.errorHandler = errorHandler;
        }

        public bool CanExecute()
        {
            return !this.isExecuting && (this.canExecute?.Invoke() ?? true);
        }


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            try
            {
                this.execute();
            }
            catch (Exception ex)
            {
                this.errorHandler?.HandleError(ex);
            }
        }
        #endregion
    }

    public class Command<T> : ICommand<T>
    {
        public event EventHandler CanExecuteChanged;

        private bool isExecuting;
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;
        private readonly IErrorHandler errorHandler;

        public Command(Action<T> execute, Func<T, bool> canExecute = null, IErrorHandler errorHandler = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.errorHandler = errorHandler;
        }

        public bool CanExecute(T parameter)
        {
            return !this.isExecuting && (this.canExecute?.Invoke(parameter) ?? true);
        }

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    this.isExecuting = true;
                    this.execute(parameter);
                }
                finally
                {
                    this.isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            try
            {
                this.execute((T)parameter);
            }
            catch (Exception ex)
            {
                this.errorHandler?.HandleError(ex);
            }
        }
        #endregion
    }
}