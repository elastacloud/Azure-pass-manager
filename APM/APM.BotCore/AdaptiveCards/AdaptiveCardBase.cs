using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace APM.BotCore.AdaptiveCards
{
    public abstract class AdaptiveCardBase
    {
        public abstract Attachment Get();
        protected Attachment GetAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
    public class APMAdaptiveCard : AdaptiveCardBase
    {
        public override Attachment Get()
        {
            var filePath = "./Resources/Adaptive.APM.json";
            return GetAttachment(filePath);
        }
    }
}
