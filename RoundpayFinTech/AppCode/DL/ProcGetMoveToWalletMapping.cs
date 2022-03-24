using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMoveToWalletMapping : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMoveToWalletMapping(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();

        public object Call()
        {
            var res = new List<MoveToWalletMapping>();
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new MoveToWalletMapping
                        {
                            ID=item["_ID"] is DBNull? 0:Convert.ToInt32(item["_ID"]),
                            FromWalletID=item["_FromWalletID"] is DBNull? 0:Convert.ToInt32(item["_FromWalletID"]),
                            ToWalletID=item["_ToWalletID"] is DBNull? 0:Convert.ToInt32(item["_ToWalletID"]),
                            FromWalletType = item["_FromWalletType"] is DBNull? string.Empty:item["_FromWalletType"].ToString(),
                            ToWalletType = item["_ToWalletType"] is DBNull? string.Empty:item["_ToWalletType"].ToString()
                        });
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
                    UserId = 1
                });
            }
            return res;
        }

        public string GetName() => "Proc_GetMoveToWalletMapping";
    }
}
