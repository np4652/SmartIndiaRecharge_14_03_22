using Fintech.AppCode.Model;
using System;
using Validators;

namespace Fintech.AppCode.HelperClass
{
    public class ConnectionStringHelper
    {
        private static Lazy<ConnectionStringHelper> Instatnce = new Lazy<ConnectionStringHelper>(() => new ConnectionStringHelper());
        public static ConnectionStringHelper O { get { return Instatnce.Value; } }
        private ConnectionStringHelper() { }
        public string ConvertTransactionIDTo_dd_MMM_yyyy(string TransactionID)
        {
            string __yy = DateTime.Now.ToString("yyyy").Substring(0, 2);
            TransactionID = TransactionID.Substring(1, 6);
            return TransactionID.Substring(4, 2) + " " + Validate.O.Months[Convert.ToInt32(TransactionID.Substring(2, 2)) - 1] + " " + __yy + TransactionID.Substring(0, 2);
        }

        public TypeMonthYear GetTypeMonthYear(DateTime _dateTime)
        {
            TypeMonthYear typeMonthYear = new TypeMonthYear();
            try
            {
                DateTime CurrentDate = DateTime.Now;
                if (_dateTime != null)
                {
                    if (_dateTime.Date >= DateTime.Now.Date)
                    {
                        typeMonthYear.ConType = ConnectionStringType.DBCon;
                    }
                    else if (_dateTime.Year == CurrentDate.Year)
                    {
                        if (_dateTime.Month == CurrentDate.Month)
                        {
                            typeMonthYear.ConType = ConnectionStringType.DBCon_Month;
                        }
                        else if (_dateTime.Month < CurrentDate.Month)
                        {
                            typeMonthYear.ConType = ConnectionStringType.DBCon_Old;
                            typeMonthYear.MM = _dateTime.ToString("MM");
                            typeMonthYear.YYYY = _dateTime.ToString("yyyy");
                        }
                    }
                    else if (_dateTime.Year < CurrentDate.Year)
                    {
                        typeMonthYear.ConType = ConnectionStringType.DBCon_Old;
                        typeMonthYear.MM = _dateTime.ToString("MM");
                        typeMonthYear.YYYY = _dateTime.ToString("yyyy");
                    }
                }
            }
            catch (Exception) { }
            return typeMonthYear;
        }
    }
}
