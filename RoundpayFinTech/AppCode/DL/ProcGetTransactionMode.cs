using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTransactionMode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTransactionMode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginId", _req.LoginID),
                new SqlParameter("@Code", _req.CommonStr2),
            };
            List<TransactionMode> _lst = new List<TransactionMode>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach(DataRow row in dt.Rows)
                    {
                        TransactionMode _res = new TransactionMode()
                        {
                            TransMode = row["_TransMode"] is DBNull ? string.Empty : row["_TransMode"].ToString(),
                            Code = row["_Code"] is DBNull ? string.Empty : row["_Code"].ToString(),
                            Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                            Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                            Charge = row["_Charge"] is DBNull ? 0 : Convert.ToDecimal(row["_Charge"])
                        };
                        if(!(string.IsNullOrEmpty(_req.CommonStr2)))
                        {
                            return _res;
                        }
                        else
                        {
                            _lst.Add(_res);
                        }
                    }
                }
                return _lst;
            }
            catch (Exception er)
            { }
            return (IResponseStatus)(res);
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetTransactionMode";
        }
    }
}
