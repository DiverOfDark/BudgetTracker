using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class TableController : Controller
    {
        public const string BadOption = nameof(BadOption);

        private readonly ObjectRepository _objectRepository;

        public TableController(ObjectRepository objectRepository) => _objectRepository = objectRepository;

        public TableJsModel IndexJson(String provider)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();

            var vm = table.GetVM();

            var providers = vm.Headers.Select(v => v.Provider).Distinct().OrderBy(v => v).ToList();
            var provider2 = provider ?? vm.Headers.Select(s => s.Provider).FirstOrDefault();

            if (provider2 == BadOption)
            {
                foreach (var header in vm.Headers.ToList())
                {
                    bool toRemove = header.IsComputed || vm.Values.All(s =>
                                                          s.CalculatedCells.Any(c =>
                                                              c.Key == header && c.Value != null &&
                                                              c.Value?.FailedToResolve.Any() == false))
                                                      || vm.Values.All(s =>
                                                          s.CalculatedCells.All(c => c.Key == header && c.Value == null));

                    if (toRemove)
                    {
                        var idx = vm.Headers.IndexOf(header);
                        vm.Headers.RemoveAt(idx);
                        foreach (var row in vm.Values)
                        {
                            row.Cells.RemoveAt(idx);
                        }
                    }
                }
            }
            else
            {
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

            return new TableJsModel(vm, provider2, providers);
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
            return Ok();
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

            return Ok();
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

            return Ok();
        }
    }
}