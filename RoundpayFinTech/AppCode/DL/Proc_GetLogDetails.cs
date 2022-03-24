using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class Proc_GetLogDetails : IProcedure
    {
        private readonly IDAL _dal;
        public Proc_GetLogDetails(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@TopRows", req.CommonInt),
                new SqlParameter("@Action", req.CommonStr),
                new SqlParameter("@FilterText", req.CommonStr2),
                new SqlParameter("@Criteria", req.CommonInt2)
            };

            List<ROfferLog> _lstROffer = new List<ROfferLog>();
            List<FetchBillLog> _lstFetchBill = new List<FetchBillLog>();
            List<LookUpLog> _1stLookUpLog = new List<LookUpLog>();
            List<APIUrlHittingLog> _lstAPIUrl = new List<APIUrlHittingLog>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (req.CommonStr == LogType.ROFR)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        ROfferLog _res = new ROfferLog
                        {
                            Request = row["_Req"] is DBNull ? "" : row["_Req"].ToString(),
                            Response = row["_Resp"] is DBNull ? "" : row["_Resp"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            Method = row["_Method"] is DBNull ? "" : row["_Method"].ToString(),

                        };
                        _lstROffer.Add(_res);
                    }
                    return _lstROffer;
                }
                else if (req.CommonStr == LogType.BillFetch)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        FetchBillLog _res = new FetchBillLog
                        {
                            RequestURL = row["_RequestURL"] is DBNull ? "" : row["_RequestURL"].ToString(),
                            Request = row["_Request"] is DBNull ? "" : row["_Request"].ToString(),
                            Response = row["_Response"] is DBNull ? "" : row["_Response"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            BillNo = row["_BillNumber"] is DBNull ? "" : row["_BillNumber"].ToString(),
                            BillDate = row["_BillDate"] is DBNull ? "" : row["_BillDate"].ToString(),
                            DueDate = row["_DueDate"] is DBNull ? "" : row["_DueDate"].ToString(),
                            Amount = row["_Amount"] is DBNull ? 0 : Convert.ToDecimal(row["_Amount"]),
                            CustomerName = row["_CustomerName"] is DBNull ? string.Empty : row["_CustomerName"].ToString(),
                            AccountNumber = row["_AccountNumber"] is DBNull ? string.Empty : row["_AccountNumber"].ToString()
                        };
                        _lstFetchBill.Add(_res);
                    }
                    return _lstFetchBill;
                }
                else if (req.CommonStr == LogType.HLR)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        LookUpLog _res = new LookUpLog
                        {
                            Request = row["_Request"] is DBNull ? "" : row["_Request"].ToString(),
                            Response = row["_Response"] is DBNull ? "" : row["_Response"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            LookUpNumber = row["_LookUpNumber"] is DBNull ? "" : row["_LookUpNumber"].ToString(),
                            CurrentOperator = row["_CurrentOperator"] is DBNull ? "" : row["_CurrentOperator"].ToString(),
                            CurrentCircle = row["_CurrentCircle"] is DBNull ? "" : row["_CurrentCircle"].ToString(),
                        };
                        _1stLookUpLog.Add(_res);
                    }
                    return _1stLookUpLog;
                }
                else if (req.CommonStr == LogType.APIURL)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        APIUrlHittingLog _res = new APIUrlHittingLog
                        {
                            Request = row["_Request"] is DBNull ? "" : row["_Request"].ToString(),
                            Response = row["_Response"] is DBNull ? "" : row["_Response"].ToString(),
                            TransactionId = row["_TransactionId"] is DBNull ? "" : row["_TransactionId"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                        };
                        _lstAPIUrl.Add(_res);
                    }
                    return _lstAPIUrl;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }


        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetLogDetails";
        }
    }
}