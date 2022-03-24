using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMenuOperations : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMenuOperations(IDAL dal) {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            string DevKey = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@DevKey", HashEncryption.O.DevEncrypt(DevKey??""))
            };
            var __res = new List<MasterMenu>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    var masterOperations = new List<MasterOperation>();
                    var menuOperations = new List<MenuOperation>();
                    foreach (DataRow row in dt.Rows)
                    {
                        var masterMenu = new MasterMenu
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Menu = row["_Menu"] is DBNull ? "" : row["_Menu"].ToString()
                        };
                        string MasterOperation = row["MasterOperation"] is DBNull ? "" : row["MasterOperation"].ToString();
                        if (masterOperations.Count == 0)
                        {
                            if (MasterOperation.Contains((char)160))
                            {
                                string[] MstrOs = MasterOperation.Split((char)160);
                                foreach (var mstro in MstrOs)
                                {
                                    if (mstro.Contains("_"))
                                    {
                                        var mstroperation = new MasterOperation
                                        {
                                            ID = Convert.ToInt32(mstro.Split('_')[0]),
                                            OperationName = mstro.Split('_')[1]
                                        };
                                        masterOperations.Add(mstroperation);
                                    }
                                }
                            }
                            else if (MasterOperation.Contains("_"))
                            {
                                var mstroperation = new MasterOperation
                                {
                                    ID = Convert.ToInt32(MasterOperation.Split('_')[0]),
                                    OperationName = MasterOperation.Split('_')[1]
                                };
                                masterOperations.Add(mstroperation);
                            }
                        }
                        masterMenu.MasterOperation = masterOperations;
                        string MenuOperation = row["tblMenuOperation"] is DBNull ? "" : row["tblMenuOperation"].ToString();
                        if (menuOperations.Count == 0)
                        {
                            if (MenuOperation.Contains((char)160))
                            {
                                string[] MenuOs = MenuOperation.Split((char)160);
                                foreach (var menu in MenuOs)
                                {
                                    if (menu.Contains("_"))
                                    {
                                        var menuoperation = new MenuOperation
                                        {
                                            MenuID = Convert.ToInt32(menu.Split('_')[0]),
                                            OperationID = Convert.ToInt32(menu.Split('_')[1]),
                                            IsActive = menu.Split('_')[2] == "0" ? false : true
                                        };
                                        menuOperations.Add(menuoperation);
                                    }
                                }
                            }
                            else if (MenuOperation.Contains("_"))
                            {
                                var menuoperation = new MenuOperation
                                {
                                    MenuID = Convert.ToInt32(MenuOperation.Split('_')[0]),
                                    OperationID = Convert.ToInt32(MenuOperation.Split('_')[1]),
                                    IsActive = MenuOperation.Split('_')[2] == "0" ? false : true
                                };
                                menuOperations.Add(menuoperation);
                            }
                        }
                        masterMenu.MenuOperation = menuOperations;
                        __res.Add(masterMenu);
                    }
                }
            }
            catch (Exception)
            { }
            return __res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_MenuOperations";
        }
    }
}
