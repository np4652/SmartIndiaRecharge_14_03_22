using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PromoCodeML : IPromoCodeML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly IRequestInfo _info;
        private readonly LoginResponse _lrEmp;
        public PromoCodeML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
            bool IsProd = _env.IsProduction();
            _info = new RequestInfo(_accessor, _env);
        }


        public IResponseStatus SavePromoCode(PromoCode PromocodeObj)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };

                 if(_lr.LoginTypeID==LoginType.ApplicationUser)
                {
                if (PromocodeObj.PromoCodeImg != null)
                {
                    if (PromocodeObj.PromoCodeImg.Length > 204800 || PromocodeObj.PromoCodeImg.Length < 102400)
                    {
                        resp.Statuscode = ErrorCodes.Minus1;
                        resp.Msg = "Image Size Must Between 100 kb and 200kb";
                        return resp;
                    }
                    var splitArray = PromocodeObj.PromoCodeImg.FileName.Split(".");
                    var ext = splitArray[splitArray.Length - 1];
                    if (ext != "png" && ext != "PNG" && ext != "jpg" && ext != "jpeg" && ext != "JPEG")
                    {
                        resp.Statuscode = ErrorCodes.Minus1;
                        resp.Msg = "Invalid Image Format";
                        return resp;
                    }
                    if (PromocodeObj.OpTypeID < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid OpType";
                        return resp;
                    }
                    if (String.IsNullOrEmpty(PromocodeObj.Promocode) || PromocodeObj.Promocode.Length > 100 || Validate.O.IsNumeric(PromocodeObj.Promocode))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid Promo code";
                        return resp;
                    }
                    if (!Validate.O.IsDecimal(PromocodeObj.CashBack)|| PromocodeObj.CashBack<1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid CashBack Value";
                        return resp;
                    }

                    if (String.IsNullOrEmpty(PromocodeObj.Description))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please Enter Description";
                        return resp;
                    }
                    if (PromocodeObj.CashbackMaxCycle < 0)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid CashBack Max Cycle";
                        return resp;
                    }
                    if (PromocodeObj.CashBackCycleType < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please select cashback cycle type";
                        return resp;
                    }
                    if (PromocodeObj.AfterTransNumber < 0)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid After Transaction Number";
                        return resp;
                    }
                    if (String.IsNullOrEmpty(PromocodeObj.OfferDetail))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please Enter Offer Detail";
                        return resp;
                    }

                    var _req = new PromoCode
                    {
                        LT = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        ID = PromocodeObj.ID,
                        Extension = "." + ext,
                        Promocode = PromocodeObj.Promocode,
                        Description = PromocodeObj.Description,
                        OpTypeID = PromocodeObj.OpTypeID,
                        OID = PromocodeObj.OID,
                        CashBack = PromocodeObj.CashBack,
                        CashbackMaxCycle = PromocodeObj.CashbackMaxCycle,
                        IsFixed = PromocodeObj.IsFixed,
                        CashBackCycleType = PromocodeObj.CashBackCycleType,
                        IsGift = PromocodeObj.IsGift,
                        AfterTransNumber = PromocodeObj.AfterTransNumber,
                        ValidFrom = PromocodeObj.ValidFrom,
                        ValidTill = PromocodeObj.ValidTill,
                        OfferDetail = PromocodeObj.OfferDetail
                    };
                    IProcedure _proc = new ProcSavePromoCode(_dal);
                    var Resp = (ResponseStatus)_proc.Call(_req);
                    resp.Statuscode = Resp.Statuscode;
                    resp.Msg = Resp.Msg;
                    resp.CommonInt = Resp.CommonInt;
                    if (Resp.Statuscode == ErrorCodes.One)
                    {
                        var res = Validate.O.IsImageValid(PromocodeObj.PromoCodeImg);
                        if(res.Statuscode==ErrorCodes.One)
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(DOCType.PromoCodeImagePath);
                                if (!Directory.Exists(sb.ToString()))
                                {
                                    Directory.CreateDirectory(sb.ToString());
                                }

                                sb.Append(resp.CommonInt.ToString());
                                sb.Append("." + ext);
                                using (FileStream fs = File.Create(sb.ToString()))
                                {
                                    PromocodeObj.PromoCodeImg.CopyTo(fs);
                                    fs.Flush();
                                }
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }
                        else
                        {
                            resp.Statuscode = res.Statuscode;
                            resp.Msg = res.Msg;
                            return resp;
                        }
                     
                    }
                }
                else
                {
                    if (PromocodeObj.OpTypeID < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid OpType";
                        return resp;
                    }
                    if (String.IsNullOrEmpty(PromocodeObj.Promocode) || PromocodeObj.Promocode.Length > 100||Validate.O.IsNumeric(PromocodeObj.Promocode))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid Promo code";
                        return resp;
                    }
                    if ( !Validate.O.IsDecimal(PromocodeObj.CashBack)|| PromocodeObj.CashBack<1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid CashBack Value";
                        return resp;
                    }

                    if (String.IsNullOrEmpty(PromocodeObj.Description))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please Enter Description";
                        return resp;
                    }
                    if (PromocodeObj.CashbackMaxCycle < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid CashBack Max Cycle";
                        return resp;
                    }
                    if (PromocodeObj.CashBackCycleType < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please select cashback cycle type";
                        return resp;
                    }
                    if (PromocodeObj.AfterTransNumber < 1)
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Invalid After Transaction Number";
                        return resp;
                    }

                    if (String.IsNullOrEmpty(PromocodeObj.OfferDetail))
                    {
                        resp.Statuscode = -1;
                        resp.Msg = "Please Enter Offer Detail";
                        return resp;
                    }
                    var _req = new PromoCode
                    {
                        LT = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        ID = PromocodeObj.ID,
                        Promocode = PromocodeObj.Promocode,
                        Description = PromocodeObj.Description,
                        OpTypeID = PromocodeObj.OpTypeID,
                        OID = PromocodeObj.OID,
                        CashBack = PromocodeObj.CashBack,
                        CashbackMaxCycle = PromocodeObj.CashbackMaxCycle,
                        IsFixed = PromocodeObj.IsFixed,
                        CashBackCycleType = PromocodeObj.CashBackCycleType,
                        IsGift = PromocodeObj.IsGift,
                        AfterTransNumber = PromocodeObj.AfterTransNumber,
                        ValidFrom = PromocodeObj.ValidFrom,
                        ValidTill = PromocodeObj.ValidTill,
                        OfferDetail=PromocodeObj.OfferDetail
                       
                    };
                    IProcedure _proc = new ProcSavePromoCode(_dal);
                    var Resp = (ResponseStatus)_proc.Call(_req);
                    resp.Statuscode = Resp.Statuscode;
                    resp.Msg = Resp.Msg;
                }
            }
                
                  
            return resp;
        }
       public List<PromoCode> GetPromoCodeDetail()
        {
            var req = new PromoCode
            {
                LT = _lr.LoginTypeID,
                LoginID = _lr.UserID
            };
             IProcedure _proc = new ProcGetPromoCodeDetail(_dal);
            return (List<PromoCode>)_proc.Call(req);
        }
        public PromoCode GetPromoCodeByID(int ID)
        {
            if(_lr.LoginTypeID==LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var req = new PromoCode
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ID=ID
                };
                IProcedure _proc = new ProcGetPromoCodeByID(_dal);
                return (PromoCode)_proc.Call(req);
            }
            return new PromoCode();
        }
        public IResponseStatus UploadPromoImage(int ID,IFormFile PromoImage)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "PENDING"
            };
            if(_lr.LoginTypeID==LoginType.ApplicationUser)
            {
                if (PromoImage.Length > 204800 || PromoImage.Length < 102400)
                {
                    resp.Statuscode = ErrorCodes.Minus1;
                    resp.Msg = "Image Size Must Between 100 kb and 200kb";
                    return resp;
                }
                var splitArray = PromoImage.FileName.Split(".");
                var ext = splitArray[splitArray.Length - 1];
                if(ext!="png" && ext!="PNG" && ext !="jpg" && ext !="jpeg" && ext !="JPEG")
                {
                    resp.Statuscode = ErrorCodes.Minus1;
                    resp.Msg = "Invalid Image Format";
                    return resp;
                }
                var req = new PromoCode
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ID=ID,
                    Extension = "." + ext
                };
                IProcedure _proc = new ProcSavePromoCode(_dal);
                var Resp = (ResponseStatus)_proc.Call(req);
                resp.Statuscode = Resp.Statuscode;
                resp.Msg = Resp.Msg;
                if(Resp.Statuscode==ErrorCodes.One)
                {
                    var res = Validate.O.IsImageValid(PromoImage);
                    if(res.Statuscode==ErrorCodes.One)
                    {
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.PromoCodeImagePath);
                            var PromoCode = GetPromoCodeByID(ID);
                            if(ID+ PromoCode.Extension!=ID+"."+ext)
                            {
                                if (System.IO.File.Exists(DOCType.PromoCodeImagePath + ID + PromoCode.Extension))
                                {
                                    System.IO.File.Delete(DOCType.PromoCodeImagePath + ID + PromoCode.Extension);
                                }
                            }
                            
                            if (!Directory.Exists(DOCType.PromoCodeImagePath))
                            {
                                Directory.CreateDirectory(DOCType.PromoCodeImagePath);
                            }
                            sb.Append(ID.ToString());
                            sb.Append("." + ext);
                            using (FileStream fs = File.Create(sb.ToString()))
                            {
                                PromoImage.CopyTo(fs);
                                fs.Flush();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        resp.Statuscode = res.Statuscode;
                        resp.Msg = res.Msg;
                        return resp;

                    }
              
                   
                }
        }
            return resp;
        }
        public List<PromoCode> GetPromoCodeByOpTypeOID(PromoCode prc)
        {
            if (prc.LT == LoginType.ApplicationUser)
            {
                var req = new PromoCode
                {
                    OpTypeID = prc.OpTypeID,
                    OID= prc.OID
                };
                IProcedure _proc = new ProcGetPromoCodeByOpTypeOID(_dal);
                return (List<PromoCode>)_proc.Call(req);
            }
            return new List<PromoCode>();
        }
    }
}
