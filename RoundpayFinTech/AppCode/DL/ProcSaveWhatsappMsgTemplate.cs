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
    public class ProcSaveWhatsappMsgTemplate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveWhatsappMsgTemplate(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                //new SqlParameter("@bulkWhatsappMsgTemplate",req.Tp_SaveWhatsappMsgTemplate)
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

            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "Proc_SaveWhatsappMsgTemplate";
    }
}
