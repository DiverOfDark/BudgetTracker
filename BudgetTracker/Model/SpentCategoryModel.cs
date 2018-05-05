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
            public string Category { get; set; }
        }
        
        public SpentCategoryModel(SpendCategoryEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public SpentCategoryModel(string pattern, string category)
        {
            Id = Guid.NewGuid();
            _entity = new SpendCategoryEntity
            {
                Pattern = pattern,
                Category = category,
                RowKey = Id.ToString(),
                PartitionKey = nameof(SpendCategoryEntity)
            };
        }
        
        public override Guid Id { get; }
        protected override Object Entity => _entity;

        public string Pattern => _entity.Pattern;
        public string Category => _entity.Category;
        
        public IEnumerable<PaymentModel> Payments => Multiple<PaymentModel>(v => v.CategoryId);
    }
}