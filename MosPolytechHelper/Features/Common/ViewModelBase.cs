namespace MosPolytechHelper.Features.Common
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (property != null && property.Equals(value) || (property == null && value == null))
                return;
            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}