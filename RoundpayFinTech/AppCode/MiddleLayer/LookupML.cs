using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Lookup;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class LookupML : ILookUpPlanAPI, ILookUpGoRecharge, ILookUpRoundpay, ILookUpAPIBox, ILookUpMPLAN, ILookUpMyPlan, ILookUpVASTWEB, ILookUpInfoAPI, ILookUpAirtelPP
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;

        public LookupML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        #region PlanAPI
        public HLRResponseStatus GetLookUp(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new LookUpRes();
            string resp = "";
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (resp != "")
                {
                    lookUpRes = JsonConvert.DeserializeObject<LookUpRes>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUp",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.PLANAPI,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(lookUpRes.Mobile) ? Mobile : (lookUpRes.Mobile.Length > 10 ? lookUpRes.Mobile.Substring(lookUpRes.Mobile.Length - 10, 10) : lookUpRes.Mobile),
                Request = URL,
                CurrentCircle = lookUpRes.Circle,
                CurrentOperator = lookUpRes.Operator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion

        #region GoRecharge
        public HLRResponseStatus GetLookUpGoRecharge(string URL, int APIID, int UserId, string MobileNo)
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = new LookUpGoRechargeReq();
            req.CorporateNo = "2019021417400235750022647";
            req.MobileNo = MobileNo;
            req.SystemReferenceNo = DateTime.Now.Ticks.ToString();
            req.APIChecksum = HashEncryption.O.MD5Hash(req.CorporateNo + req.MobileNo + req.SystemReferenceNo + "Easy@01");

            var lookUpRes = new LookUpGoRechargeRes();
            string resp = "";
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, req);

                if (resp != "")
                {
                    lookUpRes = JsonConvert.DeserializeObject<LookUpGoRechargeRes>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUpGoRecharge",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.GoRecharge,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(lookUpRes.MobileNo) ? MobileNo : (lookUpRes.MobileNo.Length > 10 ? lookUpRes.MobileNo.Substring(lookUpRes.MobileNo.Length - 10, 10) : lookUpRes.MobileNo),
                Request = URL,
                CurrentCircle = lookUpRes.CurrentLocation,
                CurrentOperator = lookUpRes.CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion

        #region Roundpay
        public HLRResponseStatus GetLookUpRoundpay(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new LookUpRoundpayRes();
            string resp = "";
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);

                if (resp != "")
                {
                    lookUpRes = JsonConvert.DeserializeObject<LookUpRoundpayRes>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUpRoundpay",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            string MobileNo = "";
            string CurrentCircle = "";
            string CurrentOperator = "";
            if (lookUpRes != null && lookUpRes.response != null)
            {
                if (lookUpRes.response[0].ported.ToString().ToUpper() == "TRUE")
                {
                    var Mob = lookUpRes.response[0].lookup_number;
                    MobileNo = Mob.Length > 10 ? Mob.Substring(Mob.Length - 10, 10) : Mob;
                    CurrentCircle = lookUpRes.response[0].new_circle;
                    CurrentOperator = lookUpRes.response[0].new_operator;
                }
                else
                {
                    var Mob = lookUpRes.response[0].lookup_number;
                    MobileNo = Mob.Length > 10 ? Mob.Substring(Mob.Length - 10, 10) : Mob;
                    CurrentCircle = lookUpRes.response[0].old_circle;
                    CurrentOperator = lookUpRes.response[0].old_operator;
                }
            }

            LookUpDBLogReq _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.Roundpay,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion

        #region APIBox
        public HLRResponseStatus GetLookUpAPIBox(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            StringBuilder sb = new StringBuilder(URL);
            sb.Replace(Replacement.TID, "A" + DateTime.Now.Ticks.ToString());
            URL = sb.ToString();
            var lookUpRes = new LookUpAPIBoxRes();
            string resp = "";
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (resp != "")
                {
                    lookUpRes = JsonConvert.DeserializeObject<LookUpAPIBoxRes>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUpAPIBox",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            string MobileNo = "";
            string CurrentCircle = "";
            string CurrentOperator = "";
            bool _IsCircleOnly = false;
            if (lookUpRes != null && lookUpRes.response != null)
            {
                if (!lookUpRes.response.status_code.Equals("SPE"))
                {
                    if (lookUpRes.response.ported ?? false)
                    {
                        if (lookUpRes.response.new_operator_circle.ToLower().Contains("other networks"))
                        {
                            res.Msg = "Network not specified";
                        }
                        else
                        {
                            var Mob = lookUpRes.response.lookup_number;
                            MobileNo = Mob.Length > 10 ? Mob.Substring(Mob.Length - 10, 10) : Mob;
                            CurrentCircle = lookUpRes.response.new_operator_circle.Split(',')[1];
                            if (lookUpRes.response.status_code.In("SDL", "SAC"))
                            {
                                CurrentOperator = lookUpRes.response.new_operator_circle.Split(',')[0];
                                _IsCircleOnly = lookUpRes.response.status_code.Equals("SAC") ? true : false;
                            }
                        }
                    }
                    else
                    {
                        if (lookUpRes.response.operator_circle.ToLower().Contains("other networks"))
                        {
                            res.Msg = "Network not specified";
                        }
                        else
                        {
                            var Mob = lookUpRes.response.lookup_number;
                            MobileNo = Mob.Length > 10 ? Mob.Substring(Mob.Length - 10, 10) : Mob;
                            CurrentCircle = lookUpRes.response.operator_circle.Split(',')[1];
                            if (lookUpRes.response.status_code.In("SDL", "SAC"))
                            {
                                CurrentOperator = lookUpRes.response.operator_circle.Split(',')[0];
                                _IsCircleOnly = lookUpRes.response.status_code.Equals("SAC") ? true : false;
                            }
                        }
                    }
                }
            }
            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.APIBox,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp,
                IsCircleOnly = _IsCircleOnly
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion

        #region MPLAN
        public HLRResponseStatus GetLookUpMplan(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new LookupMPlanAPIResponse();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (!string.IsNullOrEmpty(resp))
                {
                    lookUpRes = JsonConvert.DeserializeObject<LookupMPlanAPIResponse>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUpMplan",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            string MobileNo = "";
            string CurrentCircle = "";
            string CurrentOperator = "";
            if (lookUpRes != null && lookUpRes.records != null)
            {
                if (lookUpRes.records.status == 1)
                {
                    MobileNo = lookUpRes.tel;
                    //CurrentCircle = lookUpRes.records.circle;
                    CurrentCircle = lookUpRes.records.comcircle;
                    CurrentOperator = lookUpRes.records.Operator;
                }
            }

            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.MPLAN,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion
        public HLRResponseStatus GetLookUpApiMyPlans(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new HlrLookUPMyPlan();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);

                if (!string.IsNullOrEmpty(resp))
                {
                    lookUpRes = JsonConvert.DeserializeObject<HlrLookUPMyPlan>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetLookUpMyplan",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            string MobileNo = "";
            string CurrentCircle = "";
            string CurrentOperator = "";
            if (lookUpRes != null && lookUpRes.result.records != null)
            {
                if (lookUpRes.result.records.status == 1)
                {
                    MobileNo = lookUpRes.result.tel;
                    //CurrentCircle = lookUpRes.records.circle;
                    CurrentCircle = lookUpRes.result.records.comcircle;
                    CurrentOperator = lookUpRes.result.records.@operator;
                }
            }

            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.MYPLAN,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #region VastWeb
        public HLRResponseStatus GetHLRVastWeb(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new VastWebHLRResp();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (!string.IsNullOrEmpty(resp))
                {
                    lookUpRes = JsonConvert.DeserializeObject<VastWebHLRResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetHLRVastWeb",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            string MobileNo = Mobile;
            string CurrentCircle = "";
            string CurrentOperator = "";
            if (lookUpRes != null && lookUpRes.Response != null)
            {
                if (lookUpRes.status.ToString().ToUpper().Equals("SUCCESS"))
                {
                    CurrentCircle = lookUpRes.Response.Circle;
                    CurrentOperator = lookUpRes.Response.Operator;
                }
            }

            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.VASTWEB,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion



        #region INFOAPIHLR
        public HLRResponseStatus GetHLRINFOAPI(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new InfoAPIHLRResp();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (!string.IsNullOrEmpty(resp))
                {
                    lookUpRes = JsonConvert.DeserializeObject<InfoAPIHLRResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetHLRINFOAPI",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            string MobileNo = Mobile;
            string CurrentCircle = "";
            string CurrentOperator = "";
            if (lookUpRes != null && lookUpRes.data != null)
            {
                if (lookUpRes.status.ToString().ToUpper().Equals("SUCCESS"))
                {
                    CurrentCircle = lookUpRes.data.state.ToString();
                    CurrentOperator = lookUpRes.data.@operator.ToString();
                }
            }

            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.VASTWEB,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = CurrentCircle,
                CurrentOperator = CurrentOperator,
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            return res;
        }
        #endregion


        #region AirtelPPHLR
        public HLRResponseStatus GetHLRAirtelPostpaid(string URL, int APIID, int UserId, string Mobile = "")
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var lookUpRes = new AirtelPPHLRResp();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (!string.IsNullOrEmpty(resp))
                {
                    lookUpRes = JsonConvert.DeserializeObject<AirtelPPHLRResp>(resp);
                }
            }
            catch (Exception ex)
            {
                resp = resp + ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetHLRINFOAPI",
                    Error = resp,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            string MobileNo = Mobile;
            var _req = new LookUpDBLogReq
            {
                APIID = APIID,
                APIType = LookupAPIType.VASTWEB,
                LoginID = UserId,
                Mobile = string.IsNullOrEmpty(MobileNo) ? Mobile : MobileNo,
                Request = URL,
                CurrentCircle = "0",
                CurrentOperator = "0",
                Response = resp
            };
            IProcedure proc = new ProcLookUpAPIReqResp(_dal);
            res = (HLRResponseStatus)proc.Call(_req);
            if (lookUpRes != null)
            {
                res.CommonStr = LookupAPIType.AirtelPPHLR;
                res.CommonBool = lookUpRes.success;
                res.Msg = lookUpRes.desc;
                if (lookUpRes.success)
                    res.Statuscode = ErrorCodes.One;                    
                else
                    res.Statuscode = ErrorCodes.Minus1;
            }
            return res;
        }
        #endregion
    }
}
