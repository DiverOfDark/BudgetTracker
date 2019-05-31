using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BudgetTracker.JsModel;
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

        public IEnumerable<MoneyColumnMetadataJsModel> IndexJson()
        {
            var models = _objectRepository.Set<MoneyColumnMetadataModel>().SortColumns().Select(v=>new MoneyColumnMetadataJsModel(_objectRepository, v)).ToList();
            return models;
        }

        [HttpPost]
        public OkResult MetadataDelete(Guid id)
        {
            _objectRepository.Remove<MoneyColumnMetadataModel>(x => x.Id == id);
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

            existingModel.Function = function;
            existingModel.UserFriendlyName = userFriendlyName;
            existingModel.AutogenerateStatements = autogenerateStatements;

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