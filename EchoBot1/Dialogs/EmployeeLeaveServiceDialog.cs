using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Common;
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
         
        private const string _actionCheckLeaveBalance = "Check Leave Balance";
        private const string _actionApplyLeave = "Apply Leave";
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
        private async Task<DialogTurnResult> LeaveServiceIntro(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;
            var message = "Is there anything else I can assist you with?";
            var LoanActionList = new List<string> { _actionCheckLeaveBalance, _actionApplyLeave, _actionLogOut };

            if (userProfile.IsFirstTimeLogin)
            {
                message = "How may I help you today?";
                userProfile.IsFirstTimeLogin = false;

                // save profile
                var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                await userStateAccessors.SetAsync(stepContext.Context, userProfile, cancellationToken: cancellationToken);
            }
             
            return await stepContext.PromptAsync(_dlgGetMenuOptionId, new PromptOptions()
            {
                Prompt = MessageFactory.Text(message),
                Choices = ChoiceFactory.ToChoices(LoanActionList),
                RetryPrompt = MessageFactory.Text("Select from the List"),
                Style = ListStyle.SuggestedAction
            }, cancellationToken);

        }
         
        private async Task<DialogTurnResult> GetCommandAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options; 
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name); 
            var choice = (FoundChoice)stepContext.Result;
              
            switch (choice.Value)
            {
                case _actionCheckLeaveBalance:
                    var introText = $"Hi {emp.EmployeeName}, here's the list of Leave Types and its available Leave Balances:";

                    var messageFactory = new HrMessageFactory(emp); 
                    await stepContext.Context.SendActivityAsync(messageFactory.ShowDetailedLeaveBalances(introText));
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
