using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundReceiveInvoice : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundReceiveInvoice(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TransId", req.CommonStr??"")
            };
            var _res = new ProcFundReceiveInvoiceResponse
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    _res.Address = dt.Rows[0]["_CAddress"].ToString();
                    _res.Amount = dt.Rows[0]["_Amount"].ToString();
                    _res.City = dt.Rows[0]["_City"].ToString();
                    _res.EmailId = dt.Rows[0]["_EmailId"].ToString();
                    _res.EntryDate = dt.Rows[0]["_EntryDate"].ToString();
                    _res.Name = dt.Rows[0]["_OutletName"].ToString();
                    _res.PanNo = dt.Rows[0]["_PAN"].ToString();
                    _res.PinCode = dt.Rows[0]["_PinCode"].ToString();
                    _res.State = dt.Rows[0]["_State"].ToString();
                    _res.TID = dt.Rows[0]["_TransactionID"].ToString();
                    _res.UserId = dt.Rows[0]["_UserID"].ToString();
                    _res.MobileNo = dt.Rows[0]["_MobileNo"].ToString();
                    _res.CName = dt.Rows[0]["_CompanyName"].ToString();                    _res.CAddress = dt.Rows[0]["_CompanyAddress"].ToString();                    _res.CEmail = dt.Rows[0]["_EmailIDSupport"].ToString();                    _res.CMobileNo = dt.Rows[0]["_MobileSupport"].ToString();                    _res.CPhoneNo = dt.Rows[0]["_PhoneNoSupport"].ToString();                    _res.CWhatsapp = dt.Rows[0]["_WhatsAppSupport"].ToString();
                }
            }
            catch (Exception){}
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetFundReceiveInoiceData";
    }
}
