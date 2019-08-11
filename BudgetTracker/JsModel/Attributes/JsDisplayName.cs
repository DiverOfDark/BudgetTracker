using System;

namespace BudgetTracker.JsModel.Attributes
{
    public class JsDisplayNameAttribute: Attribute
    {
        public string Name { get; }

        public JsDisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}