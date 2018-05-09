using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BudgetTracker.Model;

namespace BudgetTracker.Services
{
    public class SmsRuleProcessor
    {
        private readonly ObjectRepository _objectRepository;

        public SmsRuleProcessor(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public void Process()
        {
            try
            {
                Monitor.Enter(this);
                var smsList = _objectRepository.Set<SmsModel>().Where(v => v.AppliedRule == null).ToList();
                var rules = _objectRepository.Set<RuleModel>();

                var regexOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
                foreach (RuleModel rule in rules)
                {
                    var sReg = string.IsNullOrEmpty(rule.RegexSender)
                        ? null
                        : new Regex(rule.RegexSender, regexOptions);
                    var sText = string.IsNullOrEmpty(rule.RegexText) ? null : new Regex(rule.RegexText, regexOptions);

                    foreach (SmsModel sms in smsList)
                    {
                        if (sReg?.IsMatch(sms.From) != false && sText?.IsMatch(sms.Message) != false)
                        {
                            try
                            {
                                if (rule.RuleType == RuleType.Money)
                                {
                                    _objectRepository.Add(new PaymentModel(sms, rule));
                                }

                                sms.AppliedRule = rule;
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                var payments = _objectRepository.Set<PaymentModel>().Where(v => v.Category == null).ToList();
                var categories = _objectRepository.Set<SpentCategoryModel>();
                foreach (SpentCategoryModel category in categories)
                {
                    var reg = new Regex(category.Pattern, regexOptions);
                    foreach (var p in payments)
                    {
                        if (reg.IsMatch(p.What))
                        {
                            p.Category = category;
                        }
                    }
                }

                var oldSms = _objectRepository.Set<SmsModel>()
                    .Where(v => v.AppliedRule != null && v.When.AddDays(7) < DateTime.UtcNow).ToList();

                foreach (var item in oldSms)
                {
                    foreach (var p in item.Payments)
                        p.Sms = null;
                }

                _objectRepository.RemoveRange(oldSms);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}