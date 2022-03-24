using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IAffiliationML
    {
        ResponseStatus SaveAffiliateVendor(string VendorName, int Id, bool IsActive);
        AfVendorCommission GetAfCategoryCommission(int VendorID);
        IEnumerable<AffiliateVendors> GetAffiliatedVendors(int id);
        ResponseStatus SaveAfVendorCateComm(int OID, int VendorID, decimal comm, int AmtType);
        ResponseStatus AddAffiliatedItems(int VendorID, string Link, string ImgUrl, bool IsActive, int ID, int LinkType, int OID, bool IsDel, bool IsImageURL);
        IEnumerable<AffiliatedItem> GetAllAfItems(int VendorID);
        AffiliateItemModal GetAfItemsById(int id, int VendorID);
        ASlabDetailModel GetASlabDetail(int SLabID, bool IsAdminDefined);
        ASlabDetailModel GetASlabDetailRole(int SLabID, int RoleID, int CategoryID);
        ResponseStatus UpdateAfSlabComm(ASlabDetail req);
        AFItemsForDisplay GetAfItemsForDisplay();
    }
}
