using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using BudgetTracker.Model;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace BudgetTracker
{
    public class ObjectRepositoryXmlStorage : IXmlRepository
    {
        private readonly ObjectRepository _objectRepository;

        public ObjectRepositoryXmlStorage(ObjectRepository objectRepository) => _objectRepository = objectRepository;

        public IReadOnlyCollection<XElement> GetAllElements() => new ReadOnlyCollection<XElement>(_objectRepository.Set<XmlKeyModel>().Select(s => s.Element).ToList());

        public void StoreElement(XElement element, string friendlyName) => _objectRepository.Add(new XmlKeyModel(element));
    }
}