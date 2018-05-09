using System;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Controllers.ViewModels.Sms;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class SmsListController : Controller
    {
        private readonly ObjectRepository _objectRepository;
        private readonly SmsRuleProcessor _smsRuleProcessor;

        public SmsListController(ObjectRepository objectRepository, SmsRuleProcessor smsRuleProcessor)
        {
            _objectRepository = objectRepository;
            _smsRuleProcessor = smsRuleProcessor;
        }

        public ActionResult Index(bool showHidden = false)
        {
            return View(new SmsListViewModel(_objectRepository, showHidden));
        }

        public ActionResult Payments()
        {
            return View(new SmsListViewModel(_objectRepository, false));
        }

        public IActionResult CreateCategory(string pattern, string category)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.Match("test", pattern);
            }
            catch
            {
                return RedirectToAction(nameof(Payments));
            }

            _objectRepository.Add(new SpentCategoryModel(pattern, category));
            return RedirectToAction(nameof(Payments));
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
                return RedirectToAction(nameof(Index));
            }

            var rule = new RuleModel(ruleType, regexSender, regexText);
            _objectRepository.Add(rule);
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteCategory(Guid id)
        {
            var category = _objectRepository.Set<SpentCategoryModel>().First(x => x.Id == id);

            foreach (var item in category.Payments)
            {
                item.Category = null;
            }
            
            _objectRepository.Remove(category);
            return RedirectToAction(nameof(Payments));
        }
    }
}