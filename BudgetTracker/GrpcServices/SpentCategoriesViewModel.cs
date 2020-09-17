using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.GrpcServices
{
    public class SpentCategoriesViewModel : GrpcViewModelBase<SpentCategoriesStream>
    {
        private ServerObservableCollection<SpentCategoryModel, SpentCategoriesStream> _collection;

        public SpentCategoriesViewModel(ObjectRepository objectRepository, ILogger<SpentCategoriesViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            _collection = new ObjectRepositoryServerObservableCollection<SpentCategoryModel, SpentCategoriesStream>(
                ObjectRepository,
                SendUpdate,
                (x, i) => new SpentCategoriesStream {Added = ToSpentCategory(x)},
                (x, i) => new SpentCategoriesStream {Removed = ToSpentCategory(x)},
                (x, i) => new SpentCategoriesStream {Updated = ToSpentCategory(x)},
                list =>
                {
                    var model = new SpentCategoriesStream {Snapshot = new SpentCategoryList()};
                    model.Snapshot.SpentCategories.AddRange(list.Select(ToSpentCategory).ToList());
                    return model;
                });
            
            Anchors.Add(() => _collection.Dispose());
            
            return Task.CompletedTask;
        }

        private SpentCategory ToSpentCategory(SpentCategoryModel spentCategoryModel)
        {
            return new SpentCategory
            {
                Category = spentCategoryModel.Category ?? "",
                Id = spentCategoryModel.Id.ToUUID(),
                Kind = spentCategoryModel.Kind,
                Pattern = spentCategoryModel.Pattern ?? ""
            };
        }

        public void DeleteCategory(UUID request)
        {
            var id = request.ToGuid();
            var category = ObjectRepository.Set<SpentCategoryModel>().First(x => x.Id == id);

            var substituteCategory = ObjectRepository.Set<SpentCategoryModel>()
                .FirstOrDefault(v => v != category && v.Category == category.Category);

            foreach (var item in category.Payments)
            {
                item.Category = substituteCategory;
            }

            ObjectRepository.Remove(category);
        }

        public void EditCategory(SpentCategory request)
        {
            if (request.Id.ToGuid() == Guid.Empty)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.Pattern))
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        new Regex(request.Pattern, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");
                    }
                }
                catch
                {
                    request.Pattern = "";
                }

                var spentCategoryModel = new SpentCategoryModel(request.Pattern, request.Category, request.Kind);
                ObjectRepository.Add(spentCategoryModel);
            }
            else
            {
                var id = request.Id.ToGuid();
                var categoryObj = ObjectRepository.Set<SpentCategoryModel>().First(v => v.Id == id);

                categoryObj.Pattern = request.Pattern;
                categoryObj.Category = request.Category;
                categoryObj.Kind = request.Kind;
            }
        }
    }
}