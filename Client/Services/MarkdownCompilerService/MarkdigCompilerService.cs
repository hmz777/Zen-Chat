using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using System.IO;

namespace MVCBlazorChatApp.Client.Services.MarkdownCompilerService
{
    public class MarkdigCompilerService : IMarkdownCompilerService
    {
        private MarkdownPipeline MarkdownPipeline { get; set; }

        public MarkdigCompilerService(MarkdownPipeline markdownPipeline)
        {
            this.MarkdownPipeline = markdownPipeline;
        }

        public string CompileMarkdown(string Text)
        {
            using StringWriter stringWriter = new StringWriter();

            HtmlRenderer htmlRenderer = new HtmlRenderer(stringWriter);
            MarkdownPipeline.Setup(htmlRenderer);

            MarkdownDocument Document = Markdown.Parse(Text, MarkdownPipeline);

            htmlRenderer.Render(Document);
            stringWriter.Flush();

            return stringWriter.ToString();
        }
    }
}
