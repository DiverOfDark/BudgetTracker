using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public enum WidgetKind
    {
        [DisplayName("Не задан")] Unknown,
        [DisplayName("Последнее значение")] LastValue,
        [DisplayName("Траты за месяц")] Expenses,
        [DisplayName("График")] Chart,
        [DisplayName("Дельта")] Delta,
        [DisplayName("Карта")] Burst
    }
        
    public sealed class WidgetModel : ModelBase
    {
        public class WidgetEntity : BaseEntity
        {
            public int Order { get; set; }
            public string Title { get; set; }
            
            public string Properties { get; set; }
            
            public int WidgetKind { get; set; }
        }

        private readonly WidgetEntity _entity;
        
        public WidgetModel(WidgetEntity entity)
        {
            _entity = entity;
        }

        public WidgetModel(int order, string title, WidgetKind kind)
        {
            _entity = new WidgetEntity
            {
                Id = Guid.NewGuid(),
                Order = order,
                Title = title,
                WidgetKind = (int) kind,
            };
        }

        public override Guid Id => _entity.Id;
        protected override object Entity => _entity;

        public int Order
        {
            get => _entity.Order;
            set => UpdateProperty(() => _entity.Order, value);
        }

        public string Title
        {
            get => _entity.Title;
            set => UpdateProperty(() => _entity.Title, value);
        }

        public ReadOnlyDictionary<string, string> Properties
        {
            get
            {
                try
                {
                    return new ReadOnlyDictionary<string, string>(
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(_entity.Properties));
                }
                catch
                {
                    return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
                }             
            }
        }

        public WidgetKind Kind
        {
            get => (WidgetKind) _entity.WidgetKind;
            set => UpdateProperty(() => _entity.WidgetKind, (int)value);
        }

        public void SetProperties(Dictionary<string, string> properties) =>
            UpdateProperty(() => _entity.Properties, JsonConvert.SerializeObject(properties));

    }
}