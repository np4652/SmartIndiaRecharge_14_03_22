using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSavePromoCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSavePromoCode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (PromoCode)obj;
            SqlParameter[] param =
            {
                 new SqlParameter("@LT",_req.LT),
                 new SqlParameter("@ID",_req.ID),
                new SqlParameter("@OpTypeID",_req.OpTypeID),
                new SqlParameter("@OID",_req.OID),
                new SqlParameter("@Promocode",_req.Promocode),
                new SqlParameter("@CashBack",_req.CashBack),
                new SqlParameter("@IsFixed",_req.IsFixed),
                new SqlParameter("@Description",_req.Description),
                new SqlParameter("@CashbackMaxCycle",_req.CashbackMaxCycle),
                new SqlParameter("@CashBackCycleType",_req.CashBackCycleType),
                new SqlParameter("@AfterTransNumber",_req.AfterTransNumber),
                new SqlParameter("@IsGift",_req.IsGift),
                new SqlParameter("@ValidFrom",_req.ValidFrom),
                new SqlParameter("@ValidTill",_req.ValidTill),
                new SqlParameter("@LoginID",_req.LoginID),
                 new SqlParameter("@Extension",_req.Extension),
                 new SqlParameter("@OfferDetail",_req.OfferDetail)


            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.CommonInt = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]);

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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_SavePromoCode";
    }
}
