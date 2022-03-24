using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPromoCodeDetail : IProcedure
    {
        private readonly IDAL _dal;
        List<PromoCode> lstPromoCode = null;
        public ProcGetPromoCodeDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (PromoCode)obj;
            SqlParameter[] param =
            {
                  new SqlParameter("@LT",_req.LT),
                new SqlParameter("@LoginID",_req.LoginID),
                 new SqlParameter("@ID",_req.ID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    lstPromoCode = new List<PromoCode>();
                    for(int i=0;i<dt.Rows.Count;i++)
                    {
                        PromoCode promocode = new PromoCode()
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            Promocode = dt.Rows[i]["_Promocode"].ToString(),
                            Description = dt.Rows[i]["_Description"].ToString(),
                            OID = Convert.ToInt32(dt.Rows[i]["_OID"]),
                            OpTypeID = Convert.ToInt32(dt.Rows[i]["_OpTypeID"]),
                            CashBack = Convert.ToDecimal(dt.Rows[i]["_Cashback"]),
                            IsFixed = Convert.ToBoolean(dt.Rows[i]["_IsFixed"]),
                            CashbackMaxCycle = Convert.ToInt32(dt.Rows[i]["_CashbackMaxCycle"]),
                            CashBackCycleType = Convert.ToInt32(dt.Rows[i]["_CashbackCycleType"]),
                            AfterTransNumber = Convert.ToInt32(dt.Rows[i]["_AfterTransactionNum"]),
                            IsGift = Convert.ToBoolean(dt.Rows[i]["_IsGift"]),
                            Extension = dt.Rows[i]["_Extension"].ToString(),
                            ValidFrom = dt.Rows[i]["_ValidFrom"].ToString(),
                            ValidTill = dt.Rows[i]["_ValidTill"].ToString(),
                            ModifyDate = dt.Rows[i]["_ModifyDate"].ToString(),
                            OpType = dt.Rows[i]["_OpType"].ToString(),
                            OperatorName = dt.Rows[i]["_Operator"].ToString(),
                            CycleType = dt.Rows[i]["_CycleType"].ToString(),
                            OfferDetail = dt.Rows[i]["_OfferDetail"].ToString()
                        };
                        lstPromoCode.Add(promocode);
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return lstPromoCode;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetPromoCodeDetail";
    }
}
