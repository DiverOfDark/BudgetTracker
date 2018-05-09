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
    }
}