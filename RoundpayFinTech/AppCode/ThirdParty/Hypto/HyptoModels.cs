using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Hypto
{
    public class HyptoResponseModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public HyptoData data { get; set; }
    }
    public class HyptoData
    {
        public int id { get; set; }
        public string txn_time { get; set; }
        public string created_at { get; set; }
        public string txn_type { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public decimal charges_gst { get; set; }
        public decimal settled_amount { get; set; }
        public string bank_ref_num { get; set; }
        public decimal closing_balance { get; set; }
        public string payment_type { get; set; }
        public string reference_number { get; set; }
        public string verify_reason { get; set; }
        public string verify_upi_id { get; set; }
        public string verify_upi_id_holder { get; set; }
        public string verify_account_holder { get; set; }
        public bool connected_banking { get; set; }
        public string transfer_reason { get; set; }
        public string transfer_beneficiary_name { get; set; }
        public string transfer_account_number { get; set; }
        public string transfer_account_ifsc { get; set; }
        public string transfer_upi_id { get; set; }
        public string transfer_note { get; set; }
        public string transfer_udf1 { get; set; }
        public string transfer_udf2 { get; set; }
        public string transfer_udf3 { get; set; }
        public string transfer_account_holder { get; set; }
        public string transfer_initiated_account_ifsc { get; set; }
        public string txn_id { get; set; }

    }
    public class HyptoAppSetting
    {
        public string BaseURL { get; set; }
        public string Auth { get; set; }
    }
}
