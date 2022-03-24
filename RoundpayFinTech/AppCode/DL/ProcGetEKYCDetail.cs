using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetEKYCDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEKYCDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@IsExternal",req.CommonBool),
                new SqlParameter("@EKYCID",req.CommonInt)
            };
            var res = new EKYCGetDetail { 
            EKYCType=new List<EKYCTypeModel>()
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        res.EKYCID = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
                        res.CompanyTypeID = dt.Rows[0]["_CompanyTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CompanyTypeID"]);
                        res.StepCompleted = dt.Rows[0]["_StepCompleted"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StepCompleted"]);
                        res.IsGSTINEKYCDone = dt.Rows[0]["_IsGSTINEKYCDone"] is DBNull? false : Convert.ToBoolean(dt.Rows[0]["_IsGSTINEKYCDone"]);
                        res.IsAadharEKYCDone = dt.Rows[0]["_IsAadharEKYCDone"] is DBNull? false : Convert.ToBoolean(dt.Rows[0]["_IsAadharEKYCDone"]);
                        res.IsPANEKYCDone = dt.Rows[0]["_IsPANEKYCDone"] is DBNull? false : Convert.ToBoolean(dt.Rows[0]["_IsPANEKYCDone"]);
                        res.IsBanckAccountEKYCDone = dt.Rows[0]["_IsBanckAccountEKYCDone"] is DBNull? false : Convert.ToBoolean(dt.Rows[0]["_IsBanckAccountEKYCDone"]);
                        res.IsGSTSkipped = dt.Rows[0]["_IsGSTSkipped"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsGSTSkipped"]);
                        res.IsEKYCDone = dt.Rows[0]["_IsEKYCDone"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEKYCDone"]);
                        res.IsGSTIN = dt.Rows[0]["_IsGSTIN"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsGSTIN"]);
                        res.GSTAuthorizedSignatory = dt.Rows[0]["_GSTAuthorizedSignatory"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_GSTAuthorizedSignatory"]);
                        res.GSTIN = dt.Rows[0]["_GSTIN"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_GSTIN"]);
                        res.PAN = dt.Rows[0]["_PANNo"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_PANNo"]);
                        res.PAN = dt.Rows[0]["_PANNo"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_PANNo"]);
                        res.AadharNo = dt.Rows[0]["_AadharNo"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AadharNo"]);
                        res.SelectedDirector = dt.Rows[0]["_SelectedDirector"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_SelectedDirector"]);
                        res.PanOfDirector = dt.Rows[0]["_PanOfDirector"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_PanOfDirector"]);
                        res.AccountNumber = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountNumber"]);
                        res.AccountHolder = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountHolder"]);
                        res.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_IFSC"]);
                        res.EditStepID = dt.Rows[0]["_EditStepID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_EditStepID"]);
                        res.DOB = dt.Rows[0]["_DOB"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_DOB"]);
                        res.OutletName = dt.Rows[0]["_OutletName"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_OutletName"]);
                        res.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_Name"]);
                    }
                    if (ds.Tables.Count > 1) {
                        var dt2 = ds.Tables[1];
                        foreach (DataRow item in dt2.Rows)
                        {
                            res.EKYCType.Add(new EKYCTypeModel
                            {
                                ID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                                EKYCStep = item["_EKYCStep"] is DBNull ? string.Empty : Convert.ToString(item["_EKYCStep"])
                            }) ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetEKYCDetail";
    }
    public class ProcLogEKYCAPIReqResp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLogEKYCAPIReqResp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@Request",req.CommonStr??string.Empty),
                new SqlParameter("@Response",req.CommonStr2??string.Empty)
            };
            try
            {
                 _dal.Execute(GetName(),param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.CommonInt
                });
            }
            return true;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "insert into Log_EKYCAPiReqResp(_APIID,_Request,_Response,_EntryDate)values(@APIID,@Request,@Response,getdate())";
    }
}
