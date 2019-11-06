namespace APM.BotCore.Dialogs
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class RootDialog : ComponentDialog
    {
        private const string HelpChoice = "HelpOption";
        private const string APMChoice = "APMChoice";
        private const string ResetChoice = "ResetChoice";
        private static List<Choice> APMOptions = new List<Choice>()
        {
            new Choice(HelpChoice) { Synonyms = new List<string> { "help" } },
            new Choice(APMChoice) { Synonyms = new List<string> { "azure", "code", "pass", "trial", "Azure trial code"}},
            new Choice(ResetChoice) { Synonyms = new List<string> { "reset", "reset code" }}
        };

        public RootDialog(APMHelper aPMHelper) : base(nameof(RootDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                PromptForUserChoice,
                ShowChildDialogAsync,
                ResumeAfterAsync
            }));
            AddDialog(new HelpDialog());
            AddDialog(new APMDialog(aPMHelper));
            AddDialog(new ResetDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }

        public async Task<DialogTurnResult> PromptForUserChoice(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await waterfallStepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Choices = APMOptions,
                    Prompt = MessageFactory.Text("Make a choice to get started"),
                    RetryPrompt = MessageFactory.Text("Choice not recognized. Make a choice to get started")
                },
                cancellationToken);
        }

        public async Task<DialogTurnResult> ShowChildDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var optionSelected = (stepContext.Result as FoundChoice).Value;

            switch (optionSelected)
            {
                case HelpChoice:
                    return await stepContext.BeginDialogAsync(
                        nameof(HelpDialog),
                        cancellationToken);
                case APMChoice:
                    return await stepContext.BeginDialogAsync(
                        nameof(APMDialog),
                        cancellationToken);
                case ResetChoice:
                    return await stepContext.BeginDialogAsync(
                        nameof(ResetDialog),
                        cancellationToken);
            }

            // We shouldn't get here, but fail gracefully if we do.
            await stepContext.Context.SendActivityAsync(
                "I don't recognize that option.",
                cancellationToken: cancellationToken);
            // Continue through to the next step without starting a child dialog.
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ResumeAfterAsync(
                WaterfallStepContext stepContext,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var message = stepContext.Context.Activity;

                var ticketNumber = new Random().Next(0, 20000);
                await stepContext.Context.SendActivityAsync(
                    $"Thank you for using the Helpdesk Bot. Your ticket number is {ticketNumber}.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(
                    $"Failed with message: {ex.Message}",
                    cancellationToken: cancellationToken);
                //logger.Error(ex);
            }

            // Replace on the stack the current instance of the waterfall with a new instance,
            // and start from the top.
            return await stepContext.ReplaceDialogAsync(
                nameof(WaterfallDialog),
                cancellationToken: cancellationToken);
        }
    }
}