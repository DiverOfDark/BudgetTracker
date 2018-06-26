using System;
using System.Globalization;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class TableController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public TableController(ObjectRepository objectRepository) => _objectRepository = objectRepository;

        public IActionResult Index(bool? showAll, bool? showControls, bool? showDelta, bool? exemptTransfers)
        {
            var showAll2 = this.TryGetLastValue(showAll, nameof(TableController) + nameof(showAll)) ?? false;
            var showControls2 = this.TryGetLastValue(showControls, nameof(TableController) + nameof(showControls)) ?? false;
            var showDelta2 = this.TryGetLastValue(showDelta, nameof(TableController) + nameof(showDelta)) ?? false;
            var exemptTransfers2 = this.TryGetLastValue(exemptTransfers, nameof(TableController) + nameof(exemptTransfers)) ?? false;
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();

            var vm = table.GetVM(exemptTransfers2);
            vm.ShowAll = showAll2;
            vm.ShowDelta = showDelta2;
            vm.ShowControls = showControls2;
            vm.ExemptTransfers = exemptTransfers2;
            return View(vm);
        }

        public IActionResult Chart(string provider, string account, bool exemptTransfers = false)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();
            var vm = new ChartWidgetViewModel(provider, account, ChartKind.Linear, _objectRepository, table.GetVM(exemptTransfers));

            return View(vm);
        }

        public IActionResult DeleteMoney(Guid id)
        {
            _objectRepository.Remove<MoneyStateModel>(x => x.Id == id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CopyFromPrevious(Guid headerid, string date)
        {
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture);

            var column = _objectRepository.Set<MoneyColumnMetadataModel>().First(v => v.Id == headerid);

            var previous = _objectRepository.Set<MoneyStateModel>().Where(v =>
                    v.AccountName == column.AccountName && v.Provider == column.Provider && v.When < realDate)
                .OrderByDescending(v => v.When).First();
            
            _objectRepository.Add(new MoneyStateModel
            {
                AccountName = column.AccountName,
                Amount = previous.Amount,
                When = realDate,
                Provider = column.Provider,
                Ccy = previous.Ccy
            });

            return RedirectToAction(nameof(Index));
        }

        public IActionResult MarkAsOk(Guid headerid, string date)
        {
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture);

            var column = _objectRepository.Set<MoneyColumnMetadataModel>().First(v => v.Id == headerid);

            _objectRepository.Add(new MoneyStateModel
            {
                AccountName = column.AccountName,
                Amount = double.NaN,
                When = realDate,
                Provider = column.Provider
            });

            return RedirectToAction(nameof(Index));
        }
    }
}