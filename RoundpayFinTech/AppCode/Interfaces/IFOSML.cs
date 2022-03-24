using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IFOSML
    {
        IResponseStatus AssignRetailerToFOS(UserRequest req);
        UserRoleSlab GetRole();
        UserList GetList(CommonFilter _filter);
        List<FOSUserExcelModel> GetFOSUserExcel(CommonFilter f);
        UserList GetListFOS(CommonFilter _filter);
    }
}
