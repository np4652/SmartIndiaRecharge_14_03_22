using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.DepartmentModel
{
    public class MasterOperation
    {
        public int ID { get; set; }
        public string OperationName { get; set; }
        public string OperationCode { get; set; }
    }
    public class MasterMenu
    {
        public int ID { get; set; }
        public string Menu { get; set; }
        public List<MasterOperation> MasterOperation { get; set; }
        public List<MenuOperation> MenuOperation { get; set; }
    }
    public class MenuOperation
    {
        public string DevKey { get; set; }
        public int ID { get; set; }
        public int MenuID { get; set; }
        public int OperationID { get; set; }
        public bool IsActive { get; set; }
    }

    public class OperationAssigned
    {       
        public int MenuID { get; set; }
        public string Menu { get; set; }
        public int OperationID { get; set; }
        public string Operation { get; set; }
        public string OperationCode { get; set; }
        public bool IsActive { get; set; }
    }
    public class _OperationAssigned: OperationAssigned
    {
        public int LoginID { get; set; }
        public string UMobile { get; set; }
        public int UserID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
        public class MenuOperationAssigned {
        public int MenuID { get; set; }
        public string Menu { get; set; }
        public List<OperationAssigned> OperationAssigneds { get; set; }
    }
}
