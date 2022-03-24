using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSavePESForm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSavePESForm(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            SavePESFormModel req = (SavePESFormModel)obj;
            DataTable TP_FieldValuesList = ConverterHelper.O.ToDataTable(req.FieldValuesList);
            SqlParameter[] param = {
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@RequestModeID",req.RequestModeID),
                new SqlParameter("@APIRequestID",req.APIRequestID??""),
                new SqlParameter("@AccountNo",req.AccountNo??""),
                new SqlParameter("@RequestIP",req.RequestIP??""),
                new SqlParameter("@IMEI",req.IMEI??""),
                new SqlParameter("@Customername",req.Customername??""),
                new SqlParameter("@CustomerMobno",req.CustomerMobno??""),
                new SqlParameter("@TP_FieldValuesList",TP_FieldValuesList)
            };
            var resp = new ResponseStatus();
            resp.Statuscode = ErrorCodes.Minus1;
            resp.Msg = ErrorCodes.AnError;
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (resp.Statuscode==ErrorCodes.One)
                    {
                        resp.CommonStr = dt.Rows[0]["Tid"].ToString();
                    }
                }
            }
            catch (Exception er)
            {
                resp.Statuscode = ErrorCodes.Minus1;
                resp.Msg = ErrorCodes.AnError;
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SavePESData";
        }
    }
}
