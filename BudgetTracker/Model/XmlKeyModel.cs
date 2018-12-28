using System;
using System.Xml.Linq;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Model
{
    public class XmlKeyModel : ModelBase
    {
        public class XmlKeyEntity : BaseEntity
        {
            public string Element { get; set; }
        }

        private readonly XmlKeyEntity _entity;

        protected override BaseEntity Entity => _entity;

        public XmlKeyModel(XmlKeyEntity entity)
        {
            _entity = entity;
        }

        public XmlKeyModel(XElement element)
        {
            _entity = new XmlKeyEntity
            {
                Id = Guid.NewGuid(),
                Element = element.ToString()
            };
        }

        public XElement Element => XElement.Parse(_entity.Element ?? "");
    }
}