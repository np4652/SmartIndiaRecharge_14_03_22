using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IMembershipML
    {

        IEnumerable<MembershipMaster> GetMembershipMaster();
        MembershipMaster GetMembershipMaster(int SlabID);
        IResponseStatus UpdateMembershipMaster(MembershipMaster MemMaster);
        IResponseStatus ChangeMemberShipStatus(int ID);
        IResponseStatus ChangeCouponAllowedStatus(int ID);
        IEnumerable<MembershipmasterB2C> GetB2CMemberShipType(int LoginID);
        ResponseStatus GetMemberShip(CommonReq req);
        IEnumerable<B2CMemberCouponDetail> GetB2CCoupon(int LoginID);
        ResponseStatus RedeemCoupon(int UserID, string CouponCode);
    }
}
