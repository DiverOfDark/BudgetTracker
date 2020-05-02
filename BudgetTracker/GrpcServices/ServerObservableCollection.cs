using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using BudgetTracker.Model;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class ObjectRepositoryServerObservableCollection<T, TProto> : ServerObservableCollection<T, TProto> where T : ModelBase
    {
        private readonly ObjectRepository _objectRepository;

        public ObjectRepositoryServerObservableCollection(
            ObjectRepository objectRepository,
            Action<TProto> onChange, 
            Func<T, int, TProto> onAdd, 
            Func<T, int, TProto> onRemove, 
            Func<T, int, TProto> onUpdate,
            Func<IEnumerable<T>, TProto> onSnapshot) : base(onChange, onAdd, onRemove, onUpdate)
        {
            SendUpdates = false;
            var modelBases = objectRepository.Set<T>();
            foreach (T model in modelBases)
            {
                Add(model);
            }

            onChange(onSnapshot(modelBases));
            SendUpdates = true;

            _objectRepository = objectRepository;
            objectRepository.ModelChanged += Handler;
        }

        public override void Dispose()
        {
            _objectRepository.ModelChanged -= Handler;
            base.Dispose();
        }

        private void Handler(ModelChangedEventArgs obj)
        {
            if (obj.Source is T model)
            {
                if (obj.ChangeType == ChangeType.Add)
                {
                    Add(model);
                }

                if (obj.ChangeType == ChangeType.Remove)
                {
                    Remove(model);
                }
            }
        }
    }

    public class ServerObservableCollection<T, TProto> : ObservableCollection<T>, IDisposable where T : class, INotifyPropertyChanged
    {
        private readonly Action<TProto> _onChange;
        private readonly Func<T, int, TProto> _onAdd;
        private readonly Func<T, int, TProto> _onRemove;
        private readonly Func<T, int, TProto> _onUpdate;

        public ServerObservableCollection(
            Action<TProto> onChange, 
            Func<T, int, TProto> onAdd, 
            Func<T, int, TProto> onRemove, 
            Func<T, int, TProto> onUpdate)
        {
            _onChange = onChange;
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onUpdate = onUpdate;
        }

        public bool SendUpdates { get; set; } = true;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int idx = e.NewStartingIndex;
                    foreach (T v in e.NewItems)
                    {
                        v.PropertyChanged += OnChildChanged;
                        if (SendUpdates)
                        {
                            _onChange(_onAdd(v, idx++));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int idx2 = e.OldStartingIndex;
                    foreach (T v in e.OldItems)
                    {
                        v.PropertyChanged -= OnChildChanged;
                        if (SendUpdates)
                        {
                            _onChange(_onRemove(v, idx2));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
            base.OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            foreach (var child in this.ToList())
            {
                Remove(child);
            }
        }

        public virtual void Dispose() => ClearItems();

        private void OnChildChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO debounce
            var child = sender as T;
            if (SendUpdates)
            {
                _onChange(_onUpdate(child, IndexOf(child)));
            }
        }
    }
}