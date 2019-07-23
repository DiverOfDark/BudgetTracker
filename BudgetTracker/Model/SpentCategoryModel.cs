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
        }

        public SpentCategoryModel(string pattern, string category, PaymentKind kind)
        {
            _entity = new SpendCategoryEntity
            {
                Id = Guid.NewGuid(),
                Pattern = pattern,
                Category = category,
                Kind = (int) kind
            };
        }

        protected override BaseEntity Entity => _entity;

        public string Pattern
        {
            get => _entity.Pattern;
            set => UpdateProperty(_entity, () => x => x.Pattern, value);
        }

        public string Category
        {
            get => _entity.Category;
            set => UpdateProperty(_entity, () => x => x.Category, value);
        }

        public PaymentKind Kind
        {
            get => (PaymentKind) _entity.Kind;
            set => UpdateProperty(_entity, () => x => x.Kind, (int) value);
        }

        public IEnumerable<PaymentModel> Payments => Multiple<PaymentModel>(() => v => v.CategoryId);
    }
}