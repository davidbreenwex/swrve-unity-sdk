using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Swrve.Helpers;
using SwrveMiniJSON;

namespace Swrve.Messaging
{
/// <summary>
/// Swrve Talk campaign.
/// </summary>
public class SwrveMessagesCampaign : SwrveBaseCampaign
{
    /// <summary>
    /// List of messages contained in the campaign.
    /// </summary>
    public List<SwrveMessage> Messages;

    private SwrveMessagesCampaign (DateTime initialisedTime, string assetPath) : base (initialisedTime, assetPath)
    {
        this.Messages = new List<SwrveMessage> ();
    }

    /// <summary>
    /// Search for a message related to the given trigger event at the given
    /// time. This function will return null if too many messages were dismissed,
    /// the campaign start is in the future, the campaign end is in the past or
    /// the given event is not contained in the trigger set.
    /// </summary>
    /// <param name="triggerEvent">
    /// Event triggered. Must be a trigger for the campaign.
    /// </param>
    /// <param name="campaignReasons">
    /// At the exit of the function will include the reasons why a campaign the campaigns
    /// in memory were not shown or chosen.
    /// </param>
    /// <returns>
    /// In-app message that contains the given event in its trigger list and satisfies all the
    /// rules.
    /// </returns>
    public SwrveMessage GetMessageForEvent (string triggerEvent, Dictionary<int, string> campaignReasons)
    {
        int messagesCount = Messages.Count;

        if (messagesCount == 0) {
            LogAndAddReason (campaignReasons, "No messages in campaign " + Id);
            return null;
        }

        if (checkCampaignLimits (triggerEvent, campaignReasons)) {
            SwrveLog.Log (triggerEvent + " matches a trigger in " + Id);

            return GetNextMessage (messagesCount, campaignReasons);
        }
        return null;
    }

    /// <summary>
    /// Get a message by its identifier.
    /// </summary>
    /// <returns>
    /// The message with the given identifier if it could be found.
    /// </returns>
    public SwrveMessage GetMessageForId (int id)
    {
        for(int mi = 0; mi < Messages.Count; mi++) {
            SwrveMessage message = Messages[mi];
            if (message.Id == id) {
                return message;
            }
        }

        return null;
    }

    protected SwrveMessage GetNextMessage (int messagesCount, Dictionary<int, string> campaignReasons)
    {
        if (RandomOrder) {
            List<SwrveMessage> randomMessages = new List<SwrveMessage> (Messages);
            randomMessages.Shuffle ();
            for(int mi = 0; mi < randomMessages.Count; mi++) {
                SwrveMessage message = randomMessages[mi];
                if (message.IsDownloaded (assetPath)) {
                    return message;
                }
            }
        } else if (Next < messagesCount) {
            SwrveMessage message = Messages [Next];
            if (message.IsDownloaded (assetPath)) {
                return message;
            }
        }

        LogAndAddReason (campaignReasons, "Campaign " + this.Id + " hasn't finished downloading.");
        return null;
    }

    protected void AddMessage (SwrveMessage message)
    {
        this.Messages.Add (message);
    }

    public override bool AreAssetsReady()
    {
        return this.Messages.All (m => m.IsDownloaded (assetPath));
    }

    public override bool SupportsOrientation(SwrveOrientation orientation) {
        if (SwrveOrientation.Either == orientation) {
            return true;
        }
        return this.Messages.Any (m => m.SupportsOrientation (orientation));
    }

    /// <summary>
    /// Get all the assets in the in-app campaign messages.
    /// </summary>
    /// <returns>
    /// All the assets in the in-app campaign.
    /// </returns>
    public override List<string> ListOfAssets ()
    {
        List<string> allAssets = new List<string> ();
        for(int mi = 0; mi < Messages.Count; mi++) {
            SwrveMessage message = Messages[mi];
            allAssets.AddRange (message.ListOfAssets ());
        }

        return allAssets;
    }

    /// <summary>
    /// Notify that the message was shown to the user. This function
    /// has to be called only once when the message is displayed to
    /// the user.
    /// This is automatically called by the SDK and will only need
    /// to be manually called if you are implementing your own
    /// in-app message rendering code.
    /// </summary>
    public void MessageWasShownToUser (SwrveMessageFormat messageFormat)
    {
        Status = SwrveCampaignState.Status.Seen;
        IncrementImpressions ();
        SetMessageMinDelayThrottle ();
        if (Messages.Count > 0) {
            if (!RandomOrder) {
                int nextMessage = (Next + 1) % Messages.Count;
                Next = nextMessage;
                SwrveLog.Log ("Round Robin: Next message in campaign " + Id + " is " + nextMessage);
            } else {
                SwrveLog.Log ("Next message in campaign " + Id + " is random");
            }
        }
    }

    new public static SwrveMessagesCampaign LoadFromJSON (SwrveSDK sdk, Dictionary<string, object> campaignData, DateTime initialisedTime, string assetPath)
    {
        SwrveMessagesCampaign campaign = new SwrveMessagesCampaign (initialisedTime, assetPath);
        IList<object> jsonMessages = (IList<object>)campaignData ["messages"];
        for (int k = 0, t = jsonMessages.Count; k < t; k++) {
            Dictionary<string, object> messageData = (Dictionary<string, object>)jsonMessages [k];
            SwrveMessage message = SwrveMessage.LoadFromJSON (sdk, campaign, messageData);
            if (message.Formats.Count > 0) {
                campaign.AddMessage (message);
            }
        }
        return campaign;
    }
}
}
