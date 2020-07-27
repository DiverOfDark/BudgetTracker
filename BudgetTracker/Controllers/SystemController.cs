using System.Threading.Tasks;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [AjaxOnlyActions, Authorize]
    public class SystemController : Controller
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;

        public SystemController(UpdateService updateService, ObjectRepository objectRepository)
        {
            _updateService = updateService;
            _objectRepository = objectRepository;
        }
        
        public async Task<SystemInfo> SiteInfo()
        {
            return await SystemInfo.Create(_updateService, _objectRepository);
        } 
    }
}