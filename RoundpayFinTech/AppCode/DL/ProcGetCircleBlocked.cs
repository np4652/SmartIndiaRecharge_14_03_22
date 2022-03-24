using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCircleBlocked : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCircleBlocked(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            List<CircleSwitch> _res = new List<CircleSwitch>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Columns.Contains("OID"))
                    {
                        CircleSwitch circleSwitch = new CircleSwitch
                        {
                            OID = Convert.ToInt32(dt.Rows[i]["OID"]),
                            Operator = dt.Rows[i]["Operator"].ToString(),
                            OpType = dt.Rows[i]["OpType"].ToString()
                        };
                        List<Circle> circleSwitched = new List<Circle>();
                        if (dt.Rows[i]["CircleSwitched"] is DBNull == false)
                        {
                            if (dt.Rows[i]["CircleSwitched"].ToString().Contains("|"))
                            {
                                string[] CWArr = dt.Rows[i]["CircleSwitched"].ToString().Split('|');
                                for (int cwa = 0; cwa < CWArr.Length; cwa++)
                                {
                                    if (CWArr[cwa].Contains((char)160))
                                    {
                                        Circle circle = new Circle
                                        {
                                            ID = Convert.ToInt32(CWArr[cwa].Split((char)160)[0]),
                                            IsActive = CWArr[cwa].Split((char)160)[1] == "1" ? true : false
                                        };
                                        circleSwitched.Add(circle);
                                    }
                                }
                            }
                            else if (dt.Rows[i]["CircleSwitched"].ToString().Contains((char)160))
                            {
                                Circle circle = new Circle
                                {
                                    ID = Convert.ToInt32(dt.Rows[i]["CircleSwitched"].ToString().Split((char)160)[0]),
                                    IsActive = dt.Rows[i]["CircleSwitched"].ToString().Split((char)160)[1] == "1" ? true : false
                                };
                                circleSwitched.Add(circle);
                            }
                        }
                        circleSwitch.CircleSwitched = circleSwitched;
                        List<Circle> Circles = new List<Circle>();
                        if (dt.Rows[i]["Circles"] is DBNull == false)
                        {
                            if (dt.Rows[i]["Circles"].ToString().Contains((char)160))
                            {
                                string[] CWArr = dt.Rows[i]["Circles"].ToString().Split((char)160);
                                for (int cwa = 0; cwa < CWArr.Length; cwa++)
                                {
                                    if (CWArr[cwa].Contains("_"))
                                    {
                                        Circle circle = new Circle
                                        {
                                            ID = Convert.ToInt32(CWArr[cwa].Split('_')[0]),
                                            Name = CWArr[cwa].Split('_')[1]
                                        };
                                        Circles.Add(circle);
                                    }
                                }
                            }
                            else if (dt.Rows[i]["Circles"].ToString().Contains("_"))
                            {
                                Circle circle = new Circle
                                {
                                    ID = Convert.ToInt32(dt.Rows[i]["Circles"].ToString().Split('_')[0]),
                                    Name = dt.Rows[i]["Circles"].ToString().Split('_')[1],
                                };
                                Circles.Add(circle);
                            }
                            circleSwitch.Circles = Circles;
                        }
                        _res.Add(circleSwitch);
                    }
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName() => "proc_GetCircleBlocked";
    }
}
