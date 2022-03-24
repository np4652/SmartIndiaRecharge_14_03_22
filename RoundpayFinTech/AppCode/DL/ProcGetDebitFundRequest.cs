using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDebitFundRequest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDebitFundRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (DebitFundrequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                 new SqlParameter("@Search",req.Criteria),
                  new SqlParameter("@Top",req.TopRows),
                   new SqlParameter("@ToDate",req.ToDate),
                    new SqlParameter("@Status",Convert.ToInt32(req.Status)),
                    new SqlParameter("@FromDate",req.FromDate),
                    new SqlParameter("@Searchtxt",req.CriteriaText??""),


            };
            var res = new List<DebitFundrequest>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var debitfundrequest = new DebitFundrequest
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Amount = row["_Amount"] is DBNull ? null : Convert.ToString(row["_Amount"]),
                            Remark = row["_Remark"] is DBNull ? null : Convert.ToString(row["_Remark"]),
                            FromName = row["AdminName"] is DBNull ? null : Convert.ToString(row["AdminName"]),
                            ToName = row["_Name"] is DBNull ? null : Convert.ToString(row["_Name"]),
                            RequetedDate = row["_EntryDate"] is DBNull ? null : Convert.ToString(row["_EntryDate"]),
                            Status = row["_Status"] is DBNull ? null : Convert.ToString(row["_Status"]),
                            MobileNo = row["_Umobile"] is DBNull ? null : Convert.ToString(row["_Umobile"]),
                            MobileNo1 = row["AdminMobile"] is DBNull ? null : Convert.ToString(row["AdminMobile"]),
                            IsAccountStatmentEntry = row["_IsAccountStatmentEntry"] is DBNull ? false : Convert.ToBoolean(row["_IsAccountStatmentEntry"])
                        };
                        res.Add(debitfundrequest);

                    }


                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;

        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "ProcGetDebitFundRequest";
    }



    public class ProcDebitRquesttStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDebitRquesttStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TableID",req.CommonInt),
                new SqlParameter("@Status",req.CommonInt2),
                new SqlParameter("@Browser",req.CommonStr??string.Empty),
                new SqlParameter("@ip",req.CommonStr2??string.Empty),
                new SqlParameter("@IsMarkCredit",req.CommonBool),
                new SqlParameter("@ApprovedRemark",req.CommonStr3??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_UpdateDebitRquesttStatus";
    }


    public class ProcCanDabitStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCanDabitStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)



            };
            var res = new UserList();

            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    // res.Candebit = dt.Rows[0]["_Candebit"]  is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_Candebit"]);
                    res.Candebit = dt.Rows[0]["_Candebit"] is DBNull ? true : Convert.ToBoolean(dt.Rows[0]["_Candebit"]);
                    res.CandebitDownline = dt.Rows[0]["_CandebitDownline"] is DBNull ? true : Convert.ToBoolean(dt.Rows[0]["_CandebitDownline"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "select _CanDebit,_CandebitDownline from tbl_users where _ID=@LoginID";
    }
}
