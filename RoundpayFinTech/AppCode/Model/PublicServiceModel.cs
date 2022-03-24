using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class FieldMasterModel
    {
        public int _ID { get; set; }
        public string _Name { get; set; }
        public PESFieldType _FieldType { get; set; }
        [StringLength(50,ErrorMessage ="Not than 50 characters !")]
        public string _InputType { get; set; }
        public int _EntryBy { get; set; }
        public string _EntryDate { get; set; }
        public int _ModifyBy { get; set; }
        public string _ModifyDate { get; set; }
        public string _ServiceName { get; set; }
        public int _OID { get; set; }
        public int _IND { get; set; }
        public int _VocabID { get; set; }
        public string _Placeholder { get; set; }
        public string _Label { get; set; }
        public bool _IsRequired { get; set; }
        public int _MaxLength { get; set; }
        public int _MinLength { get; set; }
        public bool _AutoComplete { get; set; }
        public bool _IsDisabled { get; set; }
        public bool _IsReadOnly { get; set; }
        public bool _Status { get; set; }
        public List<VocabList> _VocabOptions { get; set; }
    }
    public class VocabMaster
    {
        public int _ID { get; set; }
        public string _Name { get; set; }
        public int _IND { get; set; }
        public string _EntryDate { get; set; }
        public int _EntryBy { get; set; }
        public string _ModifyDate { get; set; }
        public int _ModifyBy { get; set; }
    }
    public class VocabList
    {
        public int _ID { get; set; }
        public int _VMID { get; set; }
        public string _Name { get; set; }
        public int _IND { get; set; }
        public string _EntryDate { get; set; }
        public int _EntryBy { get; set; }
        public string _ModifyDate { get; set; }
        public int _ModifyBy { get; set; }
    }
    public class SavePESFormModel
    {
        public int OID { get; set; }
        public List<FieldValuesModel> FieldValuesList { get; set; }
        public int UserID { get; set; }
        public int RequestModeID { get; set; }
        public string APIRequestID { get; set; }
        public string AccountNo { get; set; }
        public string RequestIP { get; set; }
        public string IMEI { get; set; }
        public string Customername { get; set; }
        public string CustomerMobno { get; set; }

    }
    public class FieldValuesModel
    {
        public int FieldID { get; set; }
        public string FieldValue { get; set; }
    }
    public class PESReportViewModel
    {
        public int _ID { get; set; }
        public int _OID { get; set; }
        public string Opname { get; set; }
        public int _FieldID { get; set; }
        public string FieldName { get; set; }
        public string _FieldValue { get; set; }
        public string _FieldLabel { get; set; }
        public int _FieldType { get; set; }
        public string _InputType { get; set; }
        public string _Remark { get; set; }
        public string _EntryDate { get; set; }
        public int _EntryBy { get; set; }
        public int _TID { get; set; }
        public bool _IsRequired { get; set; }
        public string _Customername { get; set; }
        public string _CustomerMobno { get; set; }
        public decimal _Amount { get; set; }
    }

    public class PESApprovedDocument 
    {
        public string PESImage { get; set; }
    }
}
