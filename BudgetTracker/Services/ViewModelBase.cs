using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace BudgetTracker.Services
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected readonly List<Action> Anchors = new List<Action>();

        private bool _isDisposed;

        public bool IsDisposed => _isDisposed;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void Dispose()
        {
            _isDisposed = true;
            foreach (var anchor in Anchors)
            {
                anchor();
            }
        }

        ~ViewModelBase()
        {
            if (!_isDisposed)
            {
                Dispose();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}