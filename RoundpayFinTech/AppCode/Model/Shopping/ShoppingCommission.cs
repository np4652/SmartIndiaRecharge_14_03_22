using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewShoping = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ShoppingCommission
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int CategoryID { get; set; }
        public int RoleID { get; set; }
        public string CategoryName { get; set; }
        public decimal Commission { get; set; }
        public bool AmountType { get; set; }
    }

    public class ShoppingCommissionExtend
    {
        public IEnumerable<ShoppingCommission> Items { get; set; }
        public IEnumerable<ShoppingCommission> CommissionDetail { get; set; }
        public IEnumerable<RoleMaster> Roles { get; set; }
    }

    public class ShoppingCommissionReq
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int CategoryID { get; set; }
        public int SlabID { get; set; }
        public int RoleID { get; set; }
        public bool CommType { get; set; }
        public decimal Commission { get; set; }
        public bool IsAdminDefined { get; set; }
    }

    public class ShoppingCommissionModel
    {
        public bool IsAdminDefined { get; set; }
        public int SlabID { get; set; }
        public int SubCatLvl2Id { get; set; }
        public int RoleId { get; set; }
        public IEnumerable<NewShoping.SubCategory> ShoppingCategories { get; set; }
        public IEnumerable<MasterRole> Roles { get; set; }
        public ShoppingCommissionExtend CommissionDetail { get; set; }
    }
}
