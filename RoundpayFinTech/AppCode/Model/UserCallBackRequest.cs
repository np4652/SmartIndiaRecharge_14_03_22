using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class UserCallBackModel
    {
        public int UserID { get; set; }
        public int CallbackType { get; set; }
        public string CallBackName { get; set; }
        public string URL { get; set; }
        public string UpdateUrl { get; set; }
        public string Parameters { get; set; }
        public string Remark { get; set; }
        public List<CallbackTypeModel> CallBackTypeList { get; set; }
    }
    public class CallbackTypeModel
    {
        public int ID { get; set; }        
        public string Type { get; set; }
        public string Parameters { get; set; }
    }
}
