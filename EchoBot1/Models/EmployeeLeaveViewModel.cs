using System;

namespace Ulsolution.HrBot.Models
{
    public class EmployeeLeaveViewModel
    {
        public Guid EmployeeGuid { get; set; }
        public decimal LeaveBalance { get; set; }
        public decimal ApprovedLeave { get; set; }
        public decimal FiledLeave { get; set; }
        public LeaveType LeaveTypeId { get; set; } 
    }

    public enum LeaveType
    {
        VacationLeave = 3100,
        SickLeave = 3101
    }
}
