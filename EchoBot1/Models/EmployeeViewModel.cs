using System;
using System.Collections.Generic;

namespace Ulsolution.HrBot.Models
{
    public class EmployeeViewModel
    { 
        public Guid EmployeeGuid { get; set; }
        public string LoginName { get; set; }
        public string EmployeeName { get; set; }
        public bool PasswordPassed { get; set; }
        public List<EmployeeLeaveViewModel>  Leaves { get; set; }
    }
}
