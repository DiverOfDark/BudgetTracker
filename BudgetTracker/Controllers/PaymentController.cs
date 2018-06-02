using System;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Controllers.ViewModels.Payment;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public PaymentController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public ActionResult Index(bool? groups)
        {
            var groups2 = this.TryGetLastValue(groups, nameof(PaymentController) + nameof(groups)) ?? true;

            ViewBag.Groups = groups2;
            
            return View(PaymentMonthViewModel.FromPayments(_objectRepository, groups2).OrderByDescending(v => v.When).ToList());
        }

        public ActionResult SpentCategories() => View(_objectRepository.Set<SpentCategoryModel>().ToList());

        public IActionResult CreateCategory(string pattern, string category, PaymentKind kind)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.Match("test", pattern);
            }
            catch
            {
                return RedirectToAction(nameof(SpentCategories));
            }

            _objectRepository.Add(new SpentCategoryModel(pattern, category, kind));
            return RedirectToAction(nameof(SpentCategories));
        }

        public IActionResult EditPayment(Guid id)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            var vm = new PaymentViewModel(payment);
            return View(vm);
        }

        [HttpPost]
        public IActionResult EditPayment(Guid id, double amount, string ccy, string what, Guid? categoryId, Guid? columnId, PaymentKind kind)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);

            payment.Amount = amount;
            payment.Ccy = ccy;
            payment.What = what;
            payment.Category = null;
            payment.Kind = kind;
            payment.Category = _objectRepository.Set<SpentCategoryModel>().FirstOrDefault(v => v.Id == categoryId);
            payment.Column = _objectRepository.Set<MoneyColumnMetadataModel>().FirstOrDefault(v => v.Id == columnId);
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult DeletePayment(Guid id)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            if (payment.Sms != null)
            {
                payment.Sms.AppliedRule = null;
            }

            _objectRepository.Remove(payment);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteCategory(Guid id)
        {
            lock (typeof(SpentCategoryProcessor))
            {
                var category = _objectRepository.Set<SpentCategoryModel>().First(x => x.Id == id);

                foreach (var item in category.Payments)
                {
                    item.Category = null;
                }

                _objectRepository.Remove(category);
            }

            return RedirectToAction(nameof(SpentCategories));
        }

        public IActionResult EditCategory(Guid id) => View(_objectRepository.Set<SpentCategoryModel>().First(v => v.Id == id));

        [HttpPost]
        public IActionResult EditCategory(Guid id, string pattern, string category, PaymentKind kind)
        {
            var categoryObj = _objectRepository.Set<SpentCategoryModel>().First(v => v.Id == id);

            categoryObj.Pattern = pattern;
            categoryObj.Category = category;
            categoryObj.Kind = kind;
            
            foreach (var p in categoryObj.Payments)
            {
                p.Category = null;
            }
            
            return RedirectToAction(nameof(SpentCategories));
        }
    }
}