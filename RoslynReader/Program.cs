using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RazorEngine;
using RazorEngine.Templating;
using Roslyn.Services;

namespace RoslynReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var solution = Solution.Load(args[0]);

            var template =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
    <head>
        <title>Foo</title>

        <link rel=""stylesheet"" type=""text/css"" href=""stylesheet.css"" />
    </head>
    <body>
        <div class=""paper"">

            <div id=""heading"">
                <h1>Foo</h1>
            </div>
            <div id=""contents"">
                <h2>Contents</h2>
                <ul>
                @foreach (var project in Model.Projects.OrderBy(p => p.Name)) {
                    <li><h4><a href=""#@project.Name"">@project.Name</a></h4>
                        <ul>
                        @foreach (var document in SortDocuments(project.Documents, 0)) {
                            <li><div id=""#toc.@CreateFullDocumentPath(""."", document)""><a href=""#@CreateFullDocumentPath(""."", document)"">@CreateFullDocumentPath("" \\ "", document)</a></div></li>
                        }
                        </ul>
                    </li>
                }
                </ul>
            </div>

            <div id=""text"">            @foreach (var project in Model.Projects.OrderBy(p => p.Name)) {                <div class=""section"" id=""@project.Name"">                    <h2>@project.Name</h2>                    @foreach (var document in SortDocuments(project.Documents, 0)) {                        <h3 id=""@CreateFullDocumentPath(""."", document)"">@CreateFullDocumentPath("" \\ "", document)</h3>                        <div class=""blockquote"">                            <pre><code>@HtmlEncode(document.GetText().GetText())</code></pre>                        </div>                        <a href=""#contents"">Back to Top</a>                    }                </div>            }            </div>

        </div>
    </body>
</html>";

            Razor.SetTemplateBase(typeof(MyCustomTemplateBase<>));

            var result = Razor.Parse(template, solution);

            File.WriteAllText("Example.html", result);
        }
    }

    public abstract class MyCustomTemplateBase<T> : TemplateBase<T>
    {
        public IEnumerable<IDocument> SortDocuments(IEnumerable<IDocument> documents, int depth)
        {
            var group = documents.Where(d => d.Folders.Skip(depth).Any()).GroupBy(d => d.Folders.Skip(depth).First()).OrderBy(g => g.Key);

            foreach (var item in group)
                foreach (var doc in SortDocuments(item, depth + 1))
                    yield return doc;

            foreach (var doc in documents.Where(d => !d.Folders.Skip(depth).Any()).OrderBy(d => d.Name))
                yield return doc;
        }

        public string CreateFullDocumentPath(string seperator, IDocument document)
        {
            return String.Join(seperator, document.Folders.Concat(new string[] { document.Name }));
        }

        public string HtmlEncode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }
    }
}