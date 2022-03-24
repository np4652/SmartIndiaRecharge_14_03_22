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
    public class ProcGetPromoCodeByID : IProcedure
    {
        private readonly IDAL _dal;
        PromoCode promocode = null;
        public ProcGetPromoCodeByID(IDAL dal)
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
                   
                   
                        promocode = new PromoCode()
                        {
                            ID = Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Promocode = dt.Rows[0]["_Promocode"].ToString(),
                            Description = dt.Rows[0]["_Description"].ToString(),
                            OID = Convert.ToInt32(dt.Rows[0]["_OID"]),
                            OpTypeID = Convert.ToInt32(dt.Rows[0]["_OpTypeID"]),
                            CashBack = Convert.ToDecimal(dt.Rows[0]["_Cashback"]),
                            IsFixed = Convert.ToBoolean(dt.Rows[0]["_IsFixed"]),
                            CashbackMaxCycle = Convert.ToInt32(dt.Rows[0]["_CashbackMaxCycle"]),
                            CashBackCycleType = Convert.ToInt32(dt.Rows[0]["_CashbackCycleType"]),
                            AfterTransNumber = Convert.ToInt32(dt.Rows[0]["_AfterTransactionNum"]),
                            IsGift = Convert.ToBoolean(dt.Rows[0]["_IsGift"]),
                            Extension = dt.Rows[0]["_Extension"].ToString(),
                            ValidFrom = dt.Rows[0]["_ValidFrom"].ToString(),
                            ValidTill = dt.Rows[0]["_ValidTill"].ToString(),
                            ModifyDate = dt.Rows[0]["_ModifyDate"].ToString(),
                            OpType = dt.Rows[0]["_OpType"].ToString(),
                            OperatorName = dt.Rows[0]["_Operator"].ToString(),
                            CycleType = dt.Rows[0]["_CycleType"].ToString(),
                            OfferDetail = dt.Rows[0]["_OfferDetail"].ToString()
                        };
                       
                    

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
            return promocode;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetPromoCodeDetail";
    }
}
