using System;
using System.Collections.Generic;
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
        public const string BadOption = nameof(BadOption);

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
                    item.CalculatedCells.TryGetValue(h, out var p); 

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
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexJson(String provider)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();

            var vm = table.GetVM();

            var providers = vm.Headers.Select(v => v.Provider).Distinct().OrderBy(v => v).ToList();
            var provider2 = provider ?? vm.Headers.Select(s => s.Provider).FirstOrDefault();

            if (provider2 == BadOption)
            {
                vm.Values.RemoveAll(v => v.CalculatedCells.All(s => s.Value != null && s.Value?.FailedToResolve.Any() == false));
/*                vm.Headers.RemoveAll(v => v.IsComputed || vm.Values.All(s =>
                                              s.CalculatedCells.Any(c =>
                                                  c.Key == v && c.Value != null &&
                                                  c.Value?.FailedToResolve.Any() == false)));*/
            }
            else
            {
                foreach (var calculatedResult in vm.Values.SelectMany(v=>v.Cells).Where(v=> v != null && v.Column.Provider == provider2).OfType<ExpressionCalculatedResult>())
                {
                    calculatedResult.EvalExpression();
                }

                foreach (var item in Enumerable.Reverse(vm.Headers).ToList())
                {
                    if (item.Provider != provider2)
                    {
                        var idx = vm.Headers.IndexOf(item);
                        vm.Headers.RemoveAt(idx);
                        foreach (var row in vm.Values)
                        {
                            row.Cells.RemoveAt(idx);
                        }
                    }
                }
            }

            return Json(new
            {
                vm,
                Provider = provider2,
                Providers = providers
            });
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