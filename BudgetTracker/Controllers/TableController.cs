using System;
using System.Globalization;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                    bool toRemove = header.IsComputed || vm.Values.All(s => s.CalculatedCells[header].IsOk);

                    if (toRemove)
                    {
                        vm.Headers.Remove(header);
                        foreach (var row in vm.Values)
                        {
                            row.CalculatedCells.Remove(header);
                        }
                    }
                }

                var sortedRows = vm.Values.OrderBy(v=>v.When).ToList();
                for (int i = 0; i < sortedRows.Count - 2; i++)
                {
                    var yesterdayRow = sortedRows[i];
                    var todayRow = sortedRows[i+1];

                    if (todayRow.CalculatedCells.Values.All(s => s.IsOk) && yesterdayRow.CalculatedCells.Values.All(t => t.IsOk))
                    {
                        vm.Values.Remove(yesterdayRow);
                    }
                }
            }
            else
            {
                foreach (var item in Enumerable.Reverse(vm.Headers).ToList())
                {
                    if (item.Provider != provider2)
                    {
                        vm.Headers.Remove(item);
                        foreach (var row in vm.Values)
                        {
                            row.CalculatedCells.Remove(item);
                        }
                    }
                }
            }

            return new TableJsModel(vm, provider2, providers);
        }

        public LinearChartWidgetViewModel Chart(string provider, string account, bool exemptTransfers = false)
        {
            var table = HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>();
            var vm = new LinearChartWidgetViewModel(provider, account, table.GetVM(), exemptTransfers);

            return vm;
        }

        public OkResult DeleteMoney(Guid id)
        {
            _objectRepository.Remove<MoneyStateModel>(x => x.Id == id);
            return Ok();
        }

        public OkResult CopyFromPrevious(Guid headerid, string date)
        {
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

            var column = _objectRepository.Set<MoneyColumnMetadataModel>().First(v => v.Id == headerid);

            var previous = _objectRepository.Set<MoneyStateModel>().Where(v => v.Column == column && v.When < realDate)
                .OrderByDescending(v => v.When).First();
            
            _objectRepository.Add(new MoneyStateModel
            {
                Column = column,
                Amount = previous.Amount,
                When = realDate,
                Ccy = previous.Ccy
            });

            return Ok();
        }

        public OkResult MarkAsOk(Guid headerid, string date)
        {
            var realDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

            var column = _objectRepository.Set<MoneyColumnMetadataModel>().First(v => v.Id == headerid);

            _objectRepository.Add(new MoneyStateModel
            {
                Column = column,
                Amount = double.NaN,
                When = realDate
            });

            return Ok();
        }
    }
}