namespace MosPolytechHelper.Features.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    abstract class ViewModelBase : INotifyPropertyChanged
    {
        readonly ViewModels vm;
        IMediator<ViewModels, VmMessage> mediator;

        protected void Subscribe(Action<VmMessage> action)
        {
            this.mediator?.Subscribe(this.vm, action);
        }

        protected void Unsubscribe()
        {
            this.mediator?.Unsubscribe(this.vm);
        }

        protected void Send(ViewModels target, params object[] subMessages)
        {
            this.mediator?.Send(target, new VmMessage(subMessages));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase(IMediator<ViewModels, VmMessage> mediator, ViewModels vm)
        {
            this.mediator = mediator;
            this.vm = vm;
        }


        protected void SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (property != null && property.Equals(value) || (property == null && value == null))
                return;
            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}