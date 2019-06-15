using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Payment;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Controllers
{
    [AjaxOnlyActions, Authorize]
    public abstract class ObjectRepositoryControllerBase<TModel, TViewModel>: Controller where TModel:ModelBase where TViewModel:class
    {
        private readonly ObjectRepository _objectRepository;
        private readonly Func<TModel, TViewModel> _convert;

        protected ObjectRepositoryControllerBase(ObjectRepository objectRepository, Func<TModel, TViewModel> convert)
        {
            _convert = convert;
            _objectRepository = objectRepository;
        }

        public IEnumerable<TViewModel> List() => _objectRepository.Set<TModel>().Select(_convert).ToList();

        public TViewModel Find(Guid id)
        {
            var obj = _objectRepository.Set<TModel>().Find(id);
            return obj == null ? null : _convert(obj);
        }

        public OkResult Delete(Guid id)
        {
            var set = _objectRepository.Set<TModel>();
            var found = set.Find(id);
            _objectRepository.Remove(found);
            return Ok();
        }
    }

    public class MoneyColumnMetadataModelController : ObjectRepositoryControllerBase<MoneyColumnMetadataModel,
            MoneyColumnMetadataJsModel>
    {
        public MoneyColumnMetadataModelController(ObjectRepository objectRepository) : base(objectRepository, x => new MoneyColumnMetadataJsModel(objectRepository, x))
        {
        }
    }

    public class SpentCategoryModelController : ObjectRepositoryControllerBase<SpentCategoryModel, SpentCategoryJsModel>
    {
        public SpentCategoryModelController(ObjectRepository objectRepository) : base(objectRepository,
            v => new SpentCategoryJsModel(v))
        {
        }
    }

    public class DebtModelController : ObjectRepositoryControllerBase<DebtModel, DebtJsViewModel>
    {
        public DebtModelController(ObjectRepository objectRepository) : base(objectRepository,
            v => new DebtJsViewModel(v))
        {
        }
    }

    public class PaymentViewModelController : ObjectRepositoryControllerBase<PaymentModel, PaymentViewModel>
    {
        public PaymentViewModelController(ObjectRepository objectRepository) : base(objectRepository,
            @from => new PaymentViewModel(from))
        {
        }
    }
}