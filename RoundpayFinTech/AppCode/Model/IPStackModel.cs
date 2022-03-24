using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ISPLanguage
    {
        public string code { get; set; }
        public string name { get; set; }
        public string native { get; set; }
    }

    public class IPSLocation
    {
        public int geoname_id { get; set; }
        public string capital { get; set; }
        public List<ISPLanguage> languages { get; set; }
        public string country_flag { get; set; }
        public string country_flag_emoji { get; set; }
        public string country_flag_emoji_unicode { get; set; }
        public string calling_code { get; set; }
        public bool is_eu { get; set; }
    }

    public class IPStackAPIResp
    {
        public string ip { get; set; }
        public string type { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_code { get; set; }
        public string region_name { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public IPSLocation location { get; set; }
        public bool success { get; set; }
        public IPError error { get; set; }
    }
    public class IPError
    {
        public int code { get; set; }
        public string info { get; set; }
    }

    public class IPGeoLocInfoReq : CommonReq
    {
        public string IPInfo { get; set; }
    }

    public class IPResponseStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }

        public List<IPAPIDetails> IPGeoAPIs { get; set; }
    }

    public class IPAPIDetails
    {
        public string APIURL { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
    }

    public class IPStatusResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string IP { get; set; }
        public string Type { get; set; }
        public string Continent_Code { get; set; }
        public string Continent_Name { get; set; }
        public string Country_Code { get; set; }
        public string Country_Name { get; set; }
        public string Region_Code { get; set; }
        public string Region_Name { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Capital { get; set; }
        public string LanguageName { get; set; }
        public string LanguageNative { get; set; }

    }


    public class ApiIPGeoLocInfoReq 
    {
        public int UserID { get; set; }
        public string Token { get; set; }
        public string IPInfo { get; set; }
    }

}
