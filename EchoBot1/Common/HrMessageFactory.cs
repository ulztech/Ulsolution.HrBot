
using Microsoft.Bot.Schema;
using System;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Common
{
    public class HrMessageFactory
    {
        private readonly EmployeeViewModel _emp;
         
        public HrMessageFactory(EmployeeViewModel emp)
        {
            _emp = emp;
        }

        public IMessageActivity ShowDetailedLeaveBalances(string introMessage)
        {
            var vl = _emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.VacationLeave);
            var sl = _emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.SickLeave);

            var messageText = $"{introMessage}\nVL balance: {vl.LeaveBalance - vl.FiledLeave} [ Pending (for approval): {vl.FiledLeave} ]" +
                                $"\nSL balance: {sl.LeaveBalance - sl.FiledLeave} [ Pending (for approval): {sl.FiledLeave} ]";

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Type = ActivityTypes.Message;
            message.Text = messageText;
            message.Locale = "en-Us";
            message.TextFormat = TextFormatTypes.Plain;

            return message;
        }


        public IMessageActivity ShowLeaveBalances()
        {

            var vl = _emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.VacationLeave);
            var sl = _emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.SickLeave);

            var messageText = $"Here's the list of Leave Types and its available leave balances:" +
                              $"\nVL balance: {vl.LeaveBalance - vl.FiledLeave} " +
                              $"\nSL balance: {sl.LeaveBalance - sl.FiledLeave}";


            IMessageActivity messageActivity = Activity.CreateMessageActivity();
            messageActivity.Type = ActivityTypes.Message;
            messageActivity.Text = messageText;
            messageActivity.Locale = "en-Us";
            messageActivity.TextFormat = TextFormatTypes.Plain;
             
            return messageActivity;
        }

        public IMessageActivity ShowFinalLeaveFile(LeaveAppViewModel leaveApplicaiton)
        {
            var dateFrom = Convert.ToDateTime(leaveApplicaiton.DateFrom);
            var dateTo = Convert.ToDateTime(leaveApplicaiton.DateTo);
            var filedLeave = dateTo - dateFrom;

            var numOfLeave = filedLeave.Days + 1;

            var messageText = $"Awesome! Below are the summary of your Leave Application:" +
                              $"\nEmployee ID: {_emp.EmployeeGuid} " +
                              $"\nLeave Type: {leaveApplicaiton.LeaveTypeId.ToString()} " +
                              $"\nLeave Start: {Convert.ToDateTime(leaveApplicaiton.DateFrom).ToShortDateString()} " +
                              $"\nLeave End: {Convert.ToDateTime(leaveApplicaiton.DateTo).ToShortDateString()} " +
                              $"\nDay Type: XXXXX " +
                              $"\nTotal Days Filed: {numOfLeave} " +
                              $"\nShall we continue and submit this now?";

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Type = ActivityTypes.Message;
            message.Text = messageText;
            message.Locale = "en-Us";
            message.TextFormat = TextFormatTypes.Plain;

            return message;
        }

        public IMessageActivity MessageAfterContinue()
        {
            var messageText = $"Your Leave application has been submitted and is now pending for approval. " +
                $"\nThank you for letting me assist you!";

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Type = ActivityTypes.Message;
            message.Text = messageText;
            message.Locale = "en-Us";
            message.TextFormat = TextFormatTypes.Plain;

            return message;
        }
    }
}
