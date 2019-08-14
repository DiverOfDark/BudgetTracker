using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                    return SerializeObject(result,1);
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
        
        private static string SerializeObject(object obj, int maxDepth)
        {
            using (var strWriter = new StringWriter())
            {
                using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                {
                    var resolver = new CustomContractResolver(() => jsonWriter.CurrentDepth <= maxDepth);
                    var serializer = new JsonSerializer {ContractResolver = resolver, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented};
                    serializer.Serialize(jsonWriter, obj);
                }
                return strWriter.ToString();
            }
        }
        
        public class CustomJsonTextWriter : JsonTextWriter
        {
            public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter) {}

            public int CurrentDepth { get; private set; }

            public override void WriteStartObject()
            {
                CurrentDepth++;
                base.WriteStartObject();
            }

            public override void WriteEndObject()
            {
                CurrentDepth--;
                base.WriteEndObject();
            }
        }
        
        public class CustomContractResolver : DefaultContractResolver
        {
            private readonly Func<bool> _includeProperty;

            public CustomContractResolver(Func<bool> includeProperty)
            {
                _includeProperty = includeProperty;
            }

            protected override JsonProperty CreateProperty(
                MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                var shouldSerialize = property.ShouldSerialize;
                property.ShouldSerialize = obj => _includeProperty() &&
                                                  (shouldSerialize == null ||
                                                   shouldSerialize(obj));
                return property;
            }
        }
    }
}