using System;
using System.Collections.Generic;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public sealed class SpentCategoryModel : ModelBase
    {
        private readonly SpendCategoryEntity _entity;

        public class SpendCategoryEntity : BaseEntity
        {
            public string Pattern { get; set; }
            public int Kind { get; set; }
            public string Category { get; set; }
        }
        
        public SpentCategoryModel(SpendCategoryEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public SpentCategoryModel(string pattern, string category, PaymentKind kind)
        {
            Id = Guid.NewGuid();
            _entity = new SpendCategoryEntity
            {
                Pattern = pattern,
                Category = category,
                RowKey = Id.ToString(),
                PartitionKey = nameof(SpendCategoryEntity),
                Kind = (int) kind
            };
        }
        
        public override Guid Id { get; }
        protected override Object Entity => _entity;

        public string Pattern
        {
            get => _entity.Pattern;
            set => UpdateProperty(() => _entity.Pattern, value);
        }

        public string Category
        {
            get => _entity.Category;
            set => UpdateProperty(() => _entity.Category, value);
        }

        public PaymentKind Kind
        {
            get => (PaymentKind) _entity.Kind;
            set => UpdateProperty(() => _entity.Kind, (int) value);
        }

        public IEnumerable<PaymentModel> Payments => Multiple<PaymentModel>(v => v.CategoryId);
    }
}