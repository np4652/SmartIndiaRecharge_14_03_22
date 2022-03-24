using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ICustomercareML
    {
        Task<IEnumerable<Customercare>> GetCustomerCares(CommonReq req);
        Task<Customercare> GetCustomerCare(int ID);
        IResponseStatus CustomerCareCU(Customercare customercare);
        IEnumerable<MenuOperationAssigned> CCOperationAssigned(int RoleID);
        IResponseStatus UpdateOperationAssigned(int RoleID, int MenuID, int OperationID, bool IsActive);
        IResponseStatus ChangeOTPStatusCC(int CCID, int type);
        IEnumerable<OperationAssigned> GetOperationAssigneds(int RoleID);
    }
}
