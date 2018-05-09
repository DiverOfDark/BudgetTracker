using System;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Controllers.ViewModels.Sms;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class SmsListController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public SmsListController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public ActionResult Index(bool showHidden = false) => View(new SmsListViewModel(_objectRepository, showHidden));

        public ActionResult SmsRules() => View(_objectRepository.Set<RuleModel>().ToList());

        public ActionResult Payments() => View(new SmsListViewModel(_objectRepository, false));

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
        
        public IActionResult CreateRule(RuleType ruleType, string regexSender, string regexText)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                if (!string.IsNullOrEmpty(regexSender))
                    Regex.Match("test", regexSender);
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                if (!string.IsNullOrEmpty(regexText))
                    Regex.Match("test", regexText);
            }
            catch
            {
                return RedirectToAction(nameof(SmsRules));
            }

            var rule = new RuleModel(ruleType, regexSender, regexText);
            _objectRepository.Add(rule);
            return RedirectToAction(nameof(SmsRules));
        }

        public IActionResult EditPayment(Guid id)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            var vm = new PaymentViewModel(payment);
            return View(vm);
        }

        [HttpPost]
        public IActionResult EditPayment(Guid id, double amount, string ccy, string what, PaymentKind kind)
        {
            var payment = _objectRepository.Set<PaymentModel>().First(v => v.Id == id);
            payment.Amount = amount;
            payment.Ccy = ccy;
            payment.What = what;
            payment.Category = null;
            payment.Kind = kind;
            return RedirectToAction("Payments");
        }

        public IActionResult DeleteSms(Guid id)
        {
            var sms = _objectRepository.Set<SmsModel>().First(v => v.Id == id);

            foreach(var p in sms.Payments){
            {
                p.Sms = null;
            }}
            
            _objectRepository.Remove(sms);
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
            return RedirectToAction(nameof(Payments));
        }

        public IActionResult DeleteRule(Guid id)
        {
            var rule = _objectRepository.Set<RuleModel>().First(v => v.Id == id);

            foreach (var s in rule.Smses)
            {
                s.AppliedRule = null;
            }
            
            _objectRepository.Remove(rule);
            return RedirectToAction(nameof(SmsRules));
        }

        public IActionResult DeleteCategory(Guid id)
        {
            var category = _objectRepository.Set<SpentCategoryModel>().First(x => x.Id == id);

            foreach (var item in category.Payments)
            {
                item.Category = null;
            }
            
            _objectRepository.Remove(category);
            return RedirectToAction(nameof(SpentCategories));
        }
    }
}