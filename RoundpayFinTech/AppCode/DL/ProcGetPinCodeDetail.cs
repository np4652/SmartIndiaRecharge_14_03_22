using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPinCodeDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPinCodeDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            string pcode = (string)obj;
            PincodeDetail pincode = new PincodeDetail()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "postal code not valid"
            };
            SqlParameter[] param = {
                new SqlParameter("@pcode",pcode) };
            DataTable dt = _dal.Get(GetName(),param);
            if (dt.Rows.Count > 0)
            {
                pincode.Statuscode = ErrorCodes.One;
                pincode.Msg = ErrorCodes.SUCCESS;
                pincode.StateID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                pincode.StateName = dt.Rows[0]["StateName"].ToString();
                pincode.Area = dt.Rows[0]["Area"].ToString();
                pincode.City = dt.Rows[0]["City"].ToString();
                pincode.Districtname = dt.Rows[0]["Districtname"].ToString();
                pincode.Statename = dt.Rows[0]["StateName"].ToString();
            }
            return pincode;
            
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
                return "select top 1 s.*,City,Area,Districtname from Master_State s inner join tbl_Pincode p on s._ID=p.StateID where p.Pincode=@pcode";
        }
    }
}
