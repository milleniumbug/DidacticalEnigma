﻿using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Utils;

namespace DidacticalEnigma.Models.DataSources
{
    public class CustomNotesDataSource : IDataSource
    {
        private readonly string path;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "Custom notes",
            "The information in this source is provided by you, dear user.",
            new Uri("https://nlp.stanford.edu/projects/jesc/"));

        public void Dispose()
        {

        }

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                try
                {
                    var rich = new RichFormatting();
                    var paragraphs = ReadParagraphs(path);
                    if(paragraphs != null)
                    {
                        await paragraphs.Where(paragraph => paragraph.Contains(request.QueryText)).ForEachAsync(paragraph =>
                        {
                            var text = new TextParagraph(
                                StringExt.HighlightWords(paragraph, request.QueryText)
                                    .Select(p => new Text(p.text, emphasis: p.highlight)));
                            rich.Paragraphs.Add(text);
                        });
                        await yield.ReturnAsync(rich);
                    }
                }
                catch(FileNotFoundException)
                {
                    var text = "This data source looks for a custom_notes.txt file in the data directory. Currently no such file exists.";
                    var rich = new RichFormatting(
                        EnumerableExt.OfSingle(
                            new TextParagraph(
                                EnumerableExt.OfSingle(
                                    new Text(text)))));
                    await yield.ReturnAsync(rich);
                }
            });
        }

        public IAsyncEnumerable<string> ReadParagraphs(string path)
        {
            return new AsyncEnumerable<string>(async yield =>
            {
                using(var reader = new StreamReader(path))
                {
                    var sb = new StringBuilder();
                    string line;
                    while((line = await reader.ReadLineAsync()) != null)
                    {
                        if(line == "" && sb.Length != 0)
                        {
                            await yield.ReturnAsync(sb.ToString());
                            sb.Clear();
                            continue;
                        }

                        sb.AppendLine(line);
                    }

                    if (sb.Length != 0)
                    {
                        await yield.ReturnAsync(sb.ToString());
                    }
                }
            });
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public CustomNotesDataSource(string dataDirectory)
        {
            this.path = Path.Combine(dataDirectory, "custom_notes.txt");
        }
    }
}