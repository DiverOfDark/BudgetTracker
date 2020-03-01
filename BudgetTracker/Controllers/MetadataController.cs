using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class MetadataController : Controller
    {
        private readonly ObjectRepository _objectRepository;
        
        public MetadataController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IEnumerable<MoneyColumnMetadataJsModel> IndexJson()
        {
            var models = _objectRepository.Set<MoneyColumnMetadataModel>().SortColumns().Select(v=>new MoneyColumnMetadataJsModel(v)).ToList();
            return models;
        }

        [HttpPost]
        public OkResult MetadataDelete(Guid id)
        {
            var mcmm = _objectRepository.Set<MoneyColumnMetadataModel>().Find(id);

            _objectRepository.Remove<MoneyStateModel>(v => v.Column == mcmm);
            _objectRepository.Remove<PaymentModel>(v=>v.Column == mcmm);
            _objectRepository.Remove(mcmm);
            return Ok();
        }

        [HttpPost]
        public OkResult MetadataEdit(Guid id, string userFriendlyName, string function, bool autogenerateStatements)
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

            existingModel.Function = function ?? "";
            existingModel.UserFriendlyName = userFriendlyName;
            existingModel.AutogenerateStatements = autogenerateStatements;

            _objectRepository.Remove<MoneyStateModel>(v => v.Column.IsComputed && v.When.Date == DateTime.Today.Date);

            return Ok();
        }

        public OkResult UpdateColumnOrder(Guid id, bool moveUp)
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

            return Ok();
        }
    }
}