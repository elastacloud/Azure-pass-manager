namespace APM.BotCore.Dialogs
{
    using APM.Domain;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    public class APMDialog : ComponentDialog
    {
        private const string AzureCodeKeyName = "AzurePromoCode";
        private readonly IAPMHelper aPMHelper;

        public APMDialog(IAPMHelper aPMHelper) : base(nameof(APMDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                DetectPreviousState,
                PromptForEventName,
                ResumeAfterAsync
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.aPMHelper = aPMHelper;
        }

        private async Task<DialogTurnResult> ResumeAfterAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userResponseIn = stepContext.Result;
            if (userResponseIn is string userResponse && !string.IsNullOrWhiteSpace(userResponse))
            {
                var code = await aPMHelper.GetAzurePassCode(userResponse);
                if (code != null)
                {
                    string response = $"Your Azure trial code is: {code.PromoCode} which expires on {code.Expiry.ToLongDateString()}.  You can activate your code at https://www.microsoftazurepass.com";

                    stepContext.Values[AzureCodeKeyName] = code;
                    await stepContext.Context.SendActivityAsync(response, response);

                    response = "Good luck with your project, you can now close this conversation.";
                    await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.RepromptDialogAsync(cancellationToken);
                }
            }

            return await stepContext.EndDialogAsync(stepContext.Values[AzureCodeKeyName], cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForEventName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text("What is your event name?  Your Microsoft representative can give you this if you don't know."),
                    RetryPrompt = MessageFactory.Text("That's not right. What is your event name?  Your Microsoft representative can give you this if you don't know.")
                }, 
                cancellationToken);
        }

        private async Task<DialogTurnResult> DetectPreviousState(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Options is Code existingCode)
            {
                string response = $"You already have an Azure trial code.  Which is: {existingCode.PromoCode}";
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(existingCode, cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken);
        }
    }
}