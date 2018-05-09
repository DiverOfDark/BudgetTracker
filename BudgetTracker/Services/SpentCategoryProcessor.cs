using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BudgetTracker.Model;

namespace BudgetTracker.Services
{
    public class SpentCategoryProcessor
    {
        private readonly ObjectRepository _objectRepository;

        public SpentCategoryProcessor(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public void Process()
        {
            try
            {
                Monitor.Enter(this);
                
                var payments = _objectRepository.Set<PaymentModel>().Where(v => v.Category == null).ToList();
                var categories = _objectRepository.Set<SpentCategoryModel>();

                var regexOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
                var cats = categories.ToDictionary(v => v, v => new Regex(v.Pattern, regexOptions));
                foreach (var p in payments)
                {
                    foreach (var category in cats)
                    {
                        if (category.Value.IsMatch(p.What))
                        {
                            p.Category = category.Key;
                            p.Kind = category.Key.Kind;
                            break;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}