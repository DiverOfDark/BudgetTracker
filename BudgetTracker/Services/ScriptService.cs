using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BudgetTracker.Controllers
{
    public class ScriptService
    {
        private readonly ObjectRepository _objectRepository;
        private readonly ScriptOptions _options;

        public ScriptService(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
            var start = typeof(Startup).GetTypeInfo().Assembly;

            var referenced = start.GetReferencedAssemblies().Select(Assembly.Load).Concat(new[] { start });

            _options = ScriptOptions.Default
                .AddReferences(referenced.ToArray())
                .AddImports("System.Linq")
                .AddImports("BudgetTracker")
                .AddImports("BudgetTracker.Model");
        }

        public Task<object> Evaluate(string script)
        {
            return CSharpScript.EvaluateAsync(script, _options, _objectRepository, typeof(ObjectRepository));
        }

        public async Task<ExecuteScriptResponse> Evaluate(ExecuteScriptRequest request)
        {
            var result = new ExecuteScriptResponse();
            try
            {
                var response = await Evaluate(request.Script ?? string.Empty);
                result.Result = response != null ? SerializeObject(response, 2) : "Ok";
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }

            return result;
        }

        private string SerializeObject(object obj, int maxDepth)
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

        private class CustomJsonTextWriter : JsonTextWriter
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

        private class CustomContractResolver : DefaultContractResolver
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