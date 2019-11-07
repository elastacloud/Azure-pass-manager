// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using APM.BotCore.AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace APM.BotCore.Bots
{
    public class DialogBot<TDialog> : ActivityHandler where TDialog : Dialog
    {
        private readonly ConversationState _conversationState;
        private readonly TDialog _dialog;

        public DialogBot(ConversationState conversationState, TDialog dialog)
        {
            _conversationState = conversationState;
            _dialog = dialog;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var newMember in membersAdded)
            {
                if (newMember.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync("Welcome to the Azure Pass Manager. Do you want to get started? Type any phrase to begin.");
                    var cardAttachment = new APMAdaptiveCard().Get();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                }
            }

            await base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);
        }

        /// <summary>
        /// save our conversation state at the end of the turn. In v4, we have to do this explicitly to write state out to the persistence layer. 
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>ActivityHandler.OnTurnAsync method calls specific activity handler methods, based on the type of activity received, so we save state after the call to the base method.</remarks>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
