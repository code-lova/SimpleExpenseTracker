using System.ComponentModel;

namespace SimpleExpenseTracker.Services
{
    public class NotificationService : INotifyPropertyChanged
    {
        private string _message = string.Empty;
        private NotificationType _type = NotificationType.Info;
        private bool _isVisible = false;

        public string Message
        {
            get => _message;
            private set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        public NotificationType Type
        {
            get => _type;
            private set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            private set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? StateHasChanged;

        public void ShowSuccess(string message)
        {
            ShowNotification(message, NotificationType.Success);
        }

        public void ShowError(string message)
        {
            ShowNotification(message, NotificationType.Error);
        }

        public void ShowWarning(string message)
        {
            ShowNotification(message, NotificationType.Warning);
        }

        public void ShowInfo(string message)
        {
            ShowNotification(message, NotificationType.Info);
        }

        public void Hide()
        {
            IsVisible = false;
            StateHasChanged?.Invoke();
        }

        private void ShowNotification(string message, NotificationType type)
        {
            Message = message;
            Type = type;
            IsVisible = true;
            StateHasChanged?.Invoke();

            // Auto-hide after 5 seconds for success/info messages
            if (type == NotificationType.Success || type == NotificationType.Info)
            {
                _ = Task.Delay(5000).ContinueWith(_ => Hide());
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
