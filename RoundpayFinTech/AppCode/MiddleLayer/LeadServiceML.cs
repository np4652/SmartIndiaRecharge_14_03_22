using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
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
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class LeadServiceML : ILeadServiceML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly IRequestInfo _info;
        private readonly LoginResponse _lrEmp;
        public LeadServiceML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
            bool IsProd = _env.IsProduction();
            _info = new RequestInfo(_accessor, _env);
        }
        public List<LoanTypes> GetLoanType()
        {
            
                var commonReq = new CommonReq
                {
                  
                    LoginTypeID =LoginType.ApplicationUser,

                };
                IProcedure _proc = new procGetLoanType(_dal);
                return (List<LoanTypes>)_proc.Call(commonReq);
         
        }
      
        public List<CustomerTypes> GetCustomerType()
        {
            
                var commonReq = new CommonReq
                {
                  
                    LoginTypeID = LoginType.ApplicationUser,

                };
                IProcedure _proc = new procGetCustomerType(_dal);
                return (List<CustomerTypes>)_proc.Call(commonReq);
            
            return new List<CustomerTypes>();
        }


        public List<InsuranceTypes> GetInsuranceTypes()
        {
            
                var commonReq = new CommonReq
                {
                  
                    LoginTypeID = LoginType.ApplicationUser,

                };
                IProcedure _proc = new procGetInsuranceTypes(_dal);
                return (List<InsuranceTypes>)_proc.Call(commonReq);
            
            return new List<InsuranceTypes>();
        }

        public IResponseStatus SaveLeadService(LeadService req)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };

            try
            {
                if (String.IsNullOrEmpty(req.Name) || req.Name.Length > 100)
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Invalid Name";
                    return resp;
                }
                if (!Validate.O.IsMobile(req.Mobile))
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Invalid Mobile No.";
                    return resp;
                }

                if (!String.IsNullOrEmpty(req.Email)  && !Validate.O.IsEmail(req.Email))
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Invalid Email Id.";
                    return resp;
                }
                if (!String.IsNullOrEmpty(req.PAN) && !Validate.O.IsPAN(req.PAN))
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Invalid PAN";
                    return resp;
                }
                if (!String.IsNullOrEmpty(req.PinCode) && !Validate.O.IsPinCode(req.PinCode))
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Invalid Pin Code";
                    return resp;
                }

                var _req = new LeadServiceRequest
                {
                    LT=_lr.LoginTypeID,
                    Name=req.Name,
                    Email=req.Email,
                    Mobile = req.Mobile,
                    Age =req.Age,
                    PAN=req.PAN,
                    LoanTypeID=req.LoanTypeID,
                    InsuranceTypeID=req.InsuranceTypeID,
                    Amount=req.Amount,
                    CustomerTypeID=req.CustomerTypeID,
                    RequiredFor=req.RequiredFor,
                    Comments=req.Comments,
                    Remark=req.Remark,
                    EntryBy = _lr.UserID,
                    RequestIP = _info.GetRemoteIP(),
                    Browser = _info.GetBrowser(),
                    RequestModeID = RequestMode.PANEL,
                    OID=req.OID,
                    BankID = req.BankID,
                    HaveLoan = req.HaveLoan,
                    OccupationType = req.OccupationType,
                     PinCode = req.PinCode
                };
                IProcedure _proc = new ProcLeadService(_dal);
                var saveLeadresp = (ResponseStatus)_proc.Call(_req);
                resp.Statuscode = saveLeadresp.Statuscode;
                resp.Msg = saveLeadresp.Msg;
            }
            catch { }
              
            return resp;
        }
        public IResponseStatus SaveLeadServiceApp(LeadServiceRequest req)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };

            try
            {
                req.RequestIP = _info.GetRemoteIP();
                req.Browser = _info.GetBrowser();
                IProcedure _proc = new ProcLeadService(_dal);
                var saveLeadresp = (ResponseStatus)_proc.Call(req);
                resp.Statuscode = saveLeadresp.Statuscode;
                resp.Msg = saveLeadresp.Msg;
            }
            catch { }

            return resp;
        }
        public List<LeadServiceRequest> GetLeadServiceRequest(LeadServiceRequest LeadReq)
        {
        
            var LoginResp= chkAlternateSession();
            var req = new LeadServiceRequest(); 
                    Validate validate = Validate.O;
                    req.LT = LoginType.ApplicationUser;
                    req.OID = LeadReq.OID;
                    req.FromDate = LeadReq.FromDate;
                    req.ToDate = LeadReq.ToDate;
                    req.Mobile = LeadReq.Mobile;
                    req.ID = LeadReq.ID;
                    req.LoginID = LeadReq.UserID;
                    req.Criteria = LeadReq.Criteria;
                    req.CriteriaText = LeadReq.CriteriaText;
                    req.TopRows = LeadReq.TopRows;
                    req.RequestModeID = LeadReq.RequestModeID;
                    if (LeadReq.IsAdmin)
                    {
                        if (req.Criteria > 0)
                        {
                            if (LeadReq.Criteria == Criteria.OutletMobile)
                            {
                                if (validate.IsMobile(LeadReq.CriteriaText))
                                {
                                    req.OutletNo = LeadReq.CriteriaText;
                                }
                                
                            }
                            if (req.Criteria == Criteria.UserID)
                            {
                                var Prefix = Validate.O.Prefix(LeadReq.CriteriaText);
                                if (Validate.O.IsNumeric(Prefix))
                                    req.UserID = Validate.O.IsNumeric(LeadReq.CriteriaText) ? Convert.ToInt32(LeadReq.CriteriaText) : req.UserID;
                                var uid = Validate.O.LoginID(req.CriteriaText);
                                req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : req.UserID;
                            }
                        }
                    }
                   
                
            
            
                  IProcedure _proc = new ProcGetLeadServiceRequest(_dal);
                 return (List<LeadServiceRequest>)_proc.Call(req);


        }
        

        public IResponseStatus UpdateLeadServiceRequest(int ID,string Remark, int LeadStatus)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };

            try
            {
                if (ID == 0)
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Record cannot be updated";
                    return resp;
                }
                if (String.IsNullOrEmpty(Remark) || LeadStatus == 0)
                {
                    resp.Statuscode = -1;
                    resp.Msg = "Please fill Remark and Lead Status Both.";
                    return resp;
                }
                var _req = new LeadServiceRequest
                {
                    LT = _lr.LoginTypeID,
                    ID=ID,
                    Remark=Remark,
                    LeadStatus=LeadStatus,
                    ModifyBy=_lr.UserID
                };
                IProcedure _proc = new ProcUpdateLeadServiceRequest(_dal);
                var saveLeadresp = (ResponseStatus)_proc.Call(_req);
                resp.Statuscode = saveLeadresp.Statuscode;
                resp.Msg = saveLeadresp.Msg;
            }
            catch { }

            return resp;
        }

        //public IResponseStatus UpdateLeadServiceRequestApp(LeadServiceRequest _req)
        //{
        //    IResponseStatus resp = new ResponseStatus
        //    {
        //        Statuscode = 1,
        //        Msg = "PENDING"
        //    };

        //    try
        //    {
             
        //        IProcedure _proc = new ProcUpdateLeadServiceRequest(_dal);
        //        var saveLeadresp = (ResponseStatus)_proc.Call(_req);
        //        resp.Statuscode = saveLeadresp.Statuscode;
        //        resp.Msg = saveLeadresp.Msg;
        //    }
        //    catch { }

        //    return resp;
        //}

        private LoginResponse chkAlternateSession()
        {
            var result = new LoginResponse();
            if (_lr != null)
            {
                result = _lr;
            }
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            return result;
        }

        public List<LeadServiceRequest> GetLeadDetailById(int ID)
        {

            var LoginResp = chkAlternateSession();
            var req = new LeadServiceRequest();
            if (LoginResp != null)
            {
                if (LoginResp.LoginTypeID == LoginType.ApplicationUser)
                {
                    Validate validate = Validate.O;
                    req.LT = LoginType.ApplicationUser;
                    req.ID =ID;
                    req.LoginID = LoginResp.UserID;

                }

            }
            IProcedure _proc = new ProcGetLeadServiceRequest(_dal);
            return (List<LeadServiceRequest>)_proc.Call(req);


        }
    }
}
