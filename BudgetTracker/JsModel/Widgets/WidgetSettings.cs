using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    [ExportJsModel]
    public abstract class WidgetSettings
    {
        private readonly Dictionary<string, string> _model;

        public WidgetSettings(Dictionary<string,string> model)
        {
            _model = model;
        }

        public String Kind => GetType().Name;
        
        protected string GetPropertyFromModel([CallerMemberName] string propertyName = null)
        {
            _model.TryGetValue(propertyName, out var value);
            return value;
        }

        protected T GetEnumPropertyFromModel<T>([CallerMemberName] string propertyName = null)
        {
            if (Enum.TryParse(typeof(T), GetPropertyFromModel(propertyName), out var enumval))
            {
                return (T) enumval;
            }

            return default;
        }

        protected void SetPropertyFromModel(string value, [CallerMemberName] string propertyName = null) => _model[propertyName] = value;

        protected void SetEnumPropertyFromModel<T>(T value, [CallerMemberName] string propertyName = null) => SetPropertyFromModel(value.ToString(), propertyName);
        
        
        public IEnumerable<string> GetProviders(ObjectRepository objectRepository) => objectRepository.Set<MoneyColumnMetadataModel>()
            .Select(v => v.Provider).Distinct()
            .OrderBy(v => v)
            .ToList();

        public IEnumerable<string> GetAccounts(ObjectRepository objectRepository, string providerName) => objectRepository.Set<MoneyColumnMetadataModel>()
            .Where(v => v.Provider == providerName)
            .Select(v => v.UserFriendlyName)
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }
}