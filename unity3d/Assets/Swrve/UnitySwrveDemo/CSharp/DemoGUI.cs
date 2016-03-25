#if !UNITY_5

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Swrve.Messaging;

/// <summary>
/// Swrve SDK demo.
/// </summary>
using Swrve.IAP;


public class DemoGUI : BaseDemoGUI
{
    /// Enable or disable the game UI.
    public bool UIEnabled = true;

    void Start ()
    {
        // In-app messaging setup
        SwrveComponent.SDK.GlobalMessageListener = new CustomMessageListener (this);
        SwrveComponent.SDK.GlobalCustomButtonListener = new CustomButtonListener ();
    }

    void Update ()
    {
        if (UIEnabled) {
            if (buttonPressed [(int)Buttons.SendEvent]) {
                // Trigger a custom event
                SwrveComponent.SDK.NamedEvent (@"button pressed", new Dictionary<string, string> () {
                    { "foo", "bar"
                    }
                });
            }

            if (buttonPressed [(int)Buttons.SendUserAttributes]) {
                // Update a user property
                SwrveComponent.SDK.UserUpdate (new Dictionary<string, string> () {
                    { "health", "100"}, { "gold", "20" }
                });
            }

            if (buttonPressed [(int)Buttons.PurchaseItem]) {
                // Notify of an item purchase
                SwrveComponent.SDK.Purchase (@"someItem", @"gold", 20, 1);
            }

            if (buttonPressed [(int)Buttons.InAppItemPurchase]) {
                // Notify of an in-app purchase
                SwrveComponent.SDK.Iap (1, @"productId", 1.99, @"USD");
            }

            if (buttonPressed [(int)Buttons.InAppCurrencyPurchase]) {
                // Nofity of an in-app purchase with a some currency reward
                IapRewards rewards = new IapRewards (@"gold", 200);
                SwrveComponent.SDK.Iap (1, @"productId", 0.99, @"USD", rewards);
            }

            if (buttonPressed [(int)Buttons.RealIap]) {
                IapRewards rewards = new IapRewards (@"gold", 100);
                rewards.AddCurrency (@"keys", 5);
                rewards.AddItem (@"sword", 1);
#if UNITY_IPHONE
                // IAP validation happens on our servers. Provide if possible the receipt from Apple.
                IapReceipt receipt = RawReceipt.FromString("receipt-from-apple");
                SwrveComponent.SDK.IapApple (1, @"productId", 4.99, @"EUR", rewards, receipt);
#elif UNITY_ANDROID
                // IAP validation happens on our servers. Provide if possible the purchase data from Google.
                string purchaseData = "purchase-data-from-google-play";
                string dataSignature = "data-signature-from-google-play";
                SwrveComponent.SDK.IapGooglePlay (@"productId", 4.99, @"EUR", rewards, purchaseData, dataSignature);
#endif
            }

            if (buttonPressed [(int)Buttons.CurrencyGiven]) {
                // Notify of currency given
                SwrveComponent.SDK.CurrencyGiven (@"gold", 20);
            }

            if (buttonPressed [(int)Buttons.UserResources]) {
                // Obtain the latest value of the resource item01.attribute or its default value
                int attributeValue = SwrveComponent.SDK.ResourceManager.GetResourceAttribute<int> ("item01", "attribute", 99);
                UnityEngine.Debug.Log ("User resource attribute: " + attributeValue);
            }

            if (buttonPressed [(int)Buttons.SendToSwrve]) {
                // Send the queued events in the buffer to Swrve
                SwrveComponent.SDK.SendQueuedEvents ();
            }

            if (buttonPressed [(int)Buttons.TriggerMessage]) {
                // Trigger an in-app message. You will need to setup the campaign
                // in the In-App message section in the dashboard.
                SwrveComponent.SDK.NamedEvent ("campaign_trigger");
            }

            if (buttonPressed [(int)Buttons.SaveToDisk]) {
                // Flush the queued events to disk
                SwrveComponent.SDK.FlushToDisk ();
            }
        }

        base.ClearButtons ();
    }

    /// <summary>
    /// Process in-app message custom button clicks.
    /// </summary>
    private class CustomButtonListener : ISwrveCustomButtonListener
    {
        public void OnAction (string customAction)
        {
            // Custom button logic
            UnityEngine.Debug.Log ("Custom action triggered " + customAction);
        }
    }

    /// <summary>
    /// Observe the SDK for in-app messages and pause/resume your game.
    /// </summary>
    private class CustomMessageListener : ISwrveMessageListener
    {
        private DemoGUI container;

        public CustomMessageListener (DemoGUI container)
        {
            this.container = container;
        }

        public void OnShow (SwrveMessageFormat format)
        {
            // Pause game
            container.UIEnabled = false;
        }

        public void OnShowing (SwrveMessageFormat format)
        {
        }

        public void OnDismiss (SwrveMessageFormat format)
        {
            // Resume game
            container.UIEnabled = true;
        }
    }
}

#endif
