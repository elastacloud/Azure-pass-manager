namespace APM.BotCore.Dialogs
{
    using APM.Domain;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    public class ResetDialog : ComponentDialog
    {
        private const string AzureCodeKeyName = "AzurePromoCode";

        public ResetDialog() : base(nameof(ResetDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                DetectPreviousState,
                PromptForConfirm,
                ResumeAfterAsync
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }

        private async Task<DialogTurnResult> ResumeAfterAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userResponseIn = stepContext.Result;
            if (userResponseIn is FoundChoice userResponse && userResponse.Value == "Yes")
            {                
                    var response = $"Your Azure trial code is cleared";
                    //todo: context.UserData.SetValue(AzureCodeKeyName, null);
                    await stepContext.Context.SendActivityAsync(response, response);

                    response = "Good luck with your project, you can now close this conversation.";
                    await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            }
            else
            {

                var abandonedResponse = "Ok, you can keep using that code. ";
                await stepContext.Context.SendActivityAsync(abandonedResponse, cancellationToken: cancellationToken);

                return await stepContext.EndDialogAsync(stepContext.Options, cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForConfirm(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var code = (stepContext.Options) as Code; 
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Choices = new List<Choice>() { new Choice("Yes"), new Choice("No") },
                    Prompt = MessageFactory.Text($"Are you sure you want to clear your Azure trial code '{code.PromoCode}'?"),
                }, 
                cancellationToken);
        }

        private async Task<DialogTurnResult> DetectPreviousState(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!(stepContext.Options is Code))
            {
                string response = $"You don't already have an Azure trial code.";
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.NextAsync(cancellationToken);
        }
    }
}