using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class MetadataController : Controller
    {
        private readonly ObjectRepository _objectRepository;
        
        public MetadataController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IActionResult Index()
        {
            var models = _objectRepository.Set<MoneyColumnMetadataModel>().SortColumns().ToList();
            return View(models);
        }

        [HttpGet]
        public IActionResult MetadataDelete(Guid id)
        {
            _objectRepository.Remove<MoneyColumnMetadataModel>(x => x.Id == id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult MetadataEdit(Guid id)
        {
            var model = _objectRepository.Set<MoneyColumnMetadataModel>().FirstOrDefault(v => v.Id == id);
            return View(model);
        }

        [HttpPost]
        public IActionResult MetadataEdit(Guid id, string userFriendlyName, string function, bool autogenerateStatements)
        {
            MoneyColumnMetadataModel existingModel;

            if (id == Guid.Empty)
            {
                existingModel = new MoneyColumnMetadataModel(MoneyColumnMetadataModel.ComputedProdiver, null);
                _objectRepository.Add(existingModel);
            }
            else
            {
                existingModel = _objectRepository.Set<MoneyColumnMetadataModel>().First(v => v.Id == id);
            }

            existingModel.Function = function;
            existingModel.UserFriendlyName = userFriendlyName;
            existingModel.AutogenerateStatements = autogenerateStatements;

            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public IActionResult ComputedAutocomplete()
        {
            var variants = new List<string>();
            try
            {
                string bodyStr;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = reader.ReadToEnd();
                }

                var content = JObject.Parse(bodyStr);

                var term = content["term"].Value<string>();

                var lastIndex = term.LastIndexOf(']') + 1;

                lastIndex = term.LastIndexOf('[', term.Length - 1, term.Length - lastIndex);

                if (lastIndex == -1)
                    return Json(variants);

                var searchPart = term.Substring(lastIndex + 1);

                var possibleItems = _objectRepository.Set<MoneyStateModel>()
                    .Select(v => $"[{v.Provider}/{v.AccountName}]")
                    .Distinct().Concat(_objectRepository.Set<MoneyColumnMetadataModel>().Where(v=>v.IsComputed)
                        .Select(v => $"[{v.UserFriendlyName}]"))
                    .ToList();

                var matched = possibleItems.Where(v => v.Contains(searchPart) && !term.Contains(v));

                variants = matched.Select(v => term.Substring(0, lastIndex) + v).OrderBy(v=>v).ToList();
            }
            catch { }

            return Json(variants);
        }

        public IActionResult UpdateColumnOrder(Guid id, bool moveUp)
        {
            var models = _objectRepository.Set<MoneyColumnMetadataModel>().SortColumns().ToList();

            var lastId = models.FirstOrDefault()?.Order;
            foreach (var m in models.Skip(1))
            {
                if (m.Order <= (lastId ?? 0))
                {
                    m.Order = (lastId ?? 0) + 1;
                }

                lastId = m.Order;
            }

            var model = models.First(v => v.Id == id);

            var order = model.Order + (moveUp ? -1 : +1);

            var oldModel = models.First(v => v.Order == order);

            var oldOrder = model.Order;
            model.Order = order;
            oldModel.Order = oldOrder;

            return RedirectToAction(nameof(Index));
        }
    }
}