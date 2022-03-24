using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
 
    public class ProcSaveWhatsAppGroup : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveWhatsAppGroup(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (SaveWhtsappGroup)obj;
            SqlParameter[] param = {
                new SqlParameter("@MobileNo",(req.MobileNo)),
                new SqlParameter("@GroupName",req.GroupName),
                new SqlParameter("@GroupID",req.GroupID),
                new SqlParameter("@Apiid",req.Apiid),
                new SqlParameter("@SenderNo",req.SenderNo)
            };
           
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_SaveWhatsAppGroup";
    }
}
