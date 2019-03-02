using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class UtilityController : Controller
    {
        private readonly Chrome _chrome;
        private readonly ScriptService _scriptService;
        private byte[] _latestBytes = new byte[0];

        public UtilityController(Chrome chrome, ScriptService scriptService)
        {
            _chrome = chrome;
            _scriptService = scriptService;
        }

        
        public ActionResult Screenshot() => View();

        public ActionResult ScreenshotFile()
        {
            try
            {
                _latestBytes = _chrome.Driver.GetScreenshot().AsByteArray;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            return File(_latestBytes, "image/png");
        }

        public async Task<IActionResult> ScriptConsole(string script)
        {
            script = script ?? string.Empty;
            ViewData["Script"] = script;

            try
            {
                var result = await _scriptService.Evaluate(script);

                if (result != null)
                {
                    ViewData["ScriptResult"] = result.GetPropertiesAsRawData();
                }
            }
            catch (Exception ex)
            {
                ViewData["ScriptResult"] = ex.ToString();
            }

            return View();
        }
        
        public IActionResult DownloadDump()
        {
            using (var fs = new FileStream(Startup.DbFileName, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
            using (var reader = new BinaryReader(fs))
            {
                var contents = reader.ReadBytes((int) fs.Length);
                return File(contents, "application/octet-stream", Path.GetFileName(Startup.DbFileName));
            }
        }

        public IActionResult Tasks() => View();
    }
}