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
  
    public class ProcSelectSenderNo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSelectSenderNo(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var getWhatsappSenderNoList = new List<GetWhatsappContact>();

            var resp = new GetWhatsappContactListModel
            {
                GetWhatsappSenderNoList = getWhatsappSenderNoList
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var GetWhatsappSenderNoList = new GetWhatsappContact
                        {
                            ID = row["_SenderNoID"] is DBNull ? 0 : Convert.ToInt32(row["_SenderNoID"]),
                            MobileNo = row["_MobileNO"] is DBNull ? "" : row["_MobileNO"].ToString(),
                            ApICode = row["_ApiCode"] is DBNull ? "" : row["_ApiCode"].ToString(),
                            APIID = row["_APIIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIIID"])
                        };
                        getWhatsappSenderNoList.Add(GetWhatsappSenderNoList);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_SelectSenderNo";

    }
}
