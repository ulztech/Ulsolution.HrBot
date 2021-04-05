using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Data
{
    public class EmployeeFiles
    {
        public static async Task<bool> CheckLogin(string loginName, string password)
        {
            var result = GetEmployee().Where(m => m.LoginName.ToLower() == loginName.ToLower() && password == "1234").Count() > 0;

            return await Task.FromResult(result);
        }

        public static async Task<EmployeeViewModel> GetEmployee(string loginName)
        {
            var result = GetEmployee().Where(m => m.LoginName.ToLower() == loginName.ToLower()).FirstOrDefault(); 
            return await Task.FromResult(result);
        }

        private static List<EmployeeViewModel> GetEmployee()
        {
            var list = new List<EmployeeViewModel>();

            var guidKey = Guid.NewGuid();
            list.Add(new EmployeeViewModel()
            {
                EmployeeGuid = guidKey,
                LoginName = "User1",
                EmployeeName = "James Blake",
                Leaves = new List<EmployeeLeaveViewModel>
                {
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 0, EmployeeGuid = guidKey, FiledLeave = 0, LeaveBalance = 7, LeaveTypeId = LeaveType.VacationLeave }, 
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 0, EmployeeGuid = guidKey, FiledLeave = 0, LeaveBalance = 7, LeaveTypeId = LeaveType.SickLeave }
                }
            });

            guidKey = Guid.NewGuid();
            list.Add(new EmployeeViewModel()
            {
                EmployeeGuid = guidKey,
                LoginName = "User2",
                EmployeeName = "Mitzi Sester",
                Leaves = new List<EmployeeLeaveViewModel>
                {
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 1, EmployeeGuid = guidKey, FiledLeave = 3, LeaveBalance = 7, LeaveTypeId = LeaveType.VacationLeave },
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 0, EmployeeGuid = guidKey, FiledLeave = 2, LeaveBalance = 7, LeaveTypeId = LeaveType.SickLeave }
                }
            });

            guidKey = Guid.NewGuid();
            list.Add(new EmployeeViewModel()
            {
                EmployeeGuid = guidKey,
                LoginName = "User3",
                EmployeeName = "Boyce Schlosser",
                Leaves = new List<EmployeeLeaveViewModel>
                {
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 7, EmployeeGuid = guidKey, FiledLeave = 3, LeaveBalance = 7, LeaveTypeId = LeaveType.VacationLeave },
                    new EmployeeLeaveViewModel(){ ApprovedLeave = 5, EmployeeGuid = guidKey, FiledLeave = 2, LeaveBalance = 7, LeaveTypeId = LeaveType.SickLeave }
                }
            });


            return list;
        }
    }
}
