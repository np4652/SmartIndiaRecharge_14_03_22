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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class procUpdateCallback : IProcedure
    {
        private readonly IDAL _dal;
        public procUpdateCallback(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            
            try
            {
                SqlParameter[] param = {
                  new SqlParameter("@IP", req.CommonStr2),
                  new SqlParameter("@Url", req.CommonStr)
                };
                DataTable dt = _dal.Get(GetName(), param);
            }
            catch (Exception ex)
            {
            }
            return true;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "insert into tbl_ApiCallBackUrl(_Ip, _Url, _Entrydate)values (@IP,@Url,getdate())";
        }

    }
}
