namespace APM.BotCore.Dialogs
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class HelpDialog : ComponentDialog
    {
        public HelpDialog() : base(nameof(HelpDialog))
        {

        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
        {
            var cardAttachment = GetAttachment();
            await outerDc.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

            return await base.BeginDialogAsync(outerDc, options, cancellationToken);
        }

        private Attachment GetAttachment()
        {
            var filePath = "./Resources/Adaptive.APM.json";
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}