 
namespace Ulsolution.HrBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; } 
        public int? Age { get; set; } 
        public string Date { get; set; }
        public bool IsLoginCompleted { get; set; } = false;
        public UserResponseStatus UserResponseStatusId { get; set; } = UserResponseStatus.New;
        public LeaveAppViewModel FiledLeave { get; set; }
        public bool IsFirstTimeLogin { get; set; } = true;
    }

    public enum UserResponseStatus
    {
        New,
        InvalidLogin,
        ValidLogin,
        CheckBalance,
        ApplyLeave,
        LogOut
    }
}
