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
    // TODO: Implement inheritors as dynamic controllers as described here: https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/
    // NOTE: need to think how code generation for TS would work with dynamic controllers.
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

    [Obsolete]
    public class MoneyColumnMetadataModelController : ObjectRepositoryControllerBase<MoneyColumnMetadataModel,
            MoneyColumnMetadataJsModel>
    {
        public MoneyColumnMetadataModelController(ObjectRepository objectRepository) : base(objectRepository, x => new MoneyColumnMetadataJsModel(x))
        {
        }
    }

    [Obsolete]
    public class SpentCategoryModelController : ObjectRepositoryControllerBase<SpentCategoryModel, SpentCategoryJsModel>
    {
        public SpentCategoryModelController(ObjectRepository objectRepository) : base(objectRepository,
            v => new SpentCategoryJsModel(v))
        {
        }
    }

    [Obsolete]
    public class DebtModelController : ObjectRepositoryControllerBase<DebtModel, DebtJsViewModel>
    {
        public DebtModelController(ObjectRepository objectRepository) : base(objectRepository,
            v => new DebtJsViewModel(v))
        {
        }
    }

    [Obsolete]
    public class PaymentViewModelController : ObjectRepositoryControllerBase<PaymentModel, PaymentViewModel>
    {
        public PaymentViewModelController(ObjectRepository objectRepository) : base(objectRepository,
            @from => new PaymentViewModel(from))
        {
        }
    }

    [Obsolete]
    public class WidgetViewModelController : ObjectRepositoryControllerBase<WidgetModel, WidgetJsViewModel>
    {
        public WidgetViewModelController(ObjectRepository repository) : base(repository, from => new WidgetJsViewModel(from))
        {
        }
    }
}