using System.Collections.Generic;
using System.Linq;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region OperatorRegion
        [HttpGet]
        [Route("Home/OperatorMaster")]
        [Route("OperatorMaster")]
        public IActionResult OperatorMaster()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var _OpTypes = opml.GetOptypeInSlab();
            return View(_OpTypes);
        }

        [HttpPost]
        [Route("Home/Operator-Master")]
        [Route("Operator-Master")]
        public IActionResult _OperatorMaster(int opTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IEnumerable<OperatorDetail> list = opml.GetOperators().Where(x => x.OpType == (opTypeID == 0 ? OPTypes.Prepaid : opTypeID));
            return PartialView("Partial/_OperatorMaster", list);
        }

        [HttpPost]
        [Route("Home/Operator-Edit/{id}")]
        [Route("Operator-Edit/{id}")]
        public IActionResult _OperatorEdit(int ID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            OperatorDetail operatorDetail = opml.GetOperator(ID);
            ViewBag.OpTypeMasters = opml.GetOptypes();
            ViewBag.GetExactness = opml.GetExactness();
            return PartialView("Partial/_OperatorCU", operatorDetail);
        }
        [HttpPost]
        [Route("DeleteOperatorOption")]
        public IActionResult DeleteOperatorOption(int OptionType, int OID)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            var res = operatorML.DeleteOperatorOption(OptionType, OID);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/Operator-Edit")]
        [Route("Operator-Edit")]
        public IActionResult OperatorEdit([FromBody] OperatorDetail operatorDetail)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IResponseStatus _resp = opml.SaveOperator(operatorDetail);
            return Json(_resp);
        }
        [HttpPost]
        [Route("upload-OperatorIcon")]
        public IActionResult uploadOperatorIcon(IFormFile file, int OID)
        {
            IResourceML mL = new ResourceML(_accessor, _env);
            var _res = mL.UploadOperatorIcon(file, OID);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/OPID")]
        [Route("OPID")]
        public IActionResult OperatorEdit(int ID, string OPID)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (OPID == null || ID == 0)
                return Json(_resp);
            OperatorDetail operatorDetail = new OperatorDetail
            {
                OID = ID,
                OPID = OPID,
                IsOPID = true
            };
            IOperatorML opml = new OperatorML(_accessor, _env);
            _resp = opml.SaveOperator(operatorDetail);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/billerid")]
        [Route("billerid")]
        public IActionResult OperatorBillerID(int ID, string billerID)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (billerID == null || ID == 0)
                return Json(_resp);
            var operatorDetail = new OperatorDetail
            {
                OID = ID,
                BillerID = billerID,
                IsOPID = true
            };
            IOperatorML opml = new OperatorML(_accessor, _env);
            _resp = opml.UpdateBillerID(operatorDetail);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/update-biller")]
        [Route("update-biller")]
        public IActionResult UpdateBillerID(int ID, int OpID)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (ID == 0)
                return Json(_resp);

            IOperatorML opml = new OperatorML(_accessor, _env);
            if (ID == -1)
            {
                _resp = opml.UpdateAllBillers(OpID);
            }
            else
            {
                _resp = opml.UpdateBillerInfo(ID);
            }

            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/TollFree")]
        [Route("TollFree")]
        public IActionResult TollFreeEdit(int ID, string TollFree)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (TollFree == null || ID == 0)
                return Json(_resp);
            OperatorDetail operatorDetail = new OperatorDetail
            {
                OID = ID,
                IsOPID = true,
                OPID = "1",
                TollFree = TollFree
            };
            IOperatorML opml = new OperatorML(_accessor, _env);
            _resp = opml.SaveTollFree(operatorDetail);
            return Json(_resp);
        }

        [HttpPost]
        [Route("upload-operator-pdf")]
        public IActionResult UploadOperatorPDF(IFormFile file, int oid)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var _res = opml.UploadOperatorPDF(file, oid, _lr);
            return Json(_res);
        }

        [Route("OptionaMaster")]
        public IActionResult _OptionaMaster(int ID)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return PartialView("Partial/_OperatorWiseOptionalMaster", operatorML.OperatorOption(ID));
        }
        [HttpPost]
        [Route("OperatorOptional")]
        public IActionResult OperatorOptional(int OPId, int ID, string name, string remark, bool islist, bool ismulti, int OptionalType)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return Json(operatorML.UpdateOption(new OperatorOptionalReq
            {
                DisplayName = name,
                ID = ID,
                IsList = islist,
                IsMultiSelection = ismulti,
                OID = OPId,
                OptionalType = OptionalType,
                Remark = remark
            }));
        }

        [HttpPost]
        [Route("Home/API-Op-Code")]
        [Route("API-Op-Code")]
        public IActionResult APIOpCode([FromBody] APIOpCode aPIOpCode)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            IResponseStatus _resp = ml.UpdateAPIOpCode(aPIOpCode);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/API-Op-Code-circle")]
        [Route("API-Op-Code-circle")]
        public IActionResult APIOpCodeCircle([FromBody] APIOpCode aPIOpCode)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            IResponseStatus _resp = ml.UpdateAPIOpCodeCircle(aPIOpCode);
            return Json(_resp);
        }
        [HttpGet]
        [Route("Home/APIOpCode")]
        [Route("APIOpCode")]
        public IActionResult APIOpCode()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var _OpTypes = opml.GetOptypeInSlab();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID > 1 && _lr.IsWLAPIAllowed)
            {
                _OpTypes = _OpTypes.Where(x => x.ID.In(OPTypes.AllowToWhitelabel));
            }
            return View(_OpTypes);
        }
        [HttpPost]
        [Route("Home/API-OpCode")]
        [Route("API-OpCode")]
        public IActionResult _APIOpCode(int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var reqModel = new APIDetailModel
            {
                aPIOpCodes = opml.GetAPIOpCode(OpTypeID)
            };
            return PartialView("Partial/_APIOpCode", reqModel);
        }
        [HttpPost]
        [Route("Home/API-OpCode-Circle")]
        [Route("API-OpCode-Circle")]
        public IActionResult APIOpCodeCircle(int A, int O)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IEnumerable<APIOpCode> list = opml.GetAPIOpCodeCircle(O, A);
            return PartialView("Partial/_APIOpCodeCircle", list);
        }
        [HttpPost]
        [Route("/get-MaxOpCode")]
        public IActionResult getMaxOpCode(int OpType)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var res = opml.getMaxOpCode(OpType);
            return Json(res);
        }

        [HttpPost]
        [Route("changeValidationType")]
        public IActionResult changeValidationType(int OID, int CircleValidationType)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IResponseStatus _resp = opml.changeValidationType(OID, CircleValidationType);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/u-amt-valdt")]
        [Route("u-amt-valdt")]
        public IActionResult UpdateAmountValidate(int oid, bool sts)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (oid == 0)
                return Json(_resp);

            IOperatorML opml = new OperatorML(_accessor, _env);
            _resp = opml.UpdateAmtVal(oid, sts);

            return Json(_resp);
        }
        #endregion

        #region RNPPLAN
        [HttpPost]
        [Route("Home/u-rech-plan")]
        [Route("u-rech-plan")]
        public IActionResult UpdateRechPlans(int oid, int opType)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (oid == 0)
                return Json(_resp);

            IOperatorML opml = new OperatorML(_accessor, _env);
            if (opType == OPTypes.Prepaid)
                _resp = opml.UpdateRechPlans(oid);
            else if (opType == OPTypes.DTH)
                _resp = opml.UpdateDTHPlans(oid);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/map-rnp-op")]
        [Route("map-rnp-op")]
        public IActionResult MapRNPOperator(int oid,int opType,string opName)
        {
            var data = new MapOperatorReq();
            IOperatorML opml = new OperatorML(_accessor, _env);
            data.OperatorList = opml.GetOperators();
            //.Where(x => x.OpType == (opType == 0 ? OPTypes.Prepaid : opType));
            data.OID = oid;
            data.OpTypeID = opType;
            data.OpName = opName;
            return PartialView("Partial/_MapOperators", data);
        }
        [HttpPost]
        [Route("Home/m-p-op")]
        [Route("m-p-op")]
        public IActionResult MapRNPOperator(int tooid, int tomapoid)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var resp = opml.MapPlansOperator(tooid, tomapoid);
            return Json(resp);
        }
        #endregion

    }
}
