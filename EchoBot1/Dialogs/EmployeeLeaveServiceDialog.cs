using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Data;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Dialogs
{
    public class EmployeeLeaveServiceDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
         
        private readonly string _dlgGetMenuOptionId = "GetMenuOptionId";
         
        private const string _actionCheckLeaveBalance = "Check leave balance";
        private const string _actionApplyLeave = "Apply new leave";
        private const string _actionLogOut = "Log Out";
        public EmployeeLeaveServiceDialog(ConversationState conversationState, UserState userState, EmployeeApplyLeaveDialog employeeApplyLoan, ILogger<EmployeeLeaveServiceDialog> logger) : base(nameof(EmployeeLeaveServiceDialog))
        {
            _conversationState = conversationState;
            _userState = userState;

            Logger = logger;

            AddDialog(employeeApplyLoan);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt))); 
            AddDialog(new ChoicePrompt(_dlgGetMenuOptionId, ChoiceValidataion));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LeaveServiceIntro, 
                GetCommandAsync, 
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }
        private async Task<DialogTurnResult> LeaveServiceIntro(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
            var userProfile = (UserProfile)stepContext.Options;

            var LoanActionList = new List<string> { _actionCheckLeaveBalance, _actionApplyLeave, _actionLogOut };
             
            return await stepContext.PromptAsync(_dlgGetMenuOptionId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("What can I do for you?"),
                Choices = ChoiceFactory.ToChoices(LoanActionList),
                RetryPrompt = MessageFactory.Text("Select from the List"),
                Style = ListStyle.SuggestedAction
            }, cancellationtoken);

        }
         
        private async Task<DialogTurnResult> GetCommandAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options; 
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name); 
            var choice = (FoundChoice)stepContext.Result;
              
            switch (choice.Value)
            {
                case _actionCheckLeaveBalance:
                    await stepContext.Context.SendActivityAsync($"Hi {emp.EmployeeName}, here's your leave balances for this year:");

                    var vl = emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.VacationLeave);
                    var sl = emp.Leaves.Find(m => m.LeaveTypeId == LeaveType.SickLeave);

                    var messageText = $"VL balance: {vl.LeaveBalance} " +
                                        $"\nPending (for approval): {vl.FiledLeave}" +
                                        $"\nSL balance: {sl.LeaveBalance} " +
                                        $"\nPending (for approval): {sl.FiledLeave}";

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.Type = ActivityTypes.Message;
                    message.Text = messageText;
                    message.Locale = "en-Us";
                    message.TextFormat = TextFormatTypes.Plain;
                    await stepContext.Context.SendActivityAsync(message);
                    break;
                case _actionApplyLeave: 
                    return await stepContext.BeginDialogAsync(nameof(EmployeeApplyLeaveDialog), userProfile, cancellationToken); 
                case _actionLogOut:

                    await stepContext.Context.SendActivityAsync($"You are now signed out. Thank you {emp.EmployeeName}!"); 
                    var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                    userProfile = new UserProfile();

                    await userStateAccessors.SetAsync(stepContext.Context, userProfile, cancellationToken: cancellationToken); 
                    return await stepContext.EndDialogAsync(null, cancellationToken); 
                default: 
                    break;
            }
                
             
            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //await stepContext.Context.SendActivityAsync($"Welcome back {searchEmp.EmployeeName}!!!", cancellationToken: cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private Task<bool> ChoiceValidataion(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

       

    }


}
