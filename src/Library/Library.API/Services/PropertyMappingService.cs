using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> authroPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string> { "Id" }) },
                { "Genre", new PropertyMappingValue(new List<string> { "Genre" }) },
                { "Age", new PropertyMappingValue(new List<string> { "DateOfBirth" }, true) },
                { "Name", new PropertyMappingValue(new List<string> { "FirstName", "LastName" }) },
            };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(authroPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestionation>()
        {
            var matchMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestionation>>();

            if (matchMapping.Count() == 1)
            {
                return matchMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestionation)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is sperated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields
                // are comming from an orderBy string, this part must be ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
