using Markdig;

namespace MVCBlazorChatApp.Client.Services.MarkdownCompilerService
{
    public class MarkdigCompilerFactory
    {
        private MarkdigCompilerService MarkupCompilerWorker { get; set; }

        public MarkdigCompilerService GetOrCreate()
        {
            if (MarkupCompilerWorker != null)
                return MarkupCompilerWorker;

            var Pipeline = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseReferralLinks("nofollow")
                .UseTaskLists()
                .UseListExtras()
                .UseMediaLinks()
                .UsePipeTables()
                .UseMathematics()
                .DisableHtml()
                .UseEmojiAndSmiley()
                .UseSmartyPants()
                .UseAbbreviations()
                .UseFigures()
                .UseSoftlineBreakAsHardlineBreak()
                .UseAdvancedExtensions()
                .Build();

            MarkupCompilerWorker = new MarkdigCompilerService(Pipeline);

            return MarkupCompilerWorker;
        }
    }
}
