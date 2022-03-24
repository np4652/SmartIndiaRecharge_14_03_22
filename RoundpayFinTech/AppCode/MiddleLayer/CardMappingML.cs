using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
   
    public class CardMappingML: Interfaces.ICardMapping
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly LoginResponse _lr;

        public CardMappingML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
        }

        public IEnumerable<CardAccountMapping> GetCardAccount(int UserId)
        {
            var resp = new List<CardAccountMapping>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser )
            {
                var req = new CardAccountMapping
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                     UserID= Convert.ToString(UserId),
                     CommonInt=-1
                };
                IProcedure _proc = new ProcCardAccountList(_dal);
                resp = (List<CardAccountMapping>)_proc.Call(req);
            }
            return resp;
        }


        public IResponseStatus UpdateCardAccount(CardAccountMapping req)
        {
            try
            {
                var _resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AuthError
                };

                if (string.IsNullOrWhiteSpace(req.AccountNo) || req.AccountNo.Length !=16 )
                {
                    _resp.Msg = ErrorCodes.InvalidParam + "Card Number";
                    return _resp;
                }
                if (string.IsNullOrEmpty(req.ValidThru) || string.IsNullOrEmpty(req.Validfrom))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Dates";
                    return _resp;
                }

                if (Convert.ToDateTime(req.ValidThru) < Convert.ToDateTime(req.Validfrom))
                {
                    _resp.Msg = ErrorCodes.InvalidParam +  "From date Is Greater then Through date. ";
                    return _resp;
                }




                if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)))
                {
                    req.LT = _lr.LoginTypeID;
                    req.LoginID = _lr.LoginTypeID;
                    IProcedure _proc = new procUpdateCardAccount(_dal);
                    _resp = (ResponseStatus)_proc.Call(req);
                }
                return _resp;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateCardAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });

                throw;
            }


        }

        public IResponseStatus DeleteCardAccount(CardAccountMapping req)
        {
            try
            {
                var _resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AuthError
                };
                if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)))
                {

                    req.LT = _lr.LoginTypeID;
                    req.LoginID = _lr.LoginTypeID;
                    IProcedure _proc = new procDeleteCardAccount(_dal);
                    _resp = (ResponseStatus)_proc.Call(req);
                }


                return _resp;
            }

            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DeleteCardAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });

                throw;
            }


        }










    }
}
