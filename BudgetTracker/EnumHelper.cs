using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BudgetTracker
{
    public static class ControllerHelper
    {
        public static T TryGetLastValue<T>(this Controller controller, T value, string key)
        {
            if (Equals(value, default(T)))
            {
                var currentValue = controller.HttpContext.Session.GetString(key);
                if (currentValue != null)
                {
                    return JsonConvert.DeserializeObject<T>(currentValue);
                }
            }
            else
            {
                controller.HttpContext.Session.SetString(key, JsonConvert.ToString(value));
            }

            return value;
        }
    }
    
    public static class EnumHelper
    {
        public static string GetDisplayName<T>(this T enumValue)
        {
            return typeof(T).GetMember(enumValue.ToString())?[0]?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? enumValue.ToString();
        }

        public static IEnumerable<SelectListItem> GetSelectList(this Type enumType, object enumValue = null)
        {
            return Enum.GetNames(enumType).Select(v =>
            {
                var attribute = enumType.GetMember(v)[0].GetCustomAttribute<DisplayNameAttribute>();

                return new SelectListItem
                {
                    Selected = Equals(enumValue?.ToString(), v),
                    Text = attribute?.DisplayName ?? v,
                    Value = v
                };
            }).ToList();
        }
    }
}