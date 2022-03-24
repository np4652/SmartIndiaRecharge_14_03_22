using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interface;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region SlabRegion
        [HttpGet]
        [Route("Home/SlabMaster")]
        [Route("SlabMaster")]
        public IActionResult SlabMaster()
        {
            IUserML userML = new UserML(_lr);
            bool IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                var res = new SlabModel();
                res.IsAdmin = IsAdmin;
                res.IsWebsite = _lr.IsWebsite;
                return View(res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Slab-Master")]
        [Route("Slab-Master")]
        public IActionResult _SlabMaster()
        {
            IUserML userML = new UserML(_lr);
            var slabModel = new SlabModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB),
                IsWebsite = _lr.IsWebsite
            };
            if ((slabModel.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ISlabML slabML = new SlabML(_accessor, _env);
                slabModel.slabMasters = slabML.GetSlabMaster();
                return PartialView("Partial/_SlabMaster", slabModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Slab-Edit/{id}")]
        [Route("Slab-Edit/{id}")]
        public IActionResult _SlabEdit(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            var slabModel = new SlabModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB),
                IsWebsite = _lr.IsWebsite
            };
            var DMRModels = userML.GetDMRModelList();
            if (DMRModels.Any())
            {
                slabModel.DMRModelSelect = new SelectList(DMRModels, "ID", "Name");
            }
            if ((slabModel.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ISlabML slabML = new SlabML(_accessor, _env);
                slabModel.slabMaster = slabML.GetSlabMaster(ID);

                return PartialView("Partial/_SlabCU", slabModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Slab-Edit")]
        [Route("Slab-Edit")]
        public IActionResult SlabEdit([FromBody] SlabMaster slabMaster)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ISlabML slabML = new SlabML(_accessor, _env);
                var resp = slabML.UpdateSlabMaster(slabMaster);
                return Json(resp);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Home/Range-Slab-Detail")]
        [Route("Range-Slab-Detail")]
        public IActionResult _RangeSlabDetail(int SlabID, int OTypeID = 0)
        {
            IUserML userML = new UserML(_accessor, _env);
            ISlabML slabML = new SlabML(_accessor, _env);
            RangeDetailModel slabDetailModel = slabML.GetSlabDetailRange(SlabID, OTypeID);
            slabDetailModel.OpTypeID = OTypeID;
            slabDetailModel.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            IOperatorML opml = new OperatorML(_accessor, _env);
            slabDetailModel.OpTypes = opml.GetOptypeInRange().ToList();
            var DMRModels = userML.GetDMRModelList();
            if (DMRModels.Any())
            {
                slabDetailModel.DMRModelSelect = new SelectList(DMRModels, "ID", "Name");
            }
            if (slabDetailModel.IsAdmin)
            {
                if (slabDetailModel.IsAdminDefined)
                    return PartialView("Partial/_RangeSlabDetailLVL", slabDetailModel);
                return PartialView("Partial/_RangeSlabDetail", slabDetailModel);
            }
            else if (!_lr.IsAdminDefined && !userML.IsEndUser())
            {
                slabDetailModel.IsChannel = true;
                return PartialView("Partial/_RangeSlabDetail", slabDetailModel);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Home/RangSlabDetail")]
        [Route("RangSlabDetail")]
        public IActionResult RangSlabDetail([FromBody] RangeCommission slabCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateRangeSlabDetail(slabCommission);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/R-Slab-Detail")]
        [Route("R-Slab-Detail")]
        public IActionResult _RSlabDetail(int SlabID, int OpTypeID)
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            ISlabML slabML = new SlabML(_accessor, _env);
            IOperatorML opml = new OperatorML(_accessor, _env);
            if (IsAdmin)
            {
                SlabDetailModel slabDetailModel = slabML.GetSlabDetail(SlabID, OpTypeID);
                slabDetailModel.OpTypes = opml.GetOptypeInSlab().ToList();
                if (slabDetailModel.IsAdminDefined)
                {
                    return PartialView("Partial/_SlabDetailLVL", slabDetailModel);
                }

                return PartialView("Partial/_SlabDetail", slabDetailModel);
            }
            else if (!_lr.IsAdminDefined && !userML.IsEndUser())
            {
                SlabDetailModel slabDetailModel = slabML.GetSlabDetail(SlabID, OpTypeID);
                slabDetailModel.OpTypes = opml.GetOptypeInSlab().ToList();
                slabDetailModel.IsChannel = true;
                return PartialView("Partial/_SlabDetail", slabDetailModel);
            }
            return Ok();
        }
        //[HttpPost]
        //[Route("Home/MLM-Slab-Detail")]
        //[Route("MLM-Slab-Detail")]
        //public IActionResult _Mlmslabdetails(int SlabID, int OpTypeID)
        //{
        //    string returnUrl = string.Empty;
        //    MLM_SlabDetailModel mlmslabdetails = new MLM_SlabDetailModel();
        //    IUserML userML = new UserML(_lr);
        //    var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
        //    ISlabML slabML = new SlabML(_accessor, _env);
        //    I_MlmSlabML mlmslabml = new MLMSlabML(_accessor, _env);
        //       IOperatorML opml = new OperatorML(_accessor, _env);

        //    if (IsAdmin)
        //    {
        //        if (ApplicationSetting.IsMultiLevel == true)
        //        {
        //            mlmslabdetails = mlmslabml.MLM_GetSlabDetail(SlabID, OpTypeID);
        //            mlmslabdetails.OpTypes = opml.GetOptypeInSlab().ToList();
        //            returnUrl = "Partial/MLM_SlabDetail";
        //            return PartialView(returnUrl, mlmslabdetails);
        //        }
        //    }
        //    return Ok();
        //}
        [HttpPost]
        [Route("Home/MLM-Slab-Detail")]
        [Route("MLM-Slab-Detail")]
        public IActionResult _Mlmslabdetails(int SlabID, int OpTypeID)
        {
            MLM_SlabDetailModel mlmslabdetails = new MLM_SlabDetailModel();
            IUserML userML = new UserML(_lr);
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            ISlabML slabML = new SlabML(_accessor, _env);
            I_MlmSlabML mlmslabml = new MLMSlabML(_accessor, _env);
            IOperatorML opml = new OperatorML(_accessor, _env);

            if (IsAdmin)
            {
                if (ApplicationSetting.IsMultiLevel == true)
                {
                    mlmslabdetails = mlmslabml.MLM_GetSlabDetail(SlabID, OpTypeID);
                    mlmslabdetails.OpTypes = opml.GetOptypeInSlab().ToList();
                    return PartialView("Partial/MLM_SlabDetail", mlmslabdetails);
                }
            }
            return Ok();
        }


        [HttpPost]
        [Route("Home/RSlabDetail")]
        [Route("RSlabDetail")]
        public IActionResult RSlabDetail([FromBody] SlabCommission slabCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateSlabDetail(slabCommission);
            return Json(_resp);
        }
        [HttpPost]
        [Route("MLM_RSlabDetail")]
        public IActionResult MLM_RSlabDetail(MLM_SlabCommission mlmslabCommission)
        {
            I_MlmSlabML mlmslabML = new MLMSlabML(_accessor, _env);
            IResponseStatus _resp = mlmslabML.MLM_UpdateSlabDetail(mlmslabCommission);
            return Json(_resp);
        }

        [HttpPost, Route("UpdateBulkSlabDetail")]
        public IActionResult UpdateBulkSlabDetail(SlabCommissionReq req)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateBulkSlabDetail(req);
            return Json(_resp);
        }

        [HttpPost]
        [Route("/Copy-Slab")]
        public IActionResult CopySlabDetail(int SlabID, string SlabName)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.CopySlabDetail(SlabID, SlabName);
            return Json(_resp);
        }
        [HttpPost]
        [Route("/update-dmodid-range")]
        public IActionResult UpdateDMRModelIDSlab(int s, int o, int d)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            var _resp = slabML.UpdateDMRModelForSlabDetail(o, s, d);
            return Json(_resp);
        }
        [HttpPost]
        [Route("/update-dmodid-range-api")]
        public IActionResult UpdateDMRModelIDSlabAPI(int a, int o, int d)
        {
            IAPIML apiML = new APIML(_accessor, _env);
            var _resp = apiML.UpdateDMRModelForAPI(o, a, d);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/signup-slab")]
        [Route("signup-slab")]
        public IActionResult _SignupSlab(int SlabID)
        {
            IUserML userML = new UserML(_lr);
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            ISettingML settingML = new SettingML(_accessor, _env);
            if (IsAdmin)
            {
                res.Msg = ErrorCodes.TempError;
                if (settingML.UpdateSignupSlab(SlabID))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                }
            }
            return Json(res);
        }

        [Route("/SlabCommissionSetting")]
        public IActionResult SlabCommissionSetting()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var modal = new SlabCommissionSetting
            {
                OpTypes = opml.GetOptypeInSlab(),
                Operators = opml.GetOperators(OPTypes.Prepaid)
            };
            return View(modal);
        }

        [HttpPost]
        [Route("/loadOperator")]
        public IActionResult loadOperator(int OPID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var Operators = opml.GetOperators(OPID);
            return Json(Operators);
        }

        [HttpPost]
        [Route("/_SlabCommissionSetting")]
        public IActionResult _SlabCommissionSetting(int OID)
        {
            ISlabML ml = new SlabML(_accessor, _env);
            var res = ml.SlabCommissionSetting(OID);
            return PartialView("Partial/_SlabCommissionSetting", res);
        }

        [HttpPost]
        [Route("Home/CircleSlab")]
        [Route("CircleSlab")]
        public IActionResult CircleSlab(int SlabID)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var OpML = new OperatorML(_accessor, _env);
                var circleSlabModel = new CircleSlabModel
                {
                    SlabID = SlabID,
                    Ops = OpML.GetOperators(OPTypes.Prepaid)
                };
                return PartialView("Partial/_CircleSlabParent", circleSlabModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Circle-Slab-Setting")]
        [Route("Circle-Slab-Setting")]
        public IActionResult CircleSlab(int SlabID, int OID)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                ISlabML ml = new SlabML(_accessor, _env);
                var res = ml.CircleSlabGet(SlabID, OID);
                return PartialView("Partial/_CircleSlabSetting", res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/update-circle-slab")]
        [Route("update-circle-slab")]
        public IActionResult CircleSlabUpdate([FromBody] SlabCommission slabCommission)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                ISlabML slabML = new SlabML(_accessor, _env);
                IResponseStatus _resp = slabML.UpdateCircleSlab(slabCommission);
                return Json(_resp);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Home/CircleSlab-A")]
        [Route("CircleSlab-A")]
        public IActionResult CircleSlabA(int APIID)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var OpML = new OperatorML(_accessor, _env);
                var circleSlabModel = new CircleSlabModel
                {
                    APIID = APIID,
                    Ops = OpML.GetOperators(OPTypes.Prepaid)
                };
                return PartialView("Partial/_CircleSlabParentA", circleSlabModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Circle-Slab-Setting-A")]
        [Route("Circle-Slab-Setting-A")]
        public IActionResult CircleSlabA(int APIID, int OID)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                ISlabML ml = new SlabML(_accessor, _env);
                var res = ml.CircleSlabAPIGet(APIID, OID);
                return PartialView("Partial/_CircleSlabSettingAPI", res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/update-circle-slab-A")]
        [Route("update-circle-slab-A")]
        public IActionResult CircleSlabUpdateA([FromBody] SlabCommission slabCommission)
        {
            if (ApplicationSetting.IsCircleSlabShow && _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                ISlabML slabML = new SlabML(_accessor, _env);
                IResponseStatus _resp = slabML.UpdateCircleSlabAPI(slabCommission);
                return Json(_resp);
            }
            return Ok();
        }
        #endregion

        #region special-slab
        [HttpPost]
        [Route("Home/Special-Slab-Detail")]
        [Route("Special-Slab-Detail")]
        public IActionResult _SpecialSlabDetail(int SlabID, bool IsAdminDefined, int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var opList = opml.GetOperators().Where(x => x.OpType == OpTypeID);
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            var circleList = ml.CircleListWithAll().Result;
            var Model = new DenominationDetailModal
            {
                SlabID = SlabID,
                IsAdminDefined = IsAdminDefined,
                Operator = opList.ToList(),
                CirlceList = circleList
            };
            return PartialView("Partial/_SpecialSlabDetail", Model);
        }

        [HttpPost]
        [Route("SpecialSlabDetailRole")]
        public async Task<IActionResult> _SpecialSlabDetailRole(CircleWithDomination param)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var circleDomList = await opml.GetCircleWithDominations(param).ConfigureAwait(false);

            return PartialView("Partial/_SpecialSlabDetailRole", circleDomList);
        }

        [HttpPost]
        [Route("SpecialSlabDetailSaving")]
        public async Task<IActionResult> _SpecialSlabDetailSaving(CircleWithDomination param)
        {
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            IOperatorML opml = new OperatorML(_accessor, _env);
            var circleDomList = await opml.GetRemainDominationsSpecialSlab(param).ConfigureAwait(false);
            var model = new DenominationDetailModal
            {
                circleWithDominations = circleDomList
            };
            return PartialView("Partial/_SpecialSlabDetailSaving", model);
        }

        [HttpPost]
        [Route("Home/Special-Incentive-Edit")]
        [Route("Special-Incentive-Edit")]
        public IActionResult _SpecialIncentiveEdit(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveSpecialSlabDetail(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/Special-Domination-Update")]
        [Route("Special-Domination-Update")]
        public IActionResult _SpecialDominationUpdate(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.UpdateSpecialSlabDomID(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/Special-Group-Domination-Update")]
        [Route("Special-Group-Domination-Update")]
        public IActionResult _SpecialGroupDominationUpdate(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.UpdateGroupSpecialSlabDomID(param);
            return Json(_resp);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSlabAsync(int slabId)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            var resp = await slabML.DeleteSlab(slabId, _lr.UserID).ConfigureAwait(true);
            return Json(resp);
        }
        #endregion

        [HttpPost]
        [Route("GIComm")]
        public IActionResult GiCommission(int SlabID, int OpTypeID)
        {
            ViewBag.OpTypeID = OpTypeID;
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
            {
                IUserML userML = new UserML(_lr);
                ISlabML slabML = new SlabML(_accessor, _env);
                var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                IOperatorML opml = new OperatorML(_accessor, _env);
                if (IsAdmin)
                {
                    SlabDetailModel slabDetailModel = slabML.GetSlabDetailGI(SlabID, OpTypeID);
                    slabDetailModel.OpTypes = opml.GetOptypes(ServiceType.GenralInsurance).ToList();
                    if (slabDetailModel.IsAdminDefined)
                    {
                        return PartialView("Partial/_GICommLVL", slabDetailModel);
                    }
                    return PartialView("Partial/_GIComm", slabDetailModel);
                }
                else if (!_lr.IsAdminDefined && !userML.IsEndUser())
                {
                    SlabDetailModel slabDetailModel = slabML.GetSlabDetail(SlabID, OpTypeID);
                    slabDetailModel.OpTypes = opml.GetOptypes(ServiceType.GenralInsurance).ToList();
                    slabDetailModel.IsChannel = true;
                    return PartialView("Partial/_GIComm", slabDetailModel);
                }
            }
            return Ok();
        }
        [HttpPost]
        [Route("I/UTPslabDetail")]
        public IActionResult TPslabDetail([FromBody] SlabCommission slabCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateSlabDetailGI(slabCommission);
            return Json(_resp);
        }
    }
}