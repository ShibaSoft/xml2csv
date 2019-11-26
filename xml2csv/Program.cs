using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace xml2csv
{
    class Program
    {
        static Config CONFIG { get; set; }

        static void Main(string[] args)
        {
            CONFIG = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            IEnumerable<FileInfo> files = GetFilesRecursive(CONFIG.BasePath);

            File.Create(CONFIG.OutputFile)
                .Close();

            File.WriteAllText(CONFIG.OutputFile, string.Join(",", CONFIG.Elements.Select(e => e.ColumnName)) + "\n");

            foreach (var f in files)
            {
                var elementMatches = XmlGrabberString(f);
                var line = string.Join(",", elementMatches.Select(s => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Replace(",", ";").Replace("\n", "").Replace("\r", "").Replace("\t", "")));

                File.AppendAllText(CONFIG.OutputFile, line + "\n");
                Console.WriteLine(elementMatches?.FirstOrDefault());
                elementMatches = null;
            }
        }

        public static IEnumerable<FileInfo> GetFilesRecursive(string path)
        {
            var files = Directory.GetFiles(path).ToList().Select(f => new FileInfo(f));

            foreach (var subpath in Directory.GetDirectories(path))
                files = files.Concat(GetFilesRecursive(subpath));

            return files;
        }

        public static IEnumerable<string> XmlGrabberString(FileInfo file)
        {
            var xml = XElement.Load(file.FullName);
            IEnumerable<string> retVal = new List<string>();

            foreach(var element in CONFIG.Elements)
            {
                switch (element.MatchType)
                {
                    case MatchType.ElementValue:
                        retVal = retVal.Append(string.Join(";;", xml.DescendantsAndSelf(TranslateNamespace(element.ElementName)).Select(x => x.Value)));
                        break;
                    case MatchType.ElementAttribute:
                        retVal = retVal.Append(string.Join(";;", xml.DescendantsAndSelf(TranslateNamespace(element.ElementName)).Select(e => e.Attribute(TranslateNamespace(element.AttributeName)).Value)));
                        break;
                    case MatchType.ChildElementValue:
                        retVal = retVal.Append(string.Join(";;", xml.DescendantsAndSelf(TranslateNamespace(element.ParentElementName)).FirstOrDefault()
                            ?.Descendants(TranslateNamespace(element.ElementName))?.Select(e => e.Value)));
                        break;
                    case MatchType.ChildElementSiblingWithGivenAttribute:
                        var parentElements = xml.DescendantsAndSelf(TranslateNamespace(element.ParentElementName));
                        var parentElementsThatHaveDescendant = parentElements.Where(e => e.Descendants(TranslateNamespace(element.SiblingElementName)).Select(e => e.Attribute(TranslateNamespace(element.SiblingAttributeName))).Where(a => a.Value == element.SiblingAttributeValue).Any());
                        var childValue = parentElementsThatHaveDescendant.Select(e => e.Descendants(TranslateNamespace(element.ElementName)).Select(e => e.Value).FirstOrDefault());
                        retVal = retVal.Append(string.Join(";;", childValue));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return retVal;
        }

        public static string TranslateNamespace(string elementWithNamespaceAlias)
        {
            var splitString = elementWithNamespaceAlias.Split(':');
            return $"{{{CONFIG.Namespaces.FirstOrDefault(n => n.Alias.ToLower() == splitString.First().ToLower())?.Value}}}{splitString.Last()}";
        }
    }
}
