using System;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Debt;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class DebtController : Controller
    {
        private readonly ObjectRepository _repository;

        public DebtController(ObjectRepository repository) => _repository = repository;

        public ActionResult Index() => View(_repository.Set<DebtModel>().Select(v=>new DebtViewModel(v)).ToList());

        public IActionResult AddDebt() => View("EditDebt");

        [HttpGet]
        public IActionResult EditDebt(Guid id) => View("EditDebt", _repository.Set<DebtModel>().FirstOrDefault(v => v.Id == id));

        [HttpPost]
        public IActionResult EditDebt(Guid id, DateTime when, double amount, double returned, string ccy, double percentage, int daysCount, string description)
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

            model.When = when;
            model.Amount = amount;
            model.Ccy = ccy;
            model.Percentage = percentage;
            model.DaysCount = daysCount;
            model.Description = description;
            model.Returned = returned;
            
            return RedirectToAction("Index");
        }
    }
}