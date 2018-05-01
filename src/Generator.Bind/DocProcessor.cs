using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Bind.Structures;

namespace Bind
{
    internal class DocProcessor
    {
        private static readonly char[] Numbers = "0123456789".ToCharArray();

        private static readonly Regex RemoveMathml = new Regex(
            @"<(mml:math|inlineequation)[^>]*?>(?:.|\n)*?</\s*\1\s*>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex RemoveDoctype = new Regex(
            @"<!DOCTYPE[^>\[]*(\[.*\])?>", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RemoveXmlns = new Regex(
            "xmlns=\".+\"", RegexOptions.Compiled);

        private static readonly char[] Newline = {'\n'};

        private readonly Dictionary<string, Documentation> _documentationCache =
            new Dictionary<string, Documentation>();

        private readonly Dictionary<string, string> _documentationFiles =
            new Dictionary<string, string>();

        private Documentation _cached;
        private string _lastFile;

        public DocProcessor(IBind generator)
        {
            Generator = generator ?? throw new ArgumentNullException();
            foreach (var file in Directory.GetFiles(Settings.DocPath).Concat(
                Directory.GetFiles(Settings.FallbackDocPath)))
            {
                var name = Path.GetFileName(file);
                if (!_documentationFiles.ContainsKey(name))
                {
                    _documentationFiles.Add(name, file);
                }
            }
        }

        private IBind Generator { get; }
        private Settings Settings => Generator.Settings;

        public Documentation Process(Function f, EnumProcessor processor)
        {
            Documentation docs = null;

            if (_documentationCache.ContainsKey(f.WrappedDelegate.Name))
            {
                return _documentationCache[f.WrappedDelegate.Name];
            }

            var file = Settings.FunctionPrefix + f.WrappedDelegate.Name + ".xml";
            if (!_documentationFiles.ContainsKey(file))
            {
                file = Settings.FunctionPrefix + f.TrimmedName + ".xml";
            }

            if (!_documentationFiles.ContainsKey(file))
            {
                file = Settings.FunctionPrefix + f.TrimmedName.TrimEnd(Numbers) + ".xml";
            }

            docs =
                (_documentationFiles.ContainsKey(file) ? ProcessFile(_documentationFiles[file], processor) : null) ??
                new Documentation
                {
                    Summary = string.Empty,
                    Parameters = f.Parameters.Select(p =>
                        new DocumentationParameter(p.Name, string.Empty)).ToList()
                };

            _documentationCache.Add(f.WrappedDelegate.Name, docs);

            return docs;
        }

        // Strips MathML tags from the source and replaces the equations with the content
        // found in the <!-- eqn: :--> comments in the docs.
        // Todo: Some simple MathML tags do not include comments, find a solution.
        // Todo: Some files include more than 1 function - find a way to map these extra functions.
        private Documentation ProcessFile(string file, EnumProcessor processor)
        {
            string text;

            if (_lastFile == file)
            {
                return _cached;
            }

            _lastFile = file;
            text = File.ReadAllText(file);

            text = text
                .Replace("&epsi;", "epsilon") // Fix unrecognized &epsi; entities
                .Replace("xml:", string.Empty); // Remove namespaces
            text = RemoveDoctype.Replace(text, string.Empty);
            text = RemoveXmlns.Replace(text, string.Empty);

            var m = RemoveMathml.Match(text);
            while (m.Length > 0)
            {
                var removed = text.Substring(m.Index, m.Length);
                text = text.Remove(m.Index, m.Length);
                var equation = removed.IndexOf("eqn");
                if (equation > 0)
                {
                    // Find the start and end of the equation string
                    var eqnStart = equation + 4;
                    var eqnEnd = removed.IndexOf(":-->") - equation - 4;
                    if (eqnEnd < 0)
                    {
                        // Note: a few docs from man4 delimit eqn end with ": -->"
                        eqnEnd = removed.IndexOf(": -->") - equation - 4;
                    }

                    if (eqnEnd < 0)
                    {
                        Console.WriteLine("[Warning] Failed to find equation for mml.");
                        goto next;
                    }

                    var eqnSubstring = removed.Substring(eqnStart, eqnEnd);
                    text = text.Insert(m.Index, "<![CDATA[" + eqnSubstring + "]]>");
                }

                next:
                m = RemoveMathml.Match(text);
            }

            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(text);
                _cached = ToInlineDocs(doc, processor);
                return _cached;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(doc.ToString());
                return null;
            }
        }

        private Documentation ToInlineDocs(XDocument doc, EnumProcessor enumProcessor)
        {
            if (doc == null || enumProcessor == null)
            {
                throw new ArgumentNullException();
            }

            var noConstProcessing = Settings.Legacy.NoAdvancedEnumProcessing | Settings.Legacy.ConstIntEnums;
            if (!Generator.Settings.IsEnabled(noConstProcessing))
            {
                // Translate all GL_FOO_BAR constants according to EnumProcessor
                foreach (var e in doc.XPathSelectElements("//constant"))
                {
                    var c = e.Value;
                    if (c.StartsWith(Settings.ConstantPrefix))
                    {
                        // Remove "GL_" from the beginning of the string
                        c = c.Replace(Settings.ConstantPrefix, string.Empty);
                    }

                    e.Value = enumProcessor.TranslateConstantName(c, false);
                }
            }

            // Create inline documentation
            var inline = new Documentation
            {
                Summary =
                    Cleanup(
                        ((IEnumerable)doc.XPathEvaluate("/refentry/refnamediv/refpurpose"))
                        .Cast<XElement>().First().Value),
                Parameters =
                    ((IEnumerable)doc.XPathEvaluate(
                        "/refentry/refsect1[@id='parameters']/variablelist/varlistentry/term/parameter"))
                    .Cast<XElement>()
                    .Select(p =>
                        new DocumentationParameter(
                            p.Value.Trim(),
                            Cleanup(p.XPathSelectElement("../../listitem").Value)))
                    .ToList()
            };

            inline.Summary = char.ToUpper(inline.Summary[0]) + inline.Summary.Substring(1);
            return inline;
        }

        private static string Cleanup(string text)
        {
            return
                string.Join(" ", text
                        .Replace("\r", "\n")
                        .Split(Newline, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToArray())
                    .Trim();
        }
    }
}
