#define NEWSUMMARY

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdSage.Concert.SEMObjects
{
    /// <summary>
    /// This status is to record something like ServerChanged-LocalChanged, ServerChanged-LocalNotChanged, ServerNotChange-LocalChange
    /// But it is pending on what API we can use
    /// </summary>
    public enum SEMObjectServerStatus : byte
    {
        Nothing = 0,
        Added,
        Modified,
        Deleted
    }
    /// <summary>
    /// status of an SEM object
    /// </summary>
    public enum SEMObjectStatus : byte
    {
        Original=0,   // object not modified since last download
        New=1,        // newly created object
        Changed=2,    // object changed since last download
        Deleted=3,    // object deleted since last download
        Renew=4,    //delete object and create new
        Unknown = 9   // unknow status
    };

    /// <summary>
    /// special state of an SEM object
    /// </summary>
    [Flags]
    public enum SEMObjectState : byte
    {
        Normal = 0, // object is in the normal state
        Error = 1,  // object is in the error state
        Locked = 2, // object is locked
        ValidationWarning = 4,  // object has validation warning
        ValidationError = 8,    // object has validation error
        DownloadError = 16,     // error during download
        UploadError = 32,       // error during upload
        Optimized = 64
    }

    /// <summary>
    /// type of SEM object
    /// </summary>
    public enum SEMObjectType
    {
        User = 1,
        Account = 2,
        Campaign = 3,
        AdGroup = 4,
        Ad = 5,
        OrderItem = 6,
        NegativeOrderItem = 7,
        Website = 8,
        NagativeWebsite = 9
    }

    /// <summary>
    /// type of search engine
    /// </summary>
    public enum SearchEngineType
    {
        Google = 0,
        AdCenter = 1,
        Yahoo = 2,
        Baidu = 3,
        Phoenix = 4,
        Facebook=5
    }

    /// <summary>
    /// This SEMObjectDetailType / 10000 = SearchEngineType
    /// This (SEMObjectDetailType % 10000) / 100 = SEMObjectType
    /// </summary>
    public enum SEMObjectDetailType
    {
        Unknown = 0,
        GoogleUser=100,
        GoogleAccountInfo=200,
        GoogleCampaign=300,
        GoogleAdGroup=400,
        GoogleAd=500,
        GoogleTextAd=501,
        GoogleImageAd=502,
        GoogleLocalBusinessAd=503,
        GoogleMobileAd=504,
        GoogleVideoAd=505,
        GoogleCommerceAd=506,
        GoogleKeyword=600,
        GoogleAdGroupNegativeKeyword = 601,
        GoogleCampaignNegativeKeyword = 700,
        GoogleWebsite = 800,
        GoogleAdGroupNegativeWebsite=801,
        GoogleCampaignNegativeWebsite = 900,
        GoogleSiteLink =999,


        YahooUser = 20100,
        YahooAccount=20200,
        YahooCampaign=20300,
        YahooAdGroup=20400,
        YahooAd=20500,
        YahooKeyword=20600,
        YahooAccountExcludedWord=20700,
        YahooAdGroupExcludedWord=20701,

        AdCenterUser=10100,
        AdCenterAccount=10200,
        AdCenterCampaign=10300,
        AdCenterAdGroup=10400,
        AdCenterAd=10500,
        AdCenterTextAd = 10501,
        AdCenterImageAd = 10502,
        AdCenterMobileAd = 10503,
        AdCenterKeyword=10600,
        AdCenterSitePlacement = 10800,
        
        BaiduUser = 30100,
        BaiduAccount = 30200,
        BaiduCampaign= 30300,
        BaiduKeywordGroup=30400,
        BaiduKeyword=30600,

        PhoenixUser = 40100,
        PhoenixAccount = 40200,
        PhoenixCampaign = 40300,
        PhoenixAdGroup = 40400,
        PhoenixAd = 40500,
        PhoenixKeyword = 40600,


        FacebookUser = 50100,
        FacebookAccount = 50200,
        FacebookCampaign = 50300,
        FacebookAdGroup = 50400,
        FacebookAd = 50500,
        FacebookKeyword = 50600,
        FacebookAdGroupForOptimization = 50700


    }

    public enum SEMObjectProgressType
    {
        Download_Performance_Start = 41000001,
        Download_Performance_Downloaded = 41000002,
        Download_Performance_Decompressed = 41000003,
        Download_Performance_Uploaded = 41000004,
        Download_Performance_Preprocessed = 41000005,
        Download_Performance_Completed = 41000006,
        Download_Performance_Canceled = 41000007,
        Download_Performance_Failed = 41000008,

        API_Google_Connection_Timeout = 41000010,
        API_AdCenter_Connection_Timeout = 41000011

        //Download_Performance_StartDownload = 41000001,
        //Download_Performance_EndDownload = 41000002,
        //Download_Performance_StartImportToDB = 41000003,
        //Download_Performance_EndImportToDB = 41000004,
        //Download_Performance_Failed = 41000009
    }

    public enum SEMObjectErrorType
    {
        Unknown = 0,

        Account_CurrencyCode_Invalid = 20011,

        Campaign_ExceededMaxCount = 30001,
        Campaign_Duplicate = 30002,
        Campaign_Name_Empty = 30011,
        Campaign_Name_ExceededMaxLength = 30012,
        Campaign_StartDate_BeforeToday = 30021,
        Campaign_StartDate_AfterEndDate = 30022,
        Campaign_EndDate_ExceededMaxDate = 30023,
        Campaign_EndDate_BeforeToday = 30024,
        Campaign_Budget_Invalid = 30031,
        Campaign_Budget_ExceededAccount = 30032,
        Campaign_Budget_Invalid_CNY = 30033,
        Campaign_Monthly_Budget_Empty = 30034,
        Campaign_Monthly_Budget_Invalid = 30035,
        Campaign_Daily_Budget_Invalid = 30036,
        Campaign_NetworkTargeting_Invalid = 30041,
        Campaign_Network_Both_NULL = 30042,
        Campaign_Deleted_OnServer = 30051,
        Campaign_SiteLinksNumber_ExceedMaxNumer=30057,
        Campaign_SiteLinkDisplayName_ExceedMaxLength=30058,
        Campaign_SiteLinkDesURL_ExceedMaxLength=30059,
        Campaign_SiteLinksDisplayName_Empty=30060,
        Campaign_SiteLinksURL_Empty=30061,

        CampaignStatusInvalidError = 30052,
        CampaignStopTimeMoreThanFuture=30053,
        CampaignStopTimeLessThanStartTime =30054,
        CampaignStopTimeLessThanNow = 30055,
        CampaignBudgetOverflowError = 30056,

        AdGroup_ExceededMaxCount = 40001,
        AdGroup_Duplicate = 40002,
        AdGroup_Name_Empty = 40011,
        AdGroup_Name_ExceededMaxLength = 40012,
        AdGroup_MaxCpc_Invalid = 40021,
        AdGroup_MaxCpc_ExceededCampaign = 40022,
        AdGroup_MaxCpc_ExceededAccount = 40023,
        AdGroup_MaxCpc_Invalid_CNY = 40024,
        AdGroup_SiteMaxCpc_Invalid = 40031,
        AdGroup_SiteMaxCpc_ExceededCampaign = 40032,
        AdGroup_SiteMaxCpc_Missing = 40033,
        AdGroup_SiteMaxCpc_Invalid_CNY = 40034,
        AdGroup_SiteMaxCpm_Invalid = 40035,
        AdGroup_SiteMaxCpm_ExceededCampaign = 40036,
        AdGroup_SiteMaxCpm_Missing = 40037,
        AdGroup_SiteMaxCpm_Invalid_CNY = 40038,
        AdGroup_ContentCpc_Invalid = 40041,
        AdGroup_ContentCpc_ExceededCampaign = 40042,
        AdGroup_ContentCpc_Invalid_CNY = 40043,
        AdGroup_StartDate_BeforeToday = 40051,
        AdGroup_StartDate_AfterEndDate = 40052,
        AdGroup_EndDate_BeforeStartDate = 40053,
        AdGroup_EndDate_ExceededMaxDate = 40054,
        AdGroup_EndDate_BeforeToday = 40056,
        AdGroup_AdDistribution_Null = 40061,
        AdGroup_PriceModel_CPM_With_SearchNetWork = 40062,

        //author:mengwenping
        AdGroup_MaxPrice_Between = 40057,
        AdGroup_TargetingCountriesNull = 40058,
        AdGroup_TargetingCountriesFrance = 40059,
        AdGroup_Image_Invalid = 40060,
        AdGroup_Image_Null = 40061,
        AdGroup_Status_Invalid = 40062,
        Facebook_AdGroup_Name_Empty =40063, 
        Facebook_AdGroup_Image_Null = 40064,
        Facebook_Ad_Body_Empty = 40065,
        Facebook_Ad_Title_Empty = 40066,
        Facebook_Ad_AdText_ExceededMaxLength = 40067,
        Facebook_Ad_AdBody_ExceededMaxLength = 40068,
        Facebook_Ad_AdLinkUrl_Empty = 40069,
        Facebook_Ad_AdLinkUrl_incorrect = 40070,


        Keyword_ExceededMaxCount = 60001,
        Keyword_Duplicate = 60002,
        Keyword_Name_Empty = 60011,
        Keyword_Name_ExceededMaxLength = 60012,
        Keyword_Name_Invalid = 60013,
        Keyword_Name_ExceededMaxWords = 60014,
        Keyword_Name_EndOfInvalidChar = 60015,
        Keyword_MaxCpc_Invalid_CNY = 60021,
        Keyword_MaxCpc_ExceededCampaign = 60022,
        Keyword_MaxCpc_Invalid_USD = 60023,
        Keyword_BroadBid_Invalid = 60024,
        Keyword_ExactBid_Invalid = 60025,
        Keyword_PhraseBid_Invalid = 60026,
        Keyword_BroadBid_ExceededCampaign = 60027,
        Keyword_ExactBid_ExceededCampaign = 60028,
        Keyword_PhraseBid_ExceededCampaign = 60029,
        Keyword_DestinationUrl_Invalid = 60031,
        Keyword_DestinationUrl_ExceededMaxLength = 60032,
        Keyword_Param2_ExceedMaxLength = 60041,
        Keyword_Param3_ExceedMaxLength = 60042,
        Keyword_ContentCpc_Invalid = 60051,
        Keyword_ContentCpc_ExceededCampaign = 60052,
        Keyword_Matchbid_AllNull = 60061,
        Keyword_Dynamic_Ad_Title_ExceedMaxLength = 60071,
        Keyword_Dynamic_Ad_Text_ExceedMaxLength = 60072,
        Keyword_Dynamic_Ad_DisplayUrl_ExceedMaxLength = 60073,
        Keyword_Dynamic_Ad_DestinationUrl_ExceedMaxLength = 60074,

        Ad_ExceededMaxCount = 50001,
        Ad_Duplicate = 50002,
        Ad_Title_Empty = 50011,
        Ad_Title_ExceededMaxLength = 50012,
        Ad_Title_Invalid = 50013,
        Ad_Title_WildcardKeywordEmpty = 50014,
        Ad_Title_EnterCharInTheFirstSentence = 50015,
        Ad_Title_EnterCharInTheLastSentence = 50016,
        Ad_Title_EnterCharLocateInKeyword = 50017,
        Ad_Title_EnterCharMoreThanOnce = 50018,
        Ad_DestinationUrl_Empty = 50021,
        Ad_DestinationUrl_Invalid = 50022,
        Ad_DestinationUrl_ExceededMaxLength = 50023,
        Ad_DisplayUrl_Empty = 50031,
        Ad_DisplayUrl_Invalid = 50032,
        Ad_DisplayUrl_ExceededMaxLength = 50033,
        Ad_Description1_Empty = 50041,
        Ad_Description1_ExceededMaxLength = 50042,
        Ad_Description1_Invalid = 50043,
        Ad_Description1_WildcardKeywordEmpty = 50044,
        Ad_Description1_EnterCharInTheFirstSentence = 50045,
        Ad_Description1_EnterCharInTheLastSentence = 50046,
        Ad_Description1_EnterCharLocateInKeyword = 50047,
        Ad_Description1_EnterCharMoreThanOnce = 50048,
        Ad_Description2_Empty = 50051,
        Ad_Description2_ExceededMaxLength = 50052,
        Ad_Description2_Invalid = 50053,
        Ad_Description2_WildcardKeywordEmpty = 50054,
        Ad_AdText_Empty = 50061,
        Ad_AdText_ExceededMaxLength = 50062,

        Website_ExceededMaxCount = 80001,
        Website_Duplicate = 80002,
        Website_ExceededMaxCount_Negative = 80003,
        Website_Duplicate_Negative = 80004,
        Website_Url_Empty = 80011,
        Website_Url_ExceededMaxLength = 80012,
        Website_Url_Invalid = 80013,
        Website_Url_Empty_Negative = 80014,
        Website_Url_ExceededMaxLength_Negative = 80015,
        Website_Url_Invalid_Negative = 80016,
        Website_DestinationUrl_Empty = 80021,
        Website_DestinationUrl_ExceededMaxLength = 80022,
        Website_DestinationUrl_Invalid = 80023,
        Website_MaxCpc_Invalid_USD = 80031,
        Website_MaxCpc_ExceededCampaign = 80032,
        Website_MaxCpc_Invalid_CNY = 80033,
        Website_MaxCpm_Invalid = 80041,
        Website_MaxCpm_ExceededCampaign = 80042,

        NegativeKeyword_ExceededMaxCount = 70001,
        NegativeKeyword_Duplicate = 70002, 
        NegativeKeyword_ExceededMaxLength = 70003,
        AdGroup_NegativeKeyword_ExceededMaxCount = 70004,
        Keyword_NegativeKeyword_Aggreate_ExceededMaxLength = 70005,
        NegtiveKeyword_NotFor_Exact = 70006,
        NegtiveKeyword_Match_Keyword = 70007,

        NegativeWebsite_ExceededMaxCount = 90001,
        NegativeWebsite_Duplicate = 90002,
        NegativeWebsite_ExceededDomainMaxCount = 90003,
        AdGroup_NegativeWebsite_ExceededMaxCount = 90004,
        NegativeWebsite_Url_Invalid = 90005


       
    }

    public static class SEMObjectStatusHelper
    {
        public static bool IsGoogleObject(SEMObjectDetailType detailObjectType)
        {
            return (((int)detailObjectType / 10000) == (int)SearchEngineType.Google);
        }

        public static SearchEngineType DetailObjectTypeToSearchEngineType(SEMObjectDetailType detailObjectType)
        {
            return (SearchEngineType)Enum.ToObject(typeof(SearchEngineType), (int)detailObjectType / 10000);
        }

        public static SEMObjectType DetailObjectTypeToObjectType(SEMObjectDetailType detailObjectType)
        {
            return (SEMObjectType)Enum.ToObject(typeof(SEMObjectType), ((int)detailObjectType % 10000) / 100);
        }

        public static SEMObjectDetailType SearchEngineTypeObjectTypeToDetailType (SearchEngineType seType, SEMObjectType objType)
        {
            return (SEMObjectDetailType)Enum.ToObject(typeof(SEMObjectDetailType), ((int)seType * 10000 + (int)objType * 100));
        }
    }
#if NEWSUMMARY
    public class SEMObjectSummary
    {
        public string CategoryName
        {
            get;
            set;
        }
        public SummaryType SummaryType
        {
            get;
            set;
        }
        public IEnumerable<KeyValuePair<object, object>> SummaryDetails
        {
            get;
            set;
        }
    }

    public enum SummaryType
    {
        Performance,
        ChildObjectCount
    }

    [Flags]
    public enum DescendantStatus : byte
    {
        // no errors, warnings, or changes at all
        None = 0,
        HasErrors = 1,
        HasWarnings = 2,
        HasChanges = 4
    }

#else
    public class SEMObjectSummary
    {
        public long Impressions
        {
            get;
            set;
        }
        public long Clicks
        {
            get;
            set;
        }
        public long Conversions
        {
            get;
            set;
        }
        public double TotalCost
        {
            get;
            set;
        }
        public double TotalPosition
        {
            get;
            set;
        }
        public IEnumerable<KeyValuePair<SEMObjectDetailType, int>> ChildrenCount
        {
            get;
            set;
        }
    }
#endif
}
