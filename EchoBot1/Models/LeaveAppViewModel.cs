 
namespace Ulsolution.HrBot.Models
{
    public class LeaveAppViewModel
    {   
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public decimal FiledLeave { get; set; }
        public LeaveType LeaveTypeId { get; set; }
        public decimal LeaveBalance { get; set; }
    }
}
