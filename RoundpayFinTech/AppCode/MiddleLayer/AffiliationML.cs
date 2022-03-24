using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class AffiliationML : IAffiliationML
    {
        #region Gloabl Variables

        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        #endregion

        public AffiliationML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
        }

        public ResponseStatus SaveAffiliateVendor(string VendorName, int Id, bool IsActive)
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = VendorName,
                CommonInt = Id,

                CommonBool = IsActive
            };
            IProcedure proc = new ProcSaveAffiliateVendor(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public AfVendorCommission GetAfCategoryCommission(int VendorID)
        {
            IProcedure proc = new ProcAfVendorCommission(_dal);
            var response = (IEnumerable<AffiliateVendorCommission>)proc.Call(VendorID);
            var res = new AfVendorCommission
            {
                Commissions = response,
                VendorID = VendorID
            };
            return res;
        }

        public IEnumerable<AffiliateVendors> GetAffiliatedVendors(int id)
        {
            IProcedure proc = new ProcGetAffiliatedVendors(_dal);
            var res = (List<AffiliateVendors>)proc.Call(id);
            return res;
        }

        public IEnumerable<OperatorDetail> GetAfCategories()
        {
            IProcedure proc = new ProcGetOperatorByService(_dal);
            var res = (IEnumerable<OperatorDetail>)proc.Call(new CommonReq { CommonStr = "AFS" });
            return res;
        }

        public ResponseStatus SaveAfVendorCateComm(int OID, int VendorID, decimal comm, int AmtType)
        {
            var req = new AffiliateVendorCommission
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                AmtType = AmtType,
                Commission = comm,
                OID = OID,
                CommonInt = VendorID
            };
            IProcedure proc = new ProcSaveAfVendorCateComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public ResponseStatus AddAffiliatedItems(int VendorID, string Link, string ImgUrl, bool IsActive, int ID, int LinkType, int OID, bool IsDel, bool IsImageURL)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = VendorID,
                CommonStr = Link,
                CommonStr2 = ImgUrl,
                CommonInt2 = ID,
                CommonBool = IsActive,
                CommonBool1 = IsImageURL,
                CommonInt3 = LinkType,
                CommonInt4 = OID,
                CommonBool2 = IsDel
            };
            IProcedure proc = new ProcAddAffiliatedItems(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public IEnumerable<AffiliatedItem> GetAllAfItems(int VendorID)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 1,
                CommonInt = VendorID,
                CommonBool = false
            };
            IProcedure proc = new ProcGetAllAfItems(_dal);
            var response = (IEnumerable<AffiliatedItem>)proc.Call(req);
            return response;
        }

        //===================================================================
        public AFItemsForDisplay GetAfItemsForDisplay()
        {
            var Afitems = GetAllAfItems(0);
            var Categories = GetAfCategories();
            var Vendors = GetAffiliatedVendors(0);
            var AfVendorWithCategories = new List<AfVendorWithCategories>();

            foreach (var v in Vendors)
            {
                var AfCategoriesWithItems = new List<AfCategoriesWithItems>();
                foreach (var cat in Categories)
                {
                    AfCategoriesWithItems.Add(new AfCategoriesWithItems
                    {
                        CategoryName = cat.Name,
                        CategoryID = cat.OID,
                        Items = from pro in Afitems.Where(x => x.OID == cat.OID && x.VendorID == v.Id).ToList() select new AfProducts { ID = pro.ID, VendorID = pro.VendorID, Link = pro.Link, ImgUrl = pro.ImgUrl, LinkType = pro.LinkType }
                    }); 
                }

                AfVendorWithCategories.Add(new AfVendorWithCategories
                {
                    VendorId=v.Id,
                    VendorName=v.VendorName,
                    CategoryWithItems = AfCategoriesWithItems
                });
            }

            var res = new AFItemsForDisplay()
            {
                data = AfVendorWithCategories
            };

            return res;
        }

        //=========================================================================



        public AffiliateItemModal GetAfItemsById(int id, int VendorID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = VendorID,
                CommonInt2 = id,
                CommonBool = true
            };
            IProcedure proc = new ProcGetAllAfItems(_dal);
            var response = (AffiliateItemModal)proc.Call(req);
            return response;
        }

        public ASlabDetailModel GetASlabDetail(int SLabID, bool IsAdminDefined)
        {
            var response = new ASlabDetailModel();
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = SLabID,
                CommonInt2 = _lr.RoleID,
            };
            if (!IsAdminDefined)
            {
                IProcedure proc = new ProcASlabDetail(_dal);
                response = (ASlabDetailModel)proc.Call(req);
            }
            else
            {
                var param = new CommonReq
                {
                    CommonStr = "AFS"
                };
                IProcedure proc = new ProcGetOperatorByService(_dal);
                var Operator = (IEnumerable<OperatorDetail>)proc.Call(param);
                //IProcedure proc = new ProcGetAffiliateCategory(_dal);
                //var category = (List<AffiliateCategory>)proc.Call();
                IProcedure procRole = new ProcGetAllUserRole(_dal);
                var Roles = (List<RoleMaster>)procRole.Call(req);
                response.SlabID = SLabID;
                //response.AfCategories = category;
                response.Operators = Operator;
                response.Roles = Roles;
            }
            return response;
        }

        public ASlabDetailModel GetASlabDetailRole(int SLabID, int RoleID, int OID)
        {
            var response = new ASlabDetailModel();
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = SLabID,
                CommonInt2 = RoleID,
                CommonInt3 = OID
            };
            IProcedure proc = new ProcASlabDetailRole(_dal);
            response = (ASlabDetailModel)proc.Call(req);
            return response;
        }

        public ResponseStatus UpdateAfSlabComm(ASlabDetail req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateAfSlabComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
    }
}
