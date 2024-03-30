using SV20T1020580.DataLayers;
using SV20T1020580.DataLayers.SQLServer;
using SV20T1020580.DomainModels;
using System.Runtime.CompilerServices;

namespace SV20T1020580.BusinessLayers
{
    
    public static  class UserAccountService
    {
        private static readonly IUserAccountDAL employeeAcountDB;
        /// <summary>
        /// Ctor
        /// </summary>
        static UserAccountService()
        {
            employeeAcountDB = new EmployeeAccountDAL(Configuration.ConnectionString);
        }
        public static UserAccount? Authorize(string userName, string password)
        {
            //TODO: Kiểm tra thông tin đăng nhập của Employee
            return employeeAcountDB.Authorize(userName, password);
            
        }
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return employeeAcountDB.ChangePassword(userName, oldPassword, newPassword);
            //TODO: Thay đổi mật khẩu của Employee

        }
    }
}
