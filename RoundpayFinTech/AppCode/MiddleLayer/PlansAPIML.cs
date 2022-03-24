using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PlansAPIML : IMplan, IRoundpayPlan, IPlanAPIPlan, ICyrusAPIPlan, IVastWebPlan, IMyPlanAPI
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;
        private readonly ISession _session;
        public PlansAPIML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        public PlansAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }


        #region IMplan
        public SubMplanRofferResp GetRoffer(string AccountNo, int OID)
        {
            var subMplanRofferResp = new SubMplanRofferResp();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanRofferResp = JsonConvert.DeserializeObject<SubMplanRofferResp>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanRofferResp;
        }

        public SubMplanSimplePlanResp GetSimplePlan(int CircleID, int OID)
        {
            var subMplanSimplePlanResp = new SubMplanSimplePlanResp();
            if (CircleID > 0 && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.SIMPLE,
                    CommonStr = "",
                    CommonInt = OID,
                    CommonInt2 = CircleID
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanSimplePlanResp = JsonConvert.DeserializeObject<SubMplanSimplePlanResp>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetSimplePlan",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanSimplePlanResp;
        }

        public SubMplanDTHSimplePlanResp GetDTHSimplePlan(int OID)
        {
            var subMplanDTHSimplePlanResp = new SubMplanDTHSimplePlanResp();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanSIMPLE,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<SubMplanDTHSimplePlanResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return subMplanDTHSimplePlanResp;
        }

        public SubMplanDTHSimplePlanResp GetDTHChannelPlan(int OID)
        {
            var subMplanDTHSimplePlanResp = new SubMplanDTHSimplePlanResp();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanCHANNEL,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<SubMplanDTHSimplePlanResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHChannelPlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return subMplanDTHSimplePlanResp;
        }

        public SubMplanDTHCustomerInfo GetDTHCustInfo(string AccountNo, int OID)
        {
            var respDTHPInfo = new SubMplanDTHCustomerInfo();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHCustInfo,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    respDTHPInfo = JsonConvert.DeserializeObject<SubMplanDTHCustomerInfo>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHCustInfo",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHPInfo;
        }

        public MplanDTHHeavyRefresh GetDTHHeavyRefresh(string AccountNo, int OID)
        {
            var respDTHHeavyRefresh = new MplanDTHHeavyRefresh();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHHeavyRefresh,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    //resp = "{\"tel\":\"209048456\",\"operator\":\"Videocon\",\"records\":{\"desc\":\"Reauthorized Successfully.\",\"status\":1},\"status\":1}";
                }
                if (resp != "")
                {
                    respDTHHeavyRefresh = JsonConvert.DeserializeObject<MplanDTHHeavyRefresh>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHHeavyRefresh",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHHeavyRefresh;
        }
        #endregion
        #region IRoundpayPlan
        public SubMplanRofferResp GetRofferRoundpay(string AccountNo, int OID)
        {
            var subMplanRofferResp = new SubMplanRofferResp();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanRofferResp = JsonConvert.DeserializeObject<SubMplanRofferResp>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanRofferResp;
        }
        public RoundpaySimplePlanResp GetSimplePlanRoundpay(int CircleID, int OID)
        {
            var subMplanSimplePlanResp = new RoundpaySimplePlanResp();
            if (CircleID > 0 && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.SIMPLE,
                    CommonStr = "",
                    CommonInt = OID,
                    CommonInt2 = CircleID
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanSimplePlanResp = JsonConvert.DeserializeObject<RoundpaySimplePlanResp>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetSimplePlan",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanSimplePlanResp;
        }
        public RoundpaySubMplanDTHSimplePlanResp GetDTHSimplePlanRoundpay(int OID)
        {
            RoundpaySubMplanDTHSimplePlanResp RoundpayMplanDTHSimplePlanResp = new RoundpaySubMplanDTHSimplePlanResp();
            //var subMplanDTHSimplePlanResp = new SubMplanDTHSimplePlanResp();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanSIMPLE,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    RoundpayMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<RoundpaySubMplanDTHSimplePlanResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return RoundpayMplanDTHSimplePlanResp;
        }
        public RPDTHPlansSimpleOfPackages RPDTHSimplePlansOfPackages(int OID)
        {
            RPDTHPlansSimpleOfPackages objDTHPlans = new RPDTHPlansSimpleOfPackages();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanForChRP,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    //aPIDetail.URL = "http://rechargeplans.co.in/plansservices.asmx/DTHPlansNew?UMobile=9044004486&Token=8adda5c1681b9cdaaabd10d3bd90dad2&OperatorID=AD&ZoneID=RI";
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    objDTHPlans = JsonConvert.DeserializeObject<RPDTHPlansSimpleOfPackages>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "RPDTHSimplePlansOfPackages",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return objDTHPlans;
        }
        public RPDTHPlansSimpleOfPackages RPDTHPlanListByLanguage(int OID, string Language)
        {
            RPDTHPlansSimpleOfPackages objDTHPlans = new RPDTHPlansSimpleOfPackages();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanLANGUAGE,
                CommonStr = "",
                CommonStr3 = Language,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    //aPIDetail.URL = "http://rechargeplans.co.in/plansservices.asmx/GetDTHPlanListByLanguage?OperatorID=AD&ZoneID=RI&Langauge=Bengali";
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    objDTHPlans = JsonConvert.DeserializeObject<RPDTHPlansSimpleOfPackages>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "RPDTHPlanListByLanguage",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return objDTHPlans;
        }
        public RPGetDTHChannelList RPDTHSimplePlansChannelList(string PackageID, int OID)
        {
            var objRPGetDTHChannelList = new RPGetDTHChannelList();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanCHANNEL,
                CommonStr = "",
                CommonStr2 = PackageID,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    objRPGetDTHChannelList = JsonConvert.DeserializeObject<RPGetDTHChannelList>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHChannelPlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return objRPGetDTHChannelList;
        }
        public SubMplanDTHCustomerInfo GetDTHCustInfoRoundpay(string AccountNo, int OID)
        {
            var respDTHPInfo = new SubMplanDTHCustomerInfo();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHCustInfo,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    if (resp.Contains("Customer Not Found"))
                        respDTHPInfo.Status = ErrorCodes.Minus1;
                    else
                        respDTHPInfo = JsonConvert.DeserializeObject<SubMplanDTHCustomerInfo>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHCustInfo",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHPInfo;
        }
        public RoundpayDTHHeavyRefresh GetDTHRPHeavyRefresh(string AccountNo, int OID)
        {
            var respDTHHeavyRefresh = new RoundpayDTHHeavyRefresh();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHHeavyRefresh,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    //resp = "{\"tel\":\"209048456string\",\"operator\":\"Videocon\",\"records\":{\"status\":0,\"desc\":\"Customer Not Found\"},\"status\":1}";
                }
                if (resp != "")
                {
                    respDTHHeavyRefresh = JsonConvert.DeserializeObject<RoundpayDTHHeavyRefresh>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHHeavyRefresh",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHHeavyRefresh;
        }
        #endregion
        #region PlanAPI
        public PlanApi GetRofferPLANAPI(string AccountNo, int OID)
        {
            var subMplanRofferResp = new PlanApi();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanRofferResp = JsonConvert.DeserializeObject<PlanApi>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanRofferResp;
        }
        public PlanApiViewPlan GetSimplePlanAPI(int CircleID, int OID)
        {
            var subMplanSimplePlanResp = new PlanApiViewPlan();
            if (CircleID > 0 && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.SIMPLE,
                    CommonStr = "",
                    CommonInt = OID,
                    CommonInt2 = CircleID
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subMplanSimplePlanResp = JsonConvert.DeserializeObject<PlanApiViewPlan>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetSimplePlan",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subMplanSimplePlanResp;
        }
        public DTHPlan GetDTHSimplePlanAPI(int OID)
        {
            var subMplanDTHSimplePlanResp = new DTHPlan();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanSIMPLE,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<DTHPlan>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return subMplanDTHSimplePlanResp;
        }
        public DTHPlan GetDTHChannelPlanAPI(int OID)
        {
            var subMplanDTHSimplePlanResp = new DTHPlan();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanCHANNEL,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<DTHPlan>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHChannelPlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return subMplanDTHSimplePlanResp;
        }
        public DTHCustomerInfo GetDTHCustInfoPlanAPI(string AccountNo, int OID)
        {
            var respDTHPInfo = new DTHCustomerInfo();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHCustInfo,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    respDTHPInfo = JsonConvert.DeserializeObject<DTHCustomerInfo>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHCustInfo",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHPInfo;
        }
        #endregion
        #region Cyrus
        public CyrusPlanAPI GetRofferCYRUS(string AccountNo, int OID)
        {
            var subCyrusRofferResp = new CyrusPlanAPI();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        subCyrusRofferResp = JsonConvert.DeserializeObject<CyrusPlanAPI>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return subCyrusRofferResp;
        }
        #endregion
        #region VastWeb
        public VastWebRPlan GetRofferVastWeb(string AccountNo, int OID)
        {
            var vastWebRPlan = new VastWebRPlan();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = string.Empty;
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }
                    if (resp != "")
                    {
                        vastWebRPlan = JsonConvert.DeserializeObject<VastWebRPlan>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            return vastWebRPlan;
        }
        public VastWebDTHCustInfo GetDTHCustInfoVastWeb(string AccountNo, int OID)
        {
            var respDTHPInfo = new VastWebDTHCustInfo();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHCustInfo,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    respDTHPInfo = JsonConvert.DeserializeObject<VastWebDTHCustInfo>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHCustInfo",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHPInfo;
        }
        #endregion
        #region MyPlan
        public MyPlanApiSimplePlan GetSimpleMyPlanAPI(int CircleID, int OID)
        {
            var subMplanSimplePlanResp = new MyPlanApiSimplePlan();
            if (CircleID > 0 && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.SIMPLE,
                    CommonStr = "",
                    CommonInt = OID,
                    CommonInt2 = CircleID
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }

                    if (resp != "")
                    {
                        subMplanSimplePlanResp = JsonConvert.DeserializeObject<MyPlanApiSimplePlan>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetSimplePlan",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            if (subMplanSimplePlanResp.result != null)
            {
                subMplanSimplePlanResp.result = subMplanSimplePlanResp.result;
            }

            return subMplanSimplePlanResp;
        }
        public MyPlanApi GetRofferMyPlanApi(string AccountNo, int OID)
        {
            var subMplanRofferResp = new MyPlanApi();
            //var Mlnad = new records();
            if ((AccountNo ?? "") != "" && OID > 0)
            {
                var req = new CommonReq
                {
                    str = RPlanAPIType.ROFFER,
                    CommonStr = AccountNo,
                    CommonInt = OID,
                    CommonInt2 = 0
                };
                IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
                var aPIDetail = (APIDetail)_proc.Call(req);
                string resp = "";
                try
                {
                    if (aPIDetail.ID > 0)
                    {
                        resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    }

                    // subMplanRofferResp = JsonConvert.DeserializeObject<MyPlanApi>(resp);
                    if (resp != "")
                    {
                        subMplanRofferResp = JsonConvert.DeserializeObject<MyPlanApi>(resp);
                    }
                }
                catch (Exception ex)
                {
                    resp = resp + ex.Message;
                }
                var commonReq = new CommonReq
                {
                    CommonStr = "GetRoffer",
                    CommonStr2 = aPIDetail.URL,
                    CommonStr3 = resp
                };
                IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
                proc.Call(commonReq);
            }
            if (subMplanRofferResp.result != null && subMplanRofferResp.result.records != null)
            {
                subMplanRofferResp.records = subMplanRofferResp.result.records;
            }

            return subMplanRofferResp;
        }
        public MyPlanApiDthplan GetDthSimpleMyPlanApi(int OID)
        {
            var subMplanDTHSimplePlanResp = new MyPlanApiDthplan();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanSIMPLE,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<MyPlanApiDthplan>(resp);
                //if (resp != "")
                //{
                //    subMplanDTHSimplePlanResp = JsonConvert.DeserializeObject<MyPlanDTHPlan>(resp);
                //}
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            //subMplanDTHSimplePlanResp.result = subMplanDTHSimplePlanResp.result.records;
            return subMplanDTHSimplePlanResp;
        }
        public MyPlanDTHCustomerInfo GetDTHCustInfoMyPlan(string AccountNo, int OID)
        {
            var respDTHPInfo = new MyPlanDTHCustomerInfo();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHCustInfo,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }

                respDTHPInfo = JsonConvert.DeserializeObject<MyPlanDTHCustomerInfo>(resp);
                //if (resp != "")
                //{
                //    respDTHPInfo = JsonConvert.DeserializeObject<MyPlanDTHCustomerInfo>(resp);
                //}
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHCustInfo",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            if (respDTHPInfo.result != null && respDTHPInfo.result.records != null)
            {
                respDTHPInfo.records = respDTHPInfo.result.records;
            }
            return respDTHPInfo;
        }
        public SubMplanDTHSimplePlanResp AppGetDthSimpleMyPlan(int OID)
        {
            var subMplanDTHSimplePlanResp = new SubMplanDTHSimplePlanResp();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHPlanSIMPLE,
                CommonStr = "",
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                }
                if (resp != "")
                {
                    var data = JsonConvert.DeserializeObject<AppMyPlanApiDthplan>(resp);
                    subMplanDTHSimplePlanResp.Records = data.result.Records;
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            //subMplanDTHSimplePlanResp.result = subMplanDTHSimplePlanResp.result.records;
            return subMplanDTHSimplePlanResp;
        }
        #endregion
        public void LogAppReqResp(CommonReq req)
        {
            IProcedureAsync proc = new ProcAndroidAppReqResp(_dal);
            proc.Call(req);
        }
        public void LogWebAppReqResp(CommonReq req)
        {
            IProcedureAsync proc = new ProcWebAppReqResp(_dal);
            proc.Call(req);
        }

        #region RNPPlans
        public List<RNPRechPlansPanel> GetRNPRechargePlans(int oid, int circleId)
        {
            IOperatorML op = new OperatorML(_accessor, _env);
            var rJData = new List<RNPRechPlansPanel>();
            var getAS = new SettingML(_accessor, _env);
            bool IsLivePlan = getAS.GetASSettings().IsRechargePlansAPIOnly;
            if (IsLivePlan)
            {
                rJData = GetLiveRechargePlans(oid, circleId);
            }
            else
            {
                rJData = GetRNPRechargePlansPVT(oid, circleId);
                var cDate = DateTime.Now.ToString("dd MMM yyyy");
                if (rJData.Count == 0)
                {
                    var resp = op.UpdateRechPlans(oid);
                    if (resp.Statuscode == ErrorCodes.One)
                    {
                        rJData = GetRNPRechargePlansPVT(oid, circleId);
                    }
                }
                else if (cDate != rJData[0].EntryDate)
                {

                    var resp = op.UpdateRechPlans(oid);
                    if (resp.Statuscode == ErrorCodes.One)
                    {
                        rJData = GetRNPRechargePlansPVT(oid, circleId);
                    }
                }
            }
            return rJData;
        }
        private List<RNPRechPlansPanel> GetLiveRechargePlans(int oid, int circleId)
        {
            var rLivePlans = new List<RNPRechPlansPanel>();
            //Plans binding for View starts
            string Circle = string.Empty;
            string resp = string.Empty;
            int APIID;
            IProcedure proc = new ProcGetPlansDetialsFBI(_dal);
            var data = (OpRechargePlanResp)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                UserID = _lr.UserID,
                CommonInt = oid,
                CommonInt2 = 0
            });

            if (data.Statuscode == ErrorCodes.One)
            {
                APIID = data.APIID;
                var cir = data.CircleCodes.Where(x => x.CircleID == circleId).ToList<OpRechCircleDetals>();
                DataTable dtAllPlans = new DataTable();
                dtAllPlans.Columns.Add("_OID", typeof(int));
                dtAllPlans.Columns.Add("_CircleID", typeof(int));
                dtAllPlans.Columns.Add("_RechargePlanType", typeof(string));
                dtAllPlans.Columns.Add("_RAmount", typeof(string));
                dtAllPlans.Columns.Add("_Validity", typeof(string));
                dtAllPlans.Columns.Add("_Details", typeof(string));
                dtAllPlans.Columns.Add("_EntryDate", typeof(DateTime));
                dtAllPlans.Columns.Add("_ModifyDate", typeof(DateTime));
                dtAllPlans.Columns.Add("_APIID", typeof(int));

                StringBuilder sb = new StringBuilder(data.URL);
                sb.Replace("{CIRCLE}", cir[0].CircleCode);
                if (data.APICode.Equals(PlanType.Roundpay))
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                    IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                    _err.Call(new CommonReq
                    {
                        CommonStr = "LiveRechargePlan | " + data.APICode + " | " + circleId,
                        CommonStr2 = sb.ToString(),
                        CommonStr3 = resp
                    });
                    var plans = JsonConvert.DeserializeObject<RoundpaySimplePlanResp>(resp);
                    if (plans != null)
                    {
                        try
                        {
                            if (plans.Records != null)
                            {
                                if (plans.Records.TwoG != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.TwoG));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.TwoG)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.ThreeG != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.ThreeG));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.ThreeG)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.FourG != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.FourG));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.FourG)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.AllRounder != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.AllRounder));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.AllRounder)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.DATA != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.DATA));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.DATA)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.frc != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.frc));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.frc)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.FRCNonPrime != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.FRCNonPrime));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.FRCNonPrime)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                    }
                                }
                                if (plans.Records.HotStar != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.HotStar));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.HotStar)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now);
                                    }
                                }
                                if (plans.Records.international != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.international));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.international)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.isd != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.isd));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.isd)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.JioPhone != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.JioPhone));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.JioPhone)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.Local != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.Local));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.Local)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.NewAllinOne != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.NewAllinOne));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.NewAllinOne)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.Other != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.Other));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.Other)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.roaming != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.roaming));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.roaming)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.smartrecharge != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.smartrecharge));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.smartrecharge)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.sms != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.sms));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.sms)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.stv != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.stv));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.stv)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.talktime != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.talktime));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.talktime)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.unlimited != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.unlimited));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.unlimited)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.Validity != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.Validity));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.Validity)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.VAS != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.VAS));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.VAS)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                if (plans.Records.workfromhome != null)
                                {
                                    var cat = GetCategory(nameof(plans.Records.workfromhome));
                                    var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                    foreach (var comboItem in plans.Records.workfromhome)
                                    {
                                        dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "UpdateRechPlans [" + data.APICode + "]",
                                Error = ex.Message,
                                LoginTypeID = 1,
                                UserId = 1
                            });
                        }
                    }
                }
                else if (data.APICode.Equals(PlanType.PLANSINFO))
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                    IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                    _err.Call(new CommonReq
                    {
                        CommonStr = "LiveRechargePlan | " + data.APICode + " | " + circleId,
                        CommonStr2 = sb.ToString(),
                        CommonStr3 = resp
                    });
                    var plans = JsonConvert.DeserializeObject<PlansInfoRechPlanResp>(resp);

                    try
                    {
                        foreach (var pifo in plans.data)
                        {
                            var cat = GetCategory(pifo.category);
                            var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                            dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, pifo.amount, pifo.validity, pifo.benefit, DateTime.Now, DateTime.Now, APIID);
                        }
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UpdateRechPlans [" + data.APICode + "]",
                            Error = ex.Message,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
                else if (data.APICode.Equals(PlanType.MPLAN))
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                    IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                    _err.Call(new CommonReq
                    {
                        CommonStr = "LiveRechargePlan | " + data.APICode + " | " + circleId,
                        CommonStr2 = sb.ToString(),
                        CommonStr3 = resp
                    });
                    var plans = JsonConvert.DeserializeObject<SubMplanSimplePlanResp>(resp);
                    try
                    {
                        if (plans.Records != null)
                        {
                            if (plans.Records.COMBO != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.COMBO));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.COMBO)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.FullTT != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.FullTT));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.FullTT)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.TOPUP != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.TOPUP));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.TOPUP)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.ThreeGFourG != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.ThreeGFourG));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.ThreeGFourG)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.TwoG != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.TwoG));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.TwoG)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.RateCutter != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.RateCutter));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.RateCutter)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.Roaming != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.Roaming));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.Roaming)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.Records.SMS != null)
                            {
                                var cat = GetCategory(nameof(plans.Records.SMS));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.Records.SMS)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "Live View Plans [" + data.APICode + "]",
                            Error = ex.Message,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
                else if (data.APICode.Equals(PlanType.PLANAPI))
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                    IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                    _err.Call(new CommonReq
                    {
                        CommonStr = "LiveRechargePlan | " + data.APICode + " | " + circleId,
                        CommonStr2 = sb.ToString(),
                        CommonStr3 = resp
                    });
                    var plans = JsonConvert.DeserializeObject<PlanApiViewPlan>(resp);
                    try
                    {
                        if (plans.RDATA != null)
                        {
                            if (plans.RDATA.COMBO != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.COMBO));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.COMBO)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.DATA != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.DATA));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.DATA)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.FRC != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.FRC));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.FRC)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.FULLTT != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.FULLTT));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.FULLTT)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.TOPUP != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.TOPUP));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.TOPUP)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.ThreeG4G != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.ThreeG4G));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.ThreeG4G)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.RATECUTTER != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.RATECUTTER));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.RATECUTTER)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.TwoG != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.TwoG));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.TwoG)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                }
                            }
                            if (plans.RDATA.SMS != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.SMS));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.SMS)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.Romaing != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.Romaing));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.Romaing)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                            if (plans.RDATA.STV != null)
                            {
                                var cat = GetCategory(nameof(plans.RDATA.STV));
                                var rid = data.RechPlanType.Where(x => x.RechargePlanType.ToUpper() == cat.ToUpper()).ToList<MasterRechPlanType>();
                                foreach (var comboItem in plans.RDATA.STV)
                                {
                                    dtAllPlans.Rows.Add(oid, cir[0].CircleID, rid[0].RechargePlanType, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "Recharge View [" + data.APICode + "]",
                            Error = ex.Message,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
                if (dtAllPlans.Rows.Count > 0)
                {
                    try
                    {
                        foreach (DataRow dr in dtAllPlans.Rows)
                        {
                            rLivePlans.Add(new RNPRechPlansPanel
                            {
                                OID = Convert.ToInt32(dr["_OID"]),
                                CircleID = Convert.ToInt32(dr["_CircleID"]),
                                RechargePlanType = dr["_RechargePlanType"].ToString(),
                                RAmount = dr["_RAmount"].ToString(),
                                Details = dr["_Details"].ToString(),
                                Validity = dr["_Validity"].ToString()
                            });
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            //Plans binding for View Ends
            return rLivePlans;
        }
        private string GetCategory(string category)
        {
            if (category.In("Data Pack", "data", "ThreeGFourG", "TwoG", "ThreeG4G", "FourG"))
            {
                category = "Data";
                return category;
            }
            else if (category == "international roaming")
            {
                category = "International";
                return category;
            }
            else if (category == "validity extension")
            {
                category = "Validity";
                return category;
            }
            else if (category == "workfromhome")
            {
                category = "Work From Home";
                return category;
            }
            else if (category == "FRCNonPrime")
            {
                category = "FRC/non-Prime";
                return category;
            }
            else if (category == "AllRounder")
            {
                category = "All Rounder";
                return category;
            }
            else if (category == "TOPUP")
            {
                category = "Top up";
                return category;
            }
            else if (category == "NewAllinOne")
            {
                category = "NEW ALL-IN-ONE";
                return category;
            }
            else if (category.In("Smart", "smartrecharge"))
            {
                category = "Smart Recharge";
                return category;
            }
            else
            {
                return category;
            }
        }
        private List<RNPRechPlansPanel> GetRNPRechargePlansPVT(int o, int c)
        {
            IProcedure proc = new ProcGetRNPRechargePlans(_dal);
            var res = (List<RNPRechPlansPanel>)proc.Call(new CommonReq
            {
                LoginTypeID = 1,
                LoginID = 1,
                CommonInt = o,
                CommonInt2 = c
            });
            return res;
        }
        public RNPDTHPlans GetRNPDTHPlans(int oid)
        {
            var dthPlansresp = new RNPDTHPlans();
            IOperatorML op = new OperatorML(_accessor, _env);
            dthPlansresp = GetRNPDTHPlansPVT(oid);
            var cDate = DateTime.Now.ToString("dd MMM yyyy");
            if (dthPlansresp.Response == null)
            {
                var res = op.UpdateDTHPlans(oid);
                if (res.Statuscode == ErrorCodes.One)
                {
                    dthPlansresp = GetRNPDTHPlansPVT(oid);
                }
            }
            else
            {
                if (cDate != dthPlansresp.Response[0].EntryDate.ToString("dd MMM yyyy"))
                {
                    var res = op.UpdateDTHPlans(oid);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        dthPlansresp = GetRNPDTHPlansPVT(oid);
                    }
                }
            }
            return dthPlansresp;
        }
        private RNPDTHPlans GetRNPDTHPlansPVT(int o)
        {
            IProcedure proc = new ProcGetRNPDTHPlans(_dal);
            return (RNPDTHPlans)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                UserID = _lr.UserID,
                CommonInt = o,
            });
        }
        private object GetDTHPlanFDB(int o)
        {
            IProcedure proc = new ProcDTHPlansWithPCL(_dal);
            var dthPlans = (List<DTHPlanRespDB>)proc.Call(new CommonReq
            {
                LoginID = 1,
                LoginTypeID = 1,
                CommonInt = o
            });
            var Fplans = new List<DTHPlanFDB>();
            var planType = dthPlans.Select(x => x._PackageType).Distinct();
            foreach (var pType in planType)
            {
                var p1 = pType.ToString();
                var pD = new List<DTHPlanRespDB>();
                try
                {
                    pD = dthPlans.Where(x => x._PackageType == p1).OrderBy(y => Convert.ToDouble(y._PackagePrice)).ToList();
                }
                catch (Exception)
                {
                    pD = dthPlans.Where(x => x._PackageType == p1).ToList();
                }
                //var pD = dthPlans.Where(x => x._PackageType == p1).OrderBy(y => Convert.ToDouble(y._PackagePrice));
                var pDetails = new List<PDetial>();
                foreach (var pDetail in pD)
                {
                    var pr = new Price();
                    pr.monthly = pDetail._PackagePrice;
                    pr.quarterly = pDetail._PackagePrice_3;
                    pr.halfYearly = pDetail._PackagePrice_6;
                    pr.yearly = pDetail._PackagePrice_12;
                    pDetails.Add(new PDetial
                    {
                        packageName = pDetail._PackageName,
                        price = pr,
                        pDescription = pDetail._PackageDescription,
                        packageId = Convert.ToInt32(pDetail._PackageId),
                        pLangauge = pDetail._PackageLanguage,
                        pCount = 0,
                        pChannelCount = Convert.ToInt32(pDetail._pChannelCount)
                    });
                }
                Fplans.Add(new DTHPlanFDB
                {
                    pType = p1,
                    pDetials = pDetails
                });
            }
            var pLang = dthPlans.Select(x => x._PackageLanguage).Distinct();
            var pLangDetails = new List<PDetial>();
            foreach (var pType in pLang)
            {
                var pD = dthPlans.Where(x => x._PackageLanguage == pType).Count();
                pLangDetails.Add(new PDetial
                {
                    packageName = null,
                    price = null,
                    pDescription =null,
                    packageId = 0,
                    pLangauge = pType.ToString(),
                    pCount = Convert.ToInt32(pD),
                    pChannelCount = 0
                });
            }
            Fplans.Add(new DTHPlanFDB
            {
                pType = "Languages",
                pDetials = pLangDetails.OrderBy(x => x.pCount).ToList()
            });
            return Fplans;
        }
        public RNPRoffer GetRNPRoffer(string m, int o)
        {
            var resp = new RNPRoffer
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcGetRNPRofferURL(_dal);
            var data = (HLRResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = 1,
                UserID = 1,
                CommonInt = o,
                CommonStr = m
            });
            resp.StatusCode = data.Statuscode;
            resp.Msg = data.Msg;
            if (resp.StatusCode == ErrorCodes.One)
            {
                try
                {
                    foreach (var item in data.HLRAPIs)
                    {
                        resp = HitRofferAPIs(item.APICode, item.APIURL, m);
                        if (resp.StatusCode == ErrorCodes.One)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetRNPRoffer",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }
            }
            return resp;
        }
        private RNPRoffer HitRofferAPIs(string APICode, string URL, string Mob)
        {
            var resp = new RNPRoffer
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(URL))
            {
                resp.StatusCode = ErrorCodes.Minus1;
                resp.Msg = "No URL Found!";
                return resp;
            }
            URL = URL.Replace("{MOBILE}", Mob);
            var webResult = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
            IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
            _err.Call(new CommonReq
            {
                CommonStr = "GetRNPRoffer",
                CommonStr2 = URL,
                CommonStr3 = webResult
            });
            if (APICode.Equals(PlanType.MPLAN))
            {
                if (webResult != "")
                {
                    if (webResult.Contains("You are not authorize"))
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.AnError;
                        return resp;
                    }
                    else
                    {
                        var rofferResult = JsonConvert.DeserializeObject<SubMplanRofferResp>(webResult);
                        var rofr = new List<RNPRofferData>();
                        foreach (var item in rofferResult.Records)
                        {
                            rofr.Add(new RNPRofferData
                            {
                                Amount = item.RS,
                                Description = item.Desc
                            });
                        }
                        resp.RofferData = rofr;
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            else if (APICode.Equals(PlanType.Roundpay))
            {
                if (webResult != "")
                {
                    if (webResult.Contains("No Plan found"))
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.AnError;
                        return resp;
                    }
                    else
                    {
                        var rofferResult = JsonConvert.DeserializeObject<SubMplanRofferResp>(webResult);
                        var rofr = new List<RNPRofferData>();
                        foreach (var item in rofferResult.Records)
                        {
                            rofr.Add(new RNPRofferData
                            {
                                Amount = item.RS,
                                Description = item.Desc
                            });
                        }
                        resp.RofferData = rofr;
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            else if (APICode.Equals(PlanType.PLANAPI))
            {
                if (webResult != "")
                {
                    var rofferResult = JsonConvert.DeserializeObject<PlanApi>(webResult);
                    if (rofferResult.RDATA == null)
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.AnError;
                        return resp;
                    }
                    else
                    {
                        var rofr = new List<RNPRofferData>();
                        foreach (var item in rofferResult.RDATA)
                        {
                            rofr.Add(new RNPRofferData
                            {
                                Amount = item.price,
                                Description = item.ofrtext
                            });
                        }
                        resp.RofferData = rofr;
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }

                }
            }
            else if (APICode.Equals(PlanType.CYRUS))
            {
                if (webResult != "")
                {
                    var rofferResult = JsonConvert.DeserializeObject<CyrusPlanAPI>(webResult);
                    var rofr = new List<RNPRofferData>();
                    foreach (var item in rofferResult.records)
                    {
                        rofr.Add(new RNPRofferData
                        {
                            Amount = item.rs,
                            Description = item.desc
                        });
                    }
                    resp.RofferData = rofr;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else if (APICode.Equals(PlanType.VASTWEB))
            {
                if (webResult != "")
                {
                    var rofferResult = JsonConvert.DeserializeObject<VastWebRPlan>(webResult);
                    var rofr = new List<RNPRofferData>();
                    foreach (var item in rofferResult.Response)
                    {
                        rofr.Add(new RNPRofferData
                        {
                            Amount = item.price,
                            Description = item.offerDetails
                        });
                    }
                    resp.RofferData = rofr;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else if (APICode.Equals(PlanType.MYPLAN))
            {
                if (webResult != "")
                {
                    var rofferResult = JsonConvert.DeserializeObject<MyPlanApi>(webResult);
                    var rofr = new List<RNPRofferData>();
                    foreach (var item in rofferResult.records)
                    {
                        rofr.Add(new RNPRofferData
                        {
                            Amount = item.rs,
                            Description = item.desc
                        });
                    }
                    resp.RofferData = rofr;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else if (APICode.Equals(PlanType.PLANSINFO))
            {
                if (webResult != "")
                {
                    var rofferResult = JsonConvert.DeserializeObject<MyPlanApi>(webResult);
                    var rofr = new List<RNPRofferData>();
                    foreach (var item in rofferResult.records)
                    {
                        rofr.Add(new RNPRofferData
                        {
                            Amount = item.rs,
                            Description = item.desc
                        });
                    }
                    resp.RofferData = rofr;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
            }
            try
            {
                if (resp.StatusCode == ErrorCodes.One)
                    resp.RofferData = resp.RofferData.OrderBy(x => Convert.ToInt32(x.Amount)).ToList();
            }
            catch (Exception ex)
            {
                return resp;
            }
            //if (resp.StatusCode == ErrorCodes.One)
            //    resp.RofferData = resp.RofferData.OrderBy(x => Convert.ToInt32(x.Amount)).ToList();
            return resp;
        }
        public RNPDTHCustInfo GetRNPDTHCustInfo(int o, string a)
        {
            var resp = new RNPDTHCustInfo
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcGetRNPDTHCustInfoURL(_dal);
            var data = (HLRResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = 1,
                UserID = 1,
                CommonInt = o,
                CommonStr = a
            });
            resp.StatusCode = data.Statuscode;
            resp.Msg = data.Msg;
            if (resp.StatusCode == ErrorCodes.One)
            {
                try
                {
                    foreach (var item in data.HLRAPIs)
                    {
                        resp = HitDthCustInfoAPI(item.APICode, item.APIURL, a);
                        if (resp.StatusCode == ErrorCodes.One)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetRNPDTHCustInfo",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }

            }
            return resp;
        }
        private RNPDTHCustInfo HitDthCustInfoAPI(string APICode, string URL, string Acc)
        {
            var resp = new RNPDTHCustInfo
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(URL))
            {
                resp.StatusCode = ErrorCodes.Minus1;
                resp.Msg = "No URL Found!";
                return resp;
            }
            URL = URL.Replace("{MOBILE}", Acc);
            var webResult = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
            IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
            _err.Call(new CommonReq
            {
                CommonStr = "GetRNPDTHCustInfo",
                CommonStr2 = URL,
                CommonStr3 = webResult
            });

            if (APICode.Equals(PlanType.MPLAN))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    if (webResult.Contains("You are not authorize"))
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.AnError;
                        return resp;
                    }
                    else
                    {
                        var dthCustInfoRes = JsonConvert.DeserializeObject<SubMplanDTHCustomerInfo>(webResult);
                        var resJ = DynamicHelper.O.GetKeyValuePairs(webResult, "records", true);
                        resp.Data = JsonConvert.DeserializeObject<List<RNPDTHCustList>>(JsonConvert.SerializeObject(resJ));
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            else if (APICode.Equals(PlanType.Roundpay))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    if (webResult.Contains("Customer Not Found"))
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = "Customer Not Found";
                        return resp;
                    }
                    else
                    {
                        var resJ = DynamicHelper.O.GetKeyValuePairs(webResult, "records", true);
                        resp.Data = JsonConvert.DeserializeObject<List<RNPDTHCustList>>(JsonConvert.SerializeObject(resJ));
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = ErrorCodes.AnError;
                    return resp;
                }
            }
            else if (APICode.Equals(PlanType.PLANAPI))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    var dthCustInfoRes = JsonConvert.DeserializeObject<DTHCustomerInfo>(webResult);
                    if (dthCustInfoRes.DATA == null)
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.AnError;
                    }
                    else
                    {
                        var resJ = DynamicHelper.O.GetKeyValuePairs(webResult, "DATA", true);
                        resp.Data = JsonConvert.DeserializeObject<List<RNPDTHCustList>>(JsonConvert.SerializeObject(resJ));
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            else if (APICode.Equals(PlanType.VASTWEB))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    var dthCustInfoRes = JsonConvert.DeserializeObject<VastWebDTHCustInfo>(webResult);
                    var resJ = DynamicHelper.O.GetKeyValuePairs(webResult, "records", true);
                    resp.Data = JsonConvert.DeserializeObject<List<RNPDTHCustList>>(JsonConvert.SerializeObject(resJ));
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = ErrorCodes.AnError;
                    return resp;
                }
            }
            else if (APICode.Equals(PlanType.MYPLAN))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    var dthCustInfoRes = JsonConvert.DeserializeObject<MyPlanDTHCustomerInfo>(webResult);
                    var resJ = DynamicHelper.O.GetKeyValuePairs(webResult, "records", true);
                    resp.Data = JsonConvert.DeserializeObject<List<RNPDTHCustList>>(JsonConvert.SerializeObject(resJ));
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = ErrorCodes.AnError;
                    return resp;
                }
            }
            return resp;
        }
        public RNPDTHHeavyRefresh GetRNPDTHHeavyRefresh(int o, string a)
        {
            var resp = new RNPDTHHeavyRefresh
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcGetRNPDTHHeavyRefresh(_dal);
            var data = (HLRResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = 1,
                UserID = 1,
                CommonInt = o,
                CommonStr = a
            });
            resp.StatusCode = data.Statuscode;
            resp.Msg = data.Msg;
            if (resp.StatusCode == ErrorCodes.One)
            {
                try
                {
                    foreach (var item in data.HLRAPIs)
                    {
                        resp = HitDTHHeavyRefreshAPI(item.APICode, item.APIURL, a);
                        if (resp.StatusCode == ErrorCodes.One)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetRNPDTHHeavyRefresh",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }
            }
            return resp;
        }
        private RNPDTHHeavyRefresh HitDTHHeavyRefreshAPI(string APICode, string URL, string Acc)
        {
            var resp = new RNPDTHHeavyRefresh
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(URL))
            {
                resp.StatusCode = ErrorCodes.Minus1;
                resp.Msg = "No URL Found!";
                return resp;
            }
            URL = URL.Replace("{MOBILE}", Acc);
            var webResult = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
            IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
            _err.Call(new CommonReq
            {
                CommonStr = "GetRNPDTHHeavyRefresh",
                CommonStr2 = URL,
                CommonStr3 = webResult
            });
            if (APICode.Equals(PlanType.MPLAN))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    if (webResult.Contains("You are not authorize"))
                    {
                        resp.StatusCode = ErrorCodes.Minus1;
                        resp.Msg = "No Record Found!";
                        return resp;
                    }
                    else
                    {
                        var dthHeavyRefresh = JsonConvert.DeserializeObject<MplanDTHHeavyRefresh>(webResult);
                        resp.AccountNo = dthHeavyRefresh.Number;
                        resp.Operator = dthHeavyRefresh.Operator;
                        resp.Response = dthHeavyRefresh.Response.desc;
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.SUCCESS;
                    }
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = "No Record Found!";
                    return resp;
                }
            }
            else if (APICode.Equals(PlanType.Roundpay))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    var dthHeavyRefresh = JsonConvert.DeserializeObject<RoundpayDTHHeavyRefresh>(webResult);
                    resp.AccountNo = dthHeavyRefresh.Tel;
                    resp.Operator = dthHeavyRefresh.Operator;
                    resp.Response = dthHeavyRefresh.Records.Desc;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = "No Record Found!";
                    return resp;
                }
            }
            else if (APICode.Equals(PlanType.MYPLAN))
            {
                if (!string.IsNullOrEmpty(webResult))
                {
                    var dthHeavyRefresh = JsonConvert.DeserializeObject<Root>(webResult);
                    resp.AccountNo = dthHeavyRefresh.result.tel;
                    resp.Operator = dthHeavyRefresh.result.Operator;
                    resp.Response = dthHeavyRefresh.result.records.desc;
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                }
                else
                {
                    resp.StatusCode = ErrorCodes.Minus1;
                    resp.Msg = "No Record Found!";
                    return resp;
                }
            }
            return resp;
        }
        #region AppPlans
        public object AppSimplePlan(int oid, int circleId)
        {
            var planData = GetRNPRechargePlansPVT(oid, circleId);
            var planT = planData.GroupBy(x => x.RechargePlanType).Select(y => y.First());
            dynamic root = new JObject();
            var jArr = new JArray();
            foreach (var rType in planT)
            {
                var jObj = new JObject();
                var jArrPlan = new JArray();
                jObj.Add(new JProperty("pType", rType.RechargePlanType));
                var list = planData.Where(x => x.RechargePlanType == rType.RechargePlanType);
                foreach (var item in list)
                {
                    var jObjP = new JObject();
                    jObjP.Add(new JProperty("rs", item.RAmount));
                    jObjP.Add(new JProperty("desc", item.Details));
                    jObjP.Add(new JProperty("validity", item.Validity == "" ? "N.A" : item.Validity));
                    jArrPlan.Add(jObjP);
                }
                jObj.Add(new JProperty("pDetails", jArrPlan));
                jArr.Add(jObj);
            }
            root.Add(new JProperty("types", jArr));
            return root;
        }
        public object AppDTHPlan(int oid)
        {
            return GetDTHPlanFDB(oid);
        }
        public object AppDTHLang(int oid,string Lang)
        {
            IProcedure proc = new ProcDTHPlansWithPCL(_dal);
            var dthPlans = (List<DTHPlanRespDB>)proc.Call(new CommonReq
            {
                LoginID = 1,
                LoginTypeID = 1,
                CommonInt = oid
            });
            var Fplans = new List<DTHPlanFDB>();
            var planType = dthPlans.Select(x => x._PackageType).Distinct(); ;
            foreach (var pType in planType)
            {
                var p1 = pType.ToString();
                var pD = dthPlans.Where(x => x._PackageType == p1 && x._PackageLanguage == Lang);
                var pDetails = new List<PDetial>();
                foreach (var pDetail in pD)
                {
                    var pr = new Price();
                    pr.monthly = pDetail._PackagePrice;
                    pr.quarterly = pDetail._PackagePrice_3;
                    pr.halfYearly = pDetail._PackagePrice_6;
                    pr.yearly = pDetail._PackagePrice_12;
                    pDetails.Add(new PDetial
                    {
                        packageName = pDetail._PackageName,
                        price = pr,
                        pDescription = pDetail._PackageDescription,
                        packageId = Convert.ToInt32(pDetail._PackageId),
                        pLangauge = pDetail._PackageLanguage,
                        pCount = 0,
                        pChannelCount = Convert.ToInt32(pDetail._pChannelCount)
                    });
                }
                Fplans.Add(new DTHPlanFDB
                {
                    pType = p1,
                    pDetials = pDetails
                });
            }
            return Fplans;
        }
        public object AppDTHChannels(int pid)
        {
            IProcedure proc = new ProcDTHChannelByPID(_dal);
            return (List<DTHChnlRespDB>)proc.Call(new CommonReq { 
                CommonInt = pid
            });
        }
        #endregion
        private int GetCategoryId(string category)
        {
            int catId = 0;
            IProcedure proc = new ProcGetPlanTypeID(_dal);
            var resp = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = category
            });
            if (resp.Statuscode == ErrorCodes.One)
                catId = resp.CommonInt;
            return catId;
        }

        #endregion
        public Root GetDthHaveyRefershMyPlan(string AccountNo, int OID)
        {
            var respDTHHeavyRefresh = new Root();
            var req = new CommonReq
            {
                str = RPlanAPIType.DTHHeavyRefresh,
                CommonStr = AccountNo,
                CommonInt = OID,
                CommonInt2 = 0
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            var aPIDetail = (APIDetail)_proc.Call(req);
            string resp = "";
            try
            {
                if (aPIDetail.ID > 0)
                {
                    resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(aPIDetail.URL);
                    //resp = "{\"tel\":\"209048456\",\"operator\":\"Videocon\",\"records\":{\"desc\":\"Reauthorized                      Successfully.\",\"status\":1},\"status\":1}";
                }
                respDTHHeavyRefresh = JsonConvert.DeserializeObject<Root>(resp);
                //if (resp != "")
                //{
                //    respDTHHeavyRefresh = JsonConvert.DeserializeObject<Root>(resp);
                //}
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
            }
            var commonReq = new CommonReq
            {
                CommonStr = "GetDTHHeavyRefresh",
                CommonStr2 = aPIDetail.URL,
                CommonStr3 = resp
            };
            IProcedureAsync proc = new ProcLogPlansAPIReqResp(_dal);
            proc.Call(commonReq);
            return respDTHHeavyRefresh;
        }
    }
}
