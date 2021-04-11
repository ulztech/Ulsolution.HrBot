 
namespace Ulsolution.HrBot.Models
{
    public class LeaveAppViewModel
    {   
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public decimal FiledLeave { get; set; }
        public LeaveType LeaveTypeId { get; set; }
        public decimal LeaveBalance { get; set; }
        public LeaveDayType LeaveDayTypeId { get; set; }
    }

    public enum LeaveDayType
    {
        Halfday = 1,
        Wholeday = 2
    }

}
