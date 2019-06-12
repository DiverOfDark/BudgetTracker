using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Controllers.ViewModels.Payment;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using SQLitePCL;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class PaymentController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public PaymentController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IEnumerable<PaymentViewModel> Index()
        {
            return _objectRepository.Set<PaymentModel>().Select(v => new PaymentViewModel(v)).ToList();
        }

        public IEnumerable<SpentCategoryJsModel> SpentCategories() => _objectRepository.Set<SpentCategoryModel>().Select(v=>new SpentCategoryJsModel(v)).ToList();

        public OkResult CreateCategory(string pattern, string category, PaymentKind kind)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");
                }
            }
            catch
            {
                pattern = "";
            }

            _objectRepository.Add(new SpentCategoryModel(pattern, category, kind));
            return Ok();
        }

        public PaymentViewModel EditPayment(Guid id)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            var vm = new PaymentViewModel(payment);
            return vm;
        }

        [HttpPost]
        public OkResult SplitPayment(Guid id, double amount)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);

            var newKind = amount < 0 ? PaymentKind.Expense : PaymentKind.Income;

            if (payment.Kind != PaymentKind.Expense && payment.Kind != PaymentKind.Income)
            {
                newKind = payment.Kind;

                payment.Amount -= amount;
            }
            else
            {
                amount = Math.Abs(amount);

                if (payment.Kind == newKind.GetOpposite())
                {
                    amount = -amount;
                }
                
                payment.Amount -= amount;

                amount = Math.Abs(amount);
            }

            if (Math.Abs(amount) > 0.01)
            {
                if (payment.Amount < 0 && payment.Kind != payment.Kind.GetOpposite())
                {
                    payment.Kind = payment.Kind.GetOpposite();
                    payment.Amount = Math.Abs(payment.Amount);
                }

                var newPayment = new PaymentModel(payment.When, payment.What, amount, newKind, payment.Ccy, null, payment.Column);
                _objectRepository.Add(newPayment);
                newPayment.UserEdited = true;
            }
            
            return Ok();
        }

        [HttpPost]
        public OkResult EditPayment(Guid id, double amount, string ccy, string what, Guid? categoryId, Guid? columnId, Guid? debtId, PaymentKind kind)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);

            payment.Amount = amount;
            payment.Ccy = ccy;
            payment.What = what;
            payment.Category = null;
            payment.Kind = kind;
            payment.Category = categoryId == null ? null : _objectRepository.Set<SpentCategoryModel>().Find(categoryId.Value);
            payment.Column = columnId == null ? null : _objectRepository.Set<MoneyColumnMetadataModel>().Find(columnId.Value);
            payment.Debt = debtId == null ? null : _objectRepository.Set<DebtModel>().Find(debtId.Value);
            payment.UserEdited = true;

            new SpentCategoryProcessor(_objectRepository).Process();
            
            return Ok();
        }
        
        public OkResult DeletePayment(Guid id)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            if (payment.Sms != null)
            {
                payment.Sms.AppliedRule = null;
            }

            _objectRepository.Remove(payment);
            return Ok();
        }

        public OkResult DeleteCategory(Guid id)
        {
            lock (typeof(SpentCategoryProcessor))
            {
                var category = _objectRepository.Set<SpentCategoryModel>().First(x => x.Id == id);

                var substituteCategory = _objectRepository.Set<SpentCategoryModel>()
                    .FirstOrDefault(v => v != category && v.Category == category.Category);
                
                foreach (var item in category.Payments)
                {
                    item.Category = substituteCategory;
                }

                _objectRepository.Remove(category);
            }

            return Ok();
        }

        public SpentCategoryJsModel EditCategory(Guid id) => new SpentCategoryJsModel(_objectRepository.Set<SpentCategoryModel>().First(v => v.Id == id));

        [HttpPost]
        public OkResult EditCategory(Guid id, string pattern, string category, PaymentKind kind)
        {
            var categoryObj = _objectRepository.Set<SpentCategoryModel>().First(v => v.Id == id);

            categoryObj.Pattern = pattern;
            categoryObj.Category = category;
            categoryObj.Kind = kind;
            
            return Ok();
        }
    }
}