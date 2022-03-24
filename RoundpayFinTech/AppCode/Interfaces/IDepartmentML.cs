using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IDepartmentML
    {
        Department GetDepartment(int ID);
        IEnumerable<Department> GetDepartment();
        DepartmentRole GetDepartmentRole(int ID);
        IEnumerable<DepartmentRole> GetDepartmentRoles(int DeptID = 0);
        IResponseStatus SaveDepartment(Department department);
        IResponseStatus SaveDepartmentRole(DepartmentRole departmentRole);
        Task<IEnumerable<TemplateFormatKeyMappingDisplay>> GetMapMsgTamplateToKey(string str);
        Task<IResponseStatus> MapTemplateAndKey(int FormatID, int KeyID, bool IsActive);
    }
    public interface IMenuOpsML {
        Task<IEnumerable<MasterMenu>> GetMenuOperations(string str);
        Task<IResponseStatus> UpdateMenuOperations(MenuOperation mo);
        Task<IEnumerable<TemplateFormatKeyMappingDisplay>> GetMapMsgTamplateToKey(string str);
    }
}
