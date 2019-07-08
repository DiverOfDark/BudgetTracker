using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetTracker.Controllers.ViewModels;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class WidgetController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public WidgetController(ObjectRepository objectRepository) => _objectRepository = objectRepository;

        public DashboardViewModel Index(int? period = 0)
        {
            var period2 = this.TryGetLastValue(period, nameof(WidgetController) + nameof(period)) ?? 1;
            
            return new DashboardViewModel(_objectRepository, period2, HttpContext.RequestServices.GetRequiredService<TableViewModelFactory>());
        }

        public OkResult DeleteWidget(Guid id)
        {
            _objectRepository.Remove<WidgetModel>(v=>v.Id == id);
            return Ok();
        }

        [HttpGet]
        public EditWidgetViewModel EditWidget(Guid id)
        {
            var widget = _objectRepository.Set<WidgetModel>().First(v => v.Id == id);
            return new EditWidgetViewModel(widget);
        }

        [HttpPost]
        public OkResult EditWidget(EditWidgetViewModel editWidgetViewModel)
        {
            WidgetModel existingModel;

            if (editWidgetViewModel.Id == Guid.Empty)
            {
                var maxOrder = _objectRepository.Set<WidgetModel>().Count();
                existingModel = new WidgetModel(maxOrder, editWidgetViewModel.Title, editWidgetViewModel.Kind);
                _objectRepository.Add(existingModel);
            }
            else
            {
                existingModel = _objectRepository.Set<WidgetModel>().First(v => v.Id == editWidgetViewModel.Id);
            }

            editWidgetViewModel.Id = existingModel.Id;
            
            existingModel.Kind = editWidgetViewModel.Kind;
            existingModel.Title = editWidgetViewModel.Title;
            existingModel.Properties = new ReadOnlyDictionary<string, string>(editWidgetViewModel.Properties);
            
            return Ok();
        }

        public OkResult MoveWidgetLeft(Guid id)
        {
            var widgets = _objectRepository.Set<WidgetModel>().OrderBy(v=>v.Order).ToList();
            var matchingWidget = widgets.First(v => v.Id == id);

            var oldIndex = widgets.IndexOf(matchingWidget);
            var oldWidget = widgets[oldIndex - 1];
            widgets[oldIndex - 1] = matchingWidget;
            widgets[oldIndex] = oldWidget;
            
            foreach (var w in widgets)
            {
                w.Order = widgets.IndexOf(w);
            }

            return Ok();
        }

        public OkResult MoveWidgetRight(Guid id)
        {
            var widgets = _objectRepository.Set<WidgetModel>().OrderBy(v=>v.Order).ToList();
            var matchingWidget = widgets.First(v => v.Id == id);

            var oldIndex = widgets.IndexOf(matchingWidget);
            var oldWidget = widgets[oldIndex + 1];
            widgets[oldIndex + 1] = matchingWidget;
            widgets[oldIndex] = oldWidget;
            
            foreach (var w in widgets)
            {
                w.Order = widgets.IndexOf(w);
            }

            return Ok();
        }
    }
}
