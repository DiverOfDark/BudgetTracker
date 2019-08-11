using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class UtilityController : Controller
    {
        private readonly Chrome _chrome;
        private readonly ScriptService _scriptService;
        private readonly ILogger<UtilityController> _logger;
        private byte[] _latestBytes = new byte[0];

        public UtilityController(Chrome chrome, ScriptService scriptService, ILogger<UtilityController> logger)
        {
            _chrome = chrome;
            _scriptService = scriptService;
            _logger = logger;
        }

        
        public ActionResult ScreenshotFile()
        {
            try
            {
                _latestBytes = _chrome.Driver.GetScreenshot().AsByteArray;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get screenshot", ex);
            }

            return File(_latestBytes, "image/png");
        }

        [HttpPost]
        public async Task<string> ScriptConsole(string script)
        {
            script = script ?? string.Empty;

            try
            {
                var result = await _scriptService.Evaluate(script);

                if (result != null)
                {
                    return result.GetPropertiesAsRawData();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "Ok";
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
    }
}