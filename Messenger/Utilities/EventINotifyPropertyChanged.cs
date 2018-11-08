using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Messenger.Client.Utilities
{
    public abstract class EventINotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberNameAttribute] string propertyName = null)
        {
            if (!oldValue?.Equals(newValue) ?? newValue != null)
            {
                oldValue = newValue;

                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}