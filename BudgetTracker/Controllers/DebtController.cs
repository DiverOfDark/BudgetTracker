using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class DebtController : Controller
    {
        private readonly ObjectRepository _repository;

        public DebtController(ObjectRepository repository) => _repository = repository;

        public IEnumerable<DebtJsViewModel> IndexJson() =>
            _repository.Set<DebtModel>().Select(v => new DebtJsViewModel(v)).ToList();

        [HttpPost]
        public OkResult EditDebt(Guid id, DateTime when, double amount, string ccy, double percentage, int daysCount, string description, string regexForTransfer)
        {
            DebtModel model;
            if (id == Guid.Empty)
            {
                model = new DebtModel();
                _repository.Add(model);
            }
            else
            {
                model = _repository.Set<DebtModel>().First(v => v.Id == id);
            }

            model.When = new DateTime(when.Ticks, DateTimeKind.Utc);
            model.Amount = amount;
            model.Ccy = ccy;
            model.Percentage = percentage;
            model.DaysCount = daysCount;
            model.Description = description;
            try
            {
                if (!string.IsNullOrWhiteSpace(regexForTransfer))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    new Regex(regexForTransfer, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");
                }
            }
            catch
            {
                regexForTransfer = "";
            }
            model.RegexForTransfer = regexForTransfer;

            
            return Ok();
        }
    }
}