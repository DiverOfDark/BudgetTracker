using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class SpentCategoriesViewModel : GrpcViewModelBase<SpentCategoriesStream>
    {
        public SpentCategoriesViewModel(ObjectRepository objectRepository, ILogger<SpentCategoriesViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            SendSnapshot();
            return Task.CompletedTask;
        }

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            // TODO debounce
            if (obj.Source is SpentCategoryModel spentCategoryModel)
            {
                switch (obj.ChangeType)
                {
                    case ChangeType.Update:
                        SendUpdate(new SpentCategoriesStream {Updated = ToSpentCategory(spentCategoryModel)});
                        break;
                    case ChangeType.Add:
                        SendUpdate(new SpentCategoriesStream {Added = ToSpentCategory(spentCategoryModel)});
                        break;
                    case ChangeType.Remove:
                        SendUpdate(new SpentCategoriesStream {Removed = ToSpentCategory(spentCategoryModel)});
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
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

        private void SendSnapshot()
        {
            var model = new SpentCategoriesStream {Snapshot = new SpentCategoryList()};
            model.Snapshot.SpentCategories.AddRange(ObjectRepository.Set<SpentCategoryModel>().Select(ToSpentCategory).ToList());
            SendUpdate(model);
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