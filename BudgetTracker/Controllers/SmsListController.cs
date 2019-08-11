using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class SmsListController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public SmsListController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IEnumerable<SmsMonthViewModel> IndexJson()
        {
            return SmsMonthViewModel.FromSms(_objectRepository).OrderByDescending(v=>v.When).ToList();
        }

        public IEnumerable<SmsRuleJsModel> SmsRules() => _objectRepository.Set<RuleModel>().Select(v=>new SmsRuleJsModel(v)).ToList();

        public OkResult CreateRule(RuleType ruleType, string regexSender, string regexText)
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            if (!string.IsNullOrEmpty(regexSender))
                new Regex(regexSender, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            if (!string.IsNullOrEmpty(regexText))
                new Regex(regexText, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");

            var rule = new RuleModel(ruleType, regexSender, regexText);
            _objectRepository.Add(rule);
            return Ok();
        }

        public OkResult DeleteSms(Guid id)
        {
            var sms = _objectRepository.Set<SmsModel>().First(v => v.Id == id);

            foreach(var p in sms.Payments){
            {
                p.Sms = null;
            }}
            
            _objectRepository.Remove(sms);
            return Ok();
        }

        public OkResult DeleteRule(Guid id)
        {
            var rule = _objectRepository.Set<RuleModel>().First(v => v.Id == id);

            lock (typeof(SmsRuleProcessor))
            {
                foreach (var s in rule.Smses)
                {
                    s.AppliedRule = null;
                }

                _objectRepository.Remove(rule);
            }

            return Ok();
        }
    }
}