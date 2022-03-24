using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region PackageRegion
        [HttpGet]
        [Route("Home/PackageMaster")]
        [Route("PackageMaster")]
        public IActionResult PackageMaster()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/Package-Master")]
        [Route("Package-Master")]
        public IActionResult _PackageMaster()
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var model = packageML.GetPackageMaster();
            return PartialView("Partial/_PackageMaster", model);
        }
        [HttpPost]
        [Route("Home/Package-Edit/{id}")]
        [Route("Package-Edit/{id}")]
        public IActionResult _PackageEdit(int ID)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var package = packageML.GetPackageMaster(ID);
            return PartialView("Partial/_PackageCU", package);
        }
        [HttpPost]
        [Route("Home/Package-Edit")]
        [Route("Package-Edit")]
        public IActionResult PackageEdit([FromBody] SlabMaster slabMaster)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var resp = packageML.UpdatePackageMaster(slabMaster);
            return Json(resp);
        }
        [HttpPost]
        [Route("Home/p-update")]
        [Route("p-update")]
        public IActionResult _PackageEdit(int p, int s, bool a)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var resp = packageML.UpdatePackageDetail(p, s, a);
            return Json(resp);
        }
        [HttpPost]
        [Route("Home/Package-Comm")]
        [Route("Package-Comm")]
        public IActionResult _PackageComm(CommonReq req)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);

            if (req.CommonInt2 == 2)
            {
                var package = packageML.GetPackageCommission(req);
                return PartialView("Partial/_PackageCommLVL", package);
            }
            else
            {
                var package = packageML.GetPackageMaster(req.CommonInt);
                return PartialView("Partial/_PackageCU", package);
            }
        }
        [HttpPost]
        [Route("Home/Get-Available-Pkg")]
        [Route("Get-Available-Pkg")]
        public IActionResult _GetAVailablePackageAdmin(int Id)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var package = packageML.AvailablePackage(Id);
            return PartialView("Partial/_AssignAvailablePackage", package);
        }
        [HttpGet]
        [Route("Home/")]
        [Route("Available-Package")]
        public IActionResult GetPackageAvailable()
        {
            IUserML userML = new UserML(_lr);
            var mdl = new AvailablePackageMaster
            {
                IsChannel = !_lr.IsAdminDefined,
                IsEndUser = userML.IsEndUser(),
                IsParent = !userML.IsEndUser() && (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) && (_lr.LoginTypeID != LoginType.CustomerCare)
            };
            return View("PackageAvailable", mdl);
        }
        [HttpPost]
        [Route("Home/")]
        [Route("Get-Availabe-Package")]
        public IActionResult _GetPackageChannel()
        {
            if (ApplicationSetting.IsPackageAllowed)
            {
                IUserML userML = new UserML(_lr);
                var mdl = new AvailablePackageMaster
                {
                    IsChannel = !_lr.IsAdminDefined,
                    IsEndUser = userML.IsEndUser(),
                    IsParent = !userML.IsEndUser() && (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) && (_lr.LoginTypeID != LoginType.CustomerCare)
                };
                IPackageML packageML = new PackageML(_accessor, _env);
                if (mdl.IsEndUser || mdl.IsParent)
                {
                    if (mdl.IsChannel)
                    {
                        mdl.Data = packageML.GetPackageCommissionChannel();
                        return PartialView("Partial/_PackageCommissionChannel", mdl);
                    }
                    else
                    {
                        mdl.Data = packageML.GetPackageCommissionChannel();
                        return PartialView("Partial/_PackageCommissionChannel", mdl);
                    }

                }
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Update-Level-PkgComm")]
        [Route("Update-Level-PkgComm")]
        public IActionResult _UpdateLevelPackageComm([FromBody] PkgLevelCommissionReq req)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var res = packageML.UpdateLevelPackageCommission(req);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/Update-Available-Pkg")]
        [Route("Update-Available-Pkg")]
        public IActionResult UpdateAvaliablePkg(PackageAvailableModel req)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var res = packageML.UpdateAvailablePackage(req);
            return Json(res);
        }
        [HttpPost]
        [Route("UpgradePackage")]
        public IActionResult UpgradePackage(PackageAvailableModel req)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            req.UserId = req.UserId == -1 ? _lr.UserID : req.UserId;
            IPackageML packageML = new PackageML(_accessor, _env);
            var res = packageML.UpgradePackage(req);
            return Json(res);
        }

        [HttpPost]
        [Route("Home/upgrade-package")]
        [Route("upgrade-package")]
        public async Task<IActionResult> UpgradePackage(int pid)
        {
            if (!ApplicationSetting.IsPackageAllowed)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var res = await packageML.UpgradePackage(pid);
            return Json(res);
        }
        [HttpPost]
        [Route("Get-Upgrated-Pkg")]
        public IActionResult _GetUpgratedPackage(int Id)
        {
            //if (!ApplicationSetting.IsPackageAllowed)
            //    return Ok();
            Id = Id == -1 ? _lr.UserID : Id;
            IPackageML packageML = new PackageML(_accessor, _env);
            var package = packageML.GetUpgradePackage(Id);
            return PartialView("Partial/_GetUpgratedPackage", package);
        }
        [HttpPost]
        [Route("Availabe-Packages")]
        public IActionResult _GetAvailablePackages(int Id)
        {
            if (ApplicationSetting.IsPackageAllowed)
            {
                IUserML userML = new UserML(_lr);
                IPackageML packageML = new PackageML(_accessor, _env);
                var result = packageML.GetAvailablePackages(Id);
                return PartialView("Partial/_GetAvailablePackages", result);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Seller-Availabe-Packages")]
        public IActionResult _GetSellerAvailablePackages(int Id, bool IsAddService)
        {
            if (IsAddService)
            {
                var userML = new UserML(_accessor, _env);
                var res = userML.GetAddServiceSts(_lr.LoginTypeID, _lr.UserID, _lr.OutletID);
                res.OutletID = _lr.OutletID;
                return PartialView("Partial/_GetAdditionalService", res);
            }
            else
            {
                if (ApplicationSetting.IsPackageAllowed)
                {
                    IUserML userML = new UserML(_lr);
                    IPackageML packageML = new PackageML(_accessor, _env);
                    var result = packageML.GetAvailablePackages(Id);
                    return PartialView("Partial/_GetAvailablePackages", result);
                }
            }
            return Ok();
        }
        #endregion

        #region DTHPackage
        [HttpGet]
        [Route("/DTHPackage")]
        public IActionResult DTHPackage()
        {
            return View();
        }

        [HttpPost]
        [Route("/_DTHPackage")]
        public IActionResult _DTHPackage(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHPackage(id, 0);
            return PartialView("Partial/_DTHPackage", _res);
        }

        [HttpPost]
        [Route("/_DTHPackageByID")]
        public IActionResult _DTHPackageByID(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            DTHPackageRes pacDetail = new DTHPackageRes();
            if (id > 0)
            {
                var _res = ml.GetDTHPackage(id, 0);
                pacDetail.package = _res.FirstOrDefault();
                pacDetail.Operators = ml.GetOperators().Where(x => x.OpType == pacDetail.package.OPTypeID).ToList();
            }
            pacDetail.OpTypes = ml.GetOptypes().Where(x => x.ServiceTypeID == ServiceType.DTHSubscription).ToList();
            return PartialView("Partial/_EditDTHPackage", pacDetail);
        }



        [HttpPost]
        [Route("/_EditDTHPackage")]
        public IActionResult _EditDTHPackage(DTHPackage req)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.SaveDTHPackage(req);
            return Json(_res);
        }

        [HttpPost]
        [Route("/_Add-Bulk-DTHPackage")]
        public IActionResult _AddBulkDTHPackage(List<DTHPackage> req)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.SaveBulkDTHPackage(req);
            return Json(_res);
        }

        [HttpPost]
        [Route("/_bindDTHOperator")]
        public IActionResult _bindDTHOperator(int OpType)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetOperators().Where(x => x.OpType == OpType).ToList();
            return Json(_res);
        }

        [HttpGet]
        [Route("/DTHChannelMaster")]
        public IActionResult DTHChannelMaster()
        {
            return View();
        }

        [HttpPost]
        [Route("/_DTHChannel")]
        public IActionResult _DTHChannel(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHChannel(id);
            return PartialView("Partial/_DTHChannel", _res);
        }


        [HttpPost]
        [Route("/_DTHChannel-ByID")]
        public IActionResult _DTHChannelByID(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            DTHChannel _res = new DTHChannel();
            if (id > 0)
            {
                _res = ml.GetDTHChannel(id).FirstOrDefault();
            }
            _res.categories = ml.GetChannelCategory();
            return PartialView("Partial/_EditDTHChannel", _res);
        }


        [HttpPost]
        [Route("/_Edit-DTHChannel")]
        public IActionResult _EditDTHChannel(IFormFile file, string req)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = -1
            };
            if (file != null)
            {
                res = Validate.O.IsImageValid(file);
                if (res.Statuscode == ErrorCodes.One)
                {
                    var detail = JsonConvert.DeserializeObject<DTHChannel>(req);
                    IOperatorML ml = new OperatorML(_accessor, _env);
                    res = ml.SaveDTHChannel(detail);
                    if (res.Statuscode == ErrorCodes.One && detail.Del == false)
                    {
                        IResourceML _bannerML = new ResourceML(_accessor, _env);
                        res = _bannerML.UploadChannelImage(file, _lr, res.CommonInt);
                        res.Msg = res.Statuscode == ErrorCodes.One ? "Channel saved successfully" : res.Msg;
                    }
                }
            }
            else
            {
                res.Msg = "Please Select image";
            }
            return Json(res);
        }

        [HttpPost]
        [Route("/_MapChannel")]
        public IActionResult _MapChannel(int packageID)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.MapChannelToPack(packageID);
            return PartialView("Partial/_MapChannelToPackage", _res);
        }

        [HttpGet]
        [Route("/PaymentModeSetting")]
        public IActionResult PaymentModeSettings()
        {
            return View();
        }

        [HttpPost]
        [Route("/_PaymentModeSetting")]
        public IActionResult _PaymentModeSettings()
        {
            IBankML ml = new BankML(_accessor, _env);
            var _list = ml.GetAllPaymentMode();
            return PartialView("Partial/_PaymentModeSetting", _list);
        }

        [HttpPost]
        [Route("/_Payment-Mode-Setting")]
        public IActionResult _SavePaymentModeSetting(PaymentModeMaster req)
        {
            IBankML ml = new BankML(_accessor, _env);
            var _res = ml.SavePaymentModeSetting(req);
            return Json(_res);
        }

        [HttpPost]
        [Route("/DTHChannelMapping")]
        public IActionResult SaveChannelMapping(DTHChannelMap req)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.SaveChannelMapping(req);
            return Json(_res);
        }

        [HttpGet]
        [Route("/DTHChannelCategory")]
        public IActionResult DTHChannelCategory()
        {
            return View();
        }

        [HttpPost]
        [Route("/_DTHChannelCategory")]
        public IActionResult _DTHChannelCategory(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHChannelCategory(id);
            return PartialView("Partial/_DTHChannelCategory", _res);
        }


        [HttpPost]
        [Route("/_DTHChannelCategoryByID")]
        public IActionResult _DTHChannelCategoryByID(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            DTHChannelCategory pacDetail = new DTHChannelCategory();
            if (id > 0)
            {
                pacDetail = ml.GetDTHChannelCategory(id).FirstOrDefault();

            }
            return PartialView("Partial/_DTHChannelCategoryByID", pacDetail);
        }


        [HttpPost]
        [Route("/Save-DTHChannel-Category")]
        public IActionResult _EditDTHChannelCategory(DTHChannelCategory req)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.SaveDTHChannelCategory(req);
            return Json(_res);
        }
        [HttpPost]
        [Route("DTH-Commission-Detail")]
        public IActionResult _DTHCommissionDetail(int SlabID, int OID)
        {
            IUserML userML = new UserML(_lr);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            ISlabML slabML = new SlabML(_accessor, _env);
            if (IsAdmin)
            {
                DTHCommissionModel _res = slabML.GetDTHCommissionDetail(SlabID, OID);
                _res.OID = OID;
                if (_res.IsAdminDefined)
                {
                    return PartialView("Partial/_DTHCommissionLvl", _res);
                }
                return PartialView("Partial/_DTHCommission", _res);
            }
            else if (!_lr.IsAdminDefined && !userML.IsEndUser())
            {
                DTHCommissionModel _res = slabML.GetDTHCommissionDetail(SlabID, OID);
                _res.IsChannel = true;
                return PartialView("Partial/_DTHCommission", _res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Update-DTHCommission")]
        public IActionResult _UpdateDTHCommission([FromBody] DTHCommission DTHCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateDTHCommission(DTHCommission);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Admin/UpdateDTHstatus/{id}")]
        [Route("UpdateDTHstatus/{id}")]
        public IActionResult UpdateDTHstatus(int ID, string TransactionID)
        {
            IUserML IuML = new UserML(_accessor, _env);
            DTHSubscriptionReport data = IuML.GetBookingStatus(ID, TransactionID);
            return PartialView("Partial/_UpdateDTHStatus", data);
        }
        #endregion

        #region VASPackage
        [HttpGet]
        [Route("Home/VASPackageMaster")]
        [Route("VASPackageMaster")]
        public IActionResult VASPackageMaster()
        {
            return View();
        }

        [Route("Home/VASPackage-Master")]
        [Route("VASPackage-Master")]
        public IActionResult _VASPackageMaster()
        {
            if (!ApplicationSetting.IsVASAPIResale )
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var model = packageML.VASGetPackageMaster();
            return PartialView("Partial/_VASPackageMaster", model);
        }

        [Route("Home/VASPackage-Edit/{id}")]
        [Route("VASPackage-Edit/{id}")]
        public IActionResult _VASPackageEdit(int ID)
        {
            if (!ApplicationSetting.IsVASAPIResale)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var package = packageML.VASGetPackageMaster(ID);
            return PartialView("Partial/_VASPackageCU", package);
        }

        [HttpPost]
        [Route("Home/VAS-Package-Edit")]
        [Route("VAS-Package-Edit")]
        public IActionResult VASPackageEdit([FromBody] SlabMaster slabMaster)
        {
            if (!ApplicationSetting.IsVASAPIResale)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var resp = packageML.UpdateVASPackageMaster(slabMaster);
            return Json(resp);
        }

        [HttpPost]
        [Route("Home/vp-update")]
        [Route("vp-update")]
        public IActionResult _VASPackageEdit(int p, int s, bool a,bool IsChargeable,int Charge)
        {
            if (!ApplicationSetting.IsVASAPIResale)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var resp = packageML.UpdateVASPackageDetail(p, s, a,IsChargeable,Charge);
            return Json(resp);
        }

        [HttpGet]
        [Route("Home/VASPlanMaster")]
        [Route("VASPlanMaster")]
        public IActionResult VASPlanMaster()
        {

            if (!ApplicationSetting.IsVASAPIResale)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var model = packageML.VASPackageMasterForPlans();
            return View(model);
        }

        [HttpPost]
        [Route("Home/vp-buy-plan")]
        [Route("vp-buy-plan")]
        public IActionResult VASBuyOrRenewPlan(int p, bool a)
        {
            if (!ApplicationSetting.IsVASAPIResale)
                return Ok();
            IPackageML packageML = new PackageML(_accessor, _env);
            var resp = packageML.VASPlanBuyOrRenew(p, a,_lr.UserID,_lr.LoginTypeID);
            return Json(resp);
        }

        #endregion

    }
}
