using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBenificiary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBenificiary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            DMTReq req = (DMTReq)obj;
            var res = new BenificiaryModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                benificiaries=new List<BenificiaryDetail>()
            };
            SqlParameter[] param = {
                new SqlParameter("@SenderMob", req.SenderNO??string.Empty)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One) {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            res.benificiaries.Add(new BenificiaryDetail
                            {
                                _ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]),
                                _SenderID = dt.Rows[i]["_SenderID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_SenderID"]),
                                _SenderMobileNo = dt.Rows[i]["_SenderMobileNo"] is DBNull ? "" : dt.Rows[i]["_SenderMobileNo"].ToString(),
                                _Name = dt.Rows[i]["_Name"] is DBNull ? "" : dt.Rows[i]["_Name"].ToString(),
                                _AccountNumber = dt.Rows[i]["_AccountNumber"] is DBNull ? "" : dt.Rows[i]["_AccountNumber"].ToString(),
                                _MobileNo = dt.Rows[i]["_MobileNo"] is DBNull ? "" : dt.Rows[i]["_MobileNo"].ToString(),
                                _IFSC = dt.Rows[i]["_IFSC"] is DBNull ? "" : dt.Rows[i]["_IFSC"].ToString(),
                                _BankName = dt.Rows[i]["_BankName"] is DBNull ? "" : dt.Rows[i]["_BankName"].ToString(),
                                _Branch = dt.Rows[i]["_Branch"] is DBNull ? "" : dt.Rows[i]["_Branch"].ToString(),
                                _DeleteStatus = dt.Rows[i]["_DeleteStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_DeleteStatus"]),
                                _EntryBy = dt.Rows[i]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_EntryBy"]),
                                _EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "" : dt.Rows[i]["_EntryDate"].ToString(),
                                _ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "" : dt.Rows[i]["_ModifyDate"].ToString(),
                                _VerifyStatus = dt.Rows[i]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_VerifyStatus"]),
                                _CashFreeID = dt.Rows[i]["_CashFreeID"] is DBNull ? "" : dt.Rows[i]["_CashFreeID"].ToString()

                            });
                        }
                    }
                }
                else
                {
                    res.Msg = ErrorCodes.NODATA;
                    res.Statuscode = ErrorCodes.One;
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
                    UserId = req.UserID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetBeneficiary";
    }

    public class ProcGetBenificiaryByBeneID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBenificiaryByBeneID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            DMTReq req = (DMTReq)obj;
            var res = new BenificiaryDetail();

            SqlParameter[] param = {
                new SqlParameter("@SenderMob", req.SenderNO),
                new SqlParameter("@BeneAPIID", req.BeneAPIID)
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res._ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res._SenderID = dt.Rows[0]["_SenderID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SenderID"]);
                    res._SenderMobileNo = dt.Rows[0]["_SenderMobileNo"] is DBNull ? "" : dt.Rows[0]["_SenderMobileNo"].ToString();
                    res._Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                    res._AccountNumber = dt.Rows[0]["_AccountNumber"] is DBNull ? "" : dt.Rows[0]["_AccountNumber"].ToString();
                    res._MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                    res._IFSC = dt.Rows[0]["_IFSC"] is DBNull ? "" : dt.Rows[0]["_IFSC"].ToString();
                    res._BankName = dt.Rows[0]["_BankName"] is DBNull ? "" : dt.Rows[0]["_BankName"].ToString();
                    res._Branch = dt.Rows[0]["_Branch"] is DBNull ? "" : dt.Rows[0]["_Branch"].ToString();
                    res._DeleteStatus = dt.Rows[0]["_DeleteStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_DeleteStatus"]);
                    res._EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]);
                    res._EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString();
                    res._ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString();
                    res._VerifyStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VerifyStatus"]);
                    res._BankID = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BankID"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "select _ID, _SenderID, _SenderMobileNo, _Name, _AccountNumber, _MobileNo, _IFSC, _BankName, _Branch, _EntryDate, _EntryBy, _DeleteStatus, _ModifyDate, _VerifyStatus,_BankID from tbl_Benificiary where _SenderMobileNo=@SenderMob and _DeleteStatus=0 and _BeneAPIID=@BeneAPIID";
        }
    }
}
