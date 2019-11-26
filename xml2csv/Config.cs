using System.Collections.Generic;

namespace xml2csv
{
    public class Config
    {
        public string BasePath { get; set; }
        public string OutputFile { get; set; }
        public IEnumerable<Namespace> Namespaces { get; set; }
        public IEnumerable<Element> Elements { get; set; }
    }

    public class Namespace
    {
        public string Alias { get; set; }
        public string Value { get; set; }
    }

    public class Element
    {
        public string ColumnName { get; set; }
        public MatchType MatchType { get; set; }
        public string ElementName { get; set; }
        public string? AttributeName { get; set; }
        public string? ParentElementName { get; set; }
        public string? SiblingElementName { get; set; }
        public string? SiblingAttributeName { get; set; }
        public string? SiblingAttributeValue { get; set; }
    }

    public enum MatchType
    {
        ElementValue,
        ElementAttribute,
        ChildElementValue,
        ChildElementSiblingWithGivenAttribute
    }
}
