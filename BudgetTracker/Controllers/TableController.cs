using System;
using System.Globalization;
using System.Linq;
using System.Text;
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

        [AllowAnonymous, Route("/Table.csv")]
        public IActionResult IndexCsv(string password, bool exemptTransfers = false, bool excelCompatible = false)
        {
            if (!Startup.GlobalSettings.Password.Equals(password))
            {
                return Unauthorized();
            }
            
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();
            var vm = table.GetVM();

            var sb = new StringBuilder();

            if (excelCompatible)
            {
                sb.AppendLine("sep=,");
            }
            
            var headers = string.Join(",", vm.Headers.Select(v => (v.IsComputed ? "" : v.Provider + "/") + v.UserFriendlyName));

            headers = "Когда," + headers;
            
            sb.AppendLine(headers);

            foreach (var item in vm.Values)
            {
                var rowString = item.When.ToString("dd.MM.yyyy") + ",";

                rowString += string.Join(",", vm.Headers.Select(h =>
                {
                    item.Cells.TryGetValue(h, out var p); 

                    var value = exemptTransfers ? p?.Value : p?.AdjustedValue;
                    if (value != null && !double.IsNaN(value.Value))
                    {
                        return "\"" + value.Value.ToString("0.00###", new NumberFormatInfo {NumberDecimalSeparator = ","}) + "\"";
                    }

                    return "";
                }).ToList());
                
                sb.AppendLine(rowString);
            }


            var content = sb.ToString();
            
            
            return Content(content, "text/csv", Encoding.UTF8);
        }
        
        public IActionResult Index(bool? showAll, bool? showControls, bool? showDelta, bool? exemptTransfers)
        {
            var showAll2 = this.TryGetLastValue(showAll, nameof(TableController) + nameof(showAll)) ?? false;
            var showControls2 = this.TryGetLastValue(showControls, nameof(TableController) + nameof(showControls)) ?? false;
            var showDelta2 = this.TryGetLastValue(showDelta, nameof(TableController) + nameof(showDelta)) ?? false;
            var exemptTransfers2 = this.TryGetLastValue(exemptTransfers, nameof(TableController) + nameof(exemptTransfers)) ?? false;
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();

            var vm = table.GetVM();
            vm.ShowAll = showAll2;
            vm.ShowDelta = showDelta2;
            vm.ShowControls = showControls2;
            vm.ExemptTransfers = exemptTransfers2;
            return View(vm);
        }

        public IActionResult Chart(string provider, string account, bool exemptTransfers = false)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();
            var vm = new ChartWidgetViewModel(provider, account, ChartKind.Linear, _objectRepository, table.GetVM(), exemptTransfers);

            return View(vm);
        }

        public IActionResult Burst(string provider, string account, bool exemptTransfers = false)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();
            var vm = new BurstWidgetViewModel(provider, account, _objectRepository, table.GetVM(), exemptTransfers);

            return View(vm);
        }

        public IActionResult DeleteMoney(Guid id)
        {
            _objectRepository.Remove<MoneyStateModel>(x => x.Id == id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CopyFromPrevious(Guid headerid, string date)
        {
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

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
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

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