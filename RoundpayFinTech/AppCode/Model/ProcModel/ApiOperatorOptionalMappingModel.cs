namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ApiOperatorOptionalMappingModel
    {
        public int _ID { get; set; }
        public int _OID { get; set; }
        public int _APIID { get; set; }
        public string _Key1 { get; set; }
        public string _Key2 { get; set; }
        public string _Key3 { get; set; }
        public string _Key4 { get; set; }
        public string _Value1 { get; set; }
        public string _Value2 { get; set; }
        public string _Value3 { get; set; }
        public string _Value4 { get; set; }
        public string _EntryDate { get; set; }
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int LoginID { get; set; }
    }
    public class GetApiOptionalParam
    {
        public int _OID { get; set; }
        public int _APIID { get; set; }
        public int LoginID { get; set; }
    }
}
