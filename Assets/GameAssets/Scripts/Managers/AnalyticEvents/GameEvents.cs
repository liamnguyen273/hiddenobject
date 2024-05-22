using System.Collections.Generic;

namespace com.tinycastle.StickerBooker
{
    public static class GameEvents
    {
        // Revenue base
        public const string INTERSTITIAL_AD_REQUEST = "INTERSTITIAL_AD_REQUEST";
        public const string INTERSTITIAL_AD_SHOW = "INTERSTITIAL_AD_SHOW";
        public const string REWARD_AD_REQUEST = "REWARD_AD_REQUEST";
        public const string REWARD_AD_SHOW = "REWARD_AD_SHOW";
        public const string REWARD_AD_REWARDED = "REWARD_AD_REWARDED";
        public const string AD_IMPRESSION_OR_REVENUE = "AD_IMPRESSION_OR_REVENUE";
        public const string IAP_PURCHASED = "IAP_PURCHASED";
        
        // Gameplay base
        public const string START_LEVEL = "START_LEVEL";
        public const string COMPLETE_LEVEL = "COMPLETE_LEVEL";
        public const string BACK_LEVEL = "BACK_LEVEL";
        public const string OPEN_APP = "OPEN_APP";

        public static readonly Dictionary<string, string> AppFlyerTranslations = new()
        {
            {INTERSTITIAL_AD_SHOW, AppFlyerEvents.AF_INTERS},
            {REWARD_AD_REWARDED, AppFlyerEvents.AF_REWARDED},
            {AD_IMPRESSION_OR_REVENUE, AppFlyerEvents.AF_AD_REVENUE},
            {IAP_PURCHASED, AppFlyerEvents.AF_PURCHASE},
        };

        public static readonly Dictionary<string, string> FirebaseTranslations = new()
        {
            {START_LEVEL, FirebaseEvents.START_LEVEL},
            {COMPLETE_LEVEL, FirebaseEvents.COMPLETE_LEVEL},
            {BACK_LEVEL, FirebaseEvents.BACK_LEVEL},
            {OPEN_APP, FirebaseEvents.OPEN_APP},
            {INTERSTITIAL_AD_SHOW, FirebaseEvents.AF_INTERS},
            {INTERSTITIAL_AD_REQUEST, FirebaseEvents.INTER_ATTEMPT},
            {REWARD_AD_REWARDED, FirebaseEvents.AF_REWARDED},
            {REWARD_AD_REQUEST, FirebaseEvents.REWARD_ATTEMPT},
            {AD_IMPRESSION_OR_REVENUE, FirebaseEvents.AD_IMPRESSION},
        };
    }
    
    internal static class AppFlyerEvents
    {
        public const string AF_INTERS = "af_inters";
        public const string AF_REWARDED = "af_rewarded";
        public const string AF_AD_REVENUE = "af_ad_revenue";
        public const string AF_PURCHASE = "af_purchase";
    }

    internal static class FirebaseEvents
    {
        public const string START_LEVEL = "start_level";
        public const string COMPLETE_LEVEL = "complete_level";
        public const string BACK_LEVEL = "back_level";
        public const string OPEN_APP = "open_app";
        
        public const string AF_INTERS = "af_inters";
        public const string INTER_ATTEMPT = "inter_attempt"; 
        public const string AF_REWARDED = "af_rewarded";
        public const string REWARD_ATTEMPT = "reward_attempt";
        
        public const string AD_IMPRESSION = "ad_impression";
    }
}