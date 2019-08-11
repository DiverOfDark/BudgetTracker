using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace BudgetTracker.Controllers
{
    public class ScriptService
    {
        private readonly ObjectRepository _objectRepository;
        private readonly Dictionary<string, ScriptRunner<object>> _cachedScripts;
        private readonly ScriptOptions _options;

        public ScriptService(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
            _cachedScripts = new Dictionary<string, ScriptRunner<object>>();
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

        public async Task<object> EvaluateCached(string script)
        {
            if (!_cachedScripts.TryGetValue(script, out var scriptObj))
            {
                _cachedScripts[script] = scriptObj = CSharpScript.Create(script, _options, typeof(ObjectRepository)).CreateDelegate();
            }

            return await scriptObj.Invoke(_objectRepository);
        }
    }
}