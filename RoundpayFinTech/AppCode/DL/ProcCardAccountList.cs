using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{

  
    public class ProcCardAccountList : IProcedure
    {

        private readonly IDAL _dal;
        public ProcCardAccountList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CardAccountMapping req = (CardAccountMapping)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserId",req.UserID)
            };
            List<CardAccountMapping> res = new List<CardAccountMapping> { };
          
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var data = new CardAccountMapping
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                UserID = row["_UserID"] is DBNull ? "" : Convert.ToString(row["_UserID"]),
                                AccountNo = row["_AccountNo"] is DBNull ? "" : Convert.ToString(row["_AccountNo"]),
                                Validfrom = row["_Validfrom"] is DBNull ? "" : Convert.ToDateTime(row["_Validfrom"]).ToString("dd MMM yyyy"),
                                ValidThru = row["_ValidThru"] is DBNull ? "" : Convert.ToDateTime(row["_ValidThru"]).ToString("dd MMM yyyy")

                            };
                            res.Add(data);
                        }
                    }
                    else
                    {
                        return new CardAccountMapping
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            UserID = dt.Rows[0]["_UserID"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_UserID"]),
                            AccountNo = dt.Rows[0]["_AccountNo"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_AccountNo"]),
                            Validfrom = dt.Rows[0]["_Validfrom"] is DBNull ? "" : (Convert.ToDateTime(dt.Rows[0]["_Validfrom"]).ToString("dd MMM yyyy")),
                            ValidThru = dt.Rows[0]["_ValidThru"] is DBNull ? "" : (Convert.ToDateTime(dt.Rows[0]["_ValidThru"]).ToString("dd MMM yyyy"))

                        };
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
            if (req.CommonInt == -1)
                return res;
            else
                return new CardAccountMapping();



        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetCardAccountList";
    }


    public class procUpdateCardAccount : IProcedure
    {

        private readonly IDAL _dal;
        public procUpdateCardAccount(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CardAccountMapping)obj;
            SqlParameter[] param =
                {

                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LT),
                 new SqlParameter("@UserID", req.UserID),
                  new SqlParameter("@AccountNo", req.AccountNo),
                   new SqlParameter("@Validfrom ", Convert.ToDateTime(req.Validfrom)),
                       new SqlParameter("@ValidThru ", Convert.ToDateTime(req.ValidThru)),
                       new SqlParameter("@ID ", req.ID),

            };
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });



            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateCardAccount";
    }


    public class procDeleteCardAccount : IProcedure
    {

        private readonly IDAL _dal;
        public procDeleteCardAccount(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CardAccountMapping)obj;
            SqlParameter[] param =
                {

                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LT),
                 new SqlParameter("@UserID", req.UserID),
                  new SqlParameter("@ID", req.ID),
                   

            };
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                }
            }
            catch (Exception ex) { }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_DeleteCardAccount";
    }




}
