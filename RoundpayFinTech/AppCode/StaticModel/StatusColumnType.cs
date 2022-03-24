using System;
namespace Fintech.AppCode.StaticModel
{
    public class StatusColumnType
    {
        public int USTATUS = 1;
        public int OTPSTATUS = 2;
        private static Lazy<StatusColumnType> instance = new Lazy<StatusColumnType>(() => new StatusColumnType());
        public static StatusColumnType Get_O
        {
            get
            {
                return instance.Value;
            }
        }
        private StatusColumnType() { }
    }
}
