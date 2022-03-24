using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class TBOText
    {
        [JsonProperty("@TextFormat")]
        public string TextFormat { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }

    public class TBOParagraph
    {
        public TBOText Text { get; set; }
    }

    public class TBOSubSection
    {
        [JsonProperty("@SubTitle")]
        public string SubTitle { get; set; }
        public TBOParagraph Paragraph { get; set; }
    }

    public class TBOVendorMessage
    {
        [JsonProperty("@Title")]
        public string Title { get; set; }

        [JsonProperty("@InfoType")]
        public string InfoType { get; set; }
        public TBOSubSection SubSection { get; set; }
    }

    public class TBOVendorMessages
    {
        public TBOVendorMessage VendorMessage { get; set; }
    }

    public class TBOPosition
    {
        [JsonProperty("@Latitude")]
        public string Latitude { get; set; }

        [JsonProperty("@Longitude")]
        public string Longitude { get; set; }
    }

    public class TBOCountryName
    {
        [JsonProperty("@Code")]
        public string Code { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }

    public class TBOAddress
    {
        public List<string> AddressLine { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
        public string StateProv { get; set; }
        public TBOCountryName CountryName { get; set; }
    }

    public class TBOAward
    {
        [JsonProperty("@Provider")]
        public string Provider { get; set; }

        [JsonProperty("@Rating")]
        public string Rating { get; set; }

        [JsonProperty("@ReviewURL")]
        public string ReviewURL { get; set; }
    }

    public class TBOPolicy
    {
        [JsonProperty("@CheckInTime")]
        public string CheckInTime { get; set; }

        [JsonProperty("@CheckOutTime")]
        public string CheckOutTime { get; set; }
    }

    public class TBOAttribute
    {
        [JsonProperty("@AttributeName")]
        public string AttributeName { get; set; }

        [JsonProperty("@AttributeType")]
        public string AttributeType { get; set; }
        public string TBOHotelCode { get; set; }
    }

    public class TBOAttributes
    {
        public List<TBOAttribute> Attribute { get; set; }
    }

    public class TBOBasicPropertyInfo
    {
        [JsonProperty("@BrandCode")]
        public string BrandCode { get; set; }

        [JsonProperty("@TBOHotelCode")]
        public string TBOHotelCode { get; set; }

        [JsonProperty("@HotelName")]
        public string HotelName { get; set; }

        [JsonProperty("@LocationCategoryCode")]
        public string LocationCategoryCode { get; set; }

        [JsonProperty("@IsHalal")]
        public string IsHalal { get; set; }
        public TBOVendorMessages VendorMessages { get; set; }
        public TBOPosition Position { get; set; }
        public TBOAddress Address { get; set; }
        public TBOAward Award { get; set; }
        public TBOPolicy Policy { get; set; }
        public TBOAttributes Attributes { get; set; }

        [JsonProperty("@HotelCategoryId")]
        public string HotelCategoryId { get; set; }

        [JsonProperty("@HotelCategoryName")]
        public string HotelCategoryName { get; set; }
        [JsonProperty("@ImageUrl")]
        public string ImageUrl { get; set; }
        [JsonProperty("@Facility")]
        public string Facility { get; set; }
    }

    public class TBOArrayOfBasicPropertyInfo
    {
        public List<TBOBasicPropertyInfo> BasicPropertyInfo { get; set; }
    }

    public class TvkHotelResponseRoot
    {
        public TBOArrayOfBasicPropertyInfo ArrayOfBasicPropertyInfo { get; set; }
    }

    //Request Model
    public class TvkHotelSyncProcRequest
    {
        public string xmlTBOBasicPropertyInfo { get; set; }
        public string xmlTBOAttribute { get; set; }
        public string cityId { get; set; }
    }

    public class TvkHotelSyncProcRequestByHotelCode
    {
     
        public string xmlTBOImage { get; set; }
        public string xmlTBOFacility { get; set; }
        public string cityId { get; set; }
        public string HotelID { get; set; }
    }

    public class TvkHotelInfoRequest
    {
        public List<string> TBOHotelCodes { get; set; }
        public int CityId { get; set; }
    }

    public class HotelImages
    {
        public string Type { get; set; }
        public string ImageURL { get; set; }
        public string TBOHotelCode { get; set; }
        public string CityId { get; set; }
    }
    public class HotelFacilities
    {
        public string Fcility { get; set; }
        public string Title { get; set; }
        public string TBOHotelCode { get; set; }
        public string CityId { get; set; }
    }
    public class HotelImagesData
    {
        public List<HotelImages> HotelImages { get; set; }
        public List<HotelFacilities> HotelFacility { get; set; }
    }
}
