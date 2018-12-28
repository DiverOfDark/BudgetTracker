using System;
using System.Linq;
using System.Text.RegularExpressions;
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
            lock (typeof(SmsRuleProcessor))
            {
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
                        if ((sms.From != null && sReg?.IsMatch(sms.From) != false) && (sms.Message != null && sText?.IsMatch(sms.Message) != false))
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

                var oldSms = _objectRepository.Set<SmsModel>()
                    .Where(v => v.AppliedRule != null && v.When.AddDays(7) < DateTime.UtcNow).ToList();

                foreach (var item in oldSms)
                {
                    foreach (var p in item.Payments)
                        p.Sms = null;
                }

                _objectRepository.RemoveRange(oldSms);
            }
        }
    }
}