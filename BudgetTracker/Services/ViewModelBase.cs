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
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void Dispose()
        {
            foreach (var anchor in Anchors)
            {
                anchor();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}