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
    public abstract class ObjectRepositoryControllerBase<T, U>: Controller where T:ModelBase
    {
        private readonly ObjectRepository _objectRepository;
        private readonly Func<T, U> _convert;

        protected ObjectRepositoryControllerBase(ObjectRepository objectRepository, Func<T, U> convert)
        {
            _convert = convert;
            _objectRepository = objectRepository;
        }

        public IEnumerable<U> List() => _objectRepository.Set<T>().Select(_convert).ToList();

        public U Find(Guid id) => _convert(_objectRepository.Set<T>().Find(id));
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