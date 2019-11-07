namespace APM.BotCore.Dialogs
{
    using APM.Domain;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class RootDialog : ComponentDialog
    {
        private const string APMChoice = "Get Azure Pass";
        private const string ResetChoice = "Start Again";
        private IStatePropertyAccessor<Code> stateAccessor;
        private static List<Choice> APMOptions = new List<Choice>()
        {
            new Choice(APMChoice) { Synonyms = new List<string> { "help", "azure", "code", "pass", "trial", "Azure trial code"}},
            new Choice(ResetChoice) { Synonyms = new List<string> { "reset", "reset code" }}
        };

        public RootDialog(IAPMHelper aPMHelper, ConversationState state) : base(nameof(RootDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                PromptForUserChoice,
                ShowChildDialogAsync,
                ResumeAfterAsync
            }));
            AddDialog(new APMDialog(aPMHelper));
            AddDialog(new ResetDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.stateAccessor = state.CreateProperty<Code>("awardedCode");
        }
        
        public async Task<DialogTurnResult> PromptForUserChoice(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (waterfallStepContext.Context.Activity.Value is JObject jObj && jObj.Value<int>("x") == 13)
            {

                return await waterfallStepContext.NextAsync(new FoundChoice() { Value = APMChoice }, cancellationToken: cancellationToken);
            }
            else
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
        }

        public async Task<DialogTurnResult> ShowChildDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var optionSelected = (stepContext.Result as FoundChoice).Value;

            switch (optionSelected)
            {
                case APMChoice:
                    return await stepContext.BeginDialogAsync(
                        nameof(APMDialog),
                        await stateAccessor.GetAsync(stepContext.Context),
                        cancellationToken);
                case ResetChoice:
                    return await stepContext.BeginDialogAsync(
                        nameof(ResetDialog),
                        await stateAccessor.GetAsync(stepContext.Context),
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
                var result = stepContext.Result;
                await stateAccessor.SetAsync(stepContext.Context, result as Code);
                return await stepContext.EndDialogAsync(result, cancellationToken);
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