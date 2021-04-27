using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Common;
using Ulsolution.HrBot.Data;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Dialogs
{
    public class EmployeeApplyLeaveDialog : ComponentDialog
    {
        protected readonly ILogger Logger; 
        private readonly string _dlgNumber = "_dlgNumberId";
        private readonly string _dlgDateFrom = "_dlgDateFromId";
        private readonly string _dlgDateTo = "_dlgDateToId";
        private readonly string _dlgGetMenuOption = "_dlgGetMenuOptionId";
        private const string RepromptMsgText = "That's an invalid date format \nplease enter a date including Day Month and Year.";

        public EmployeeApplyLeaveDialog(ILogger<EmployeeApplyLeaveDialog> logger) : base(nameof(EmployeeApplyLeaveDialog))
        {
            Logger = logger;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LeaveServiceIntro,
                EnterDateFromAsync,
                EnterDateToAsync,
                SelectLeaveTypeAsync,
                ConfirmResultToAsync,
                FinalResultToAsync
                //UserNameAsync,
                //GetUserNameAsync,
                //MobileNumberAsync,
                //SelectLanguageList,
                //DateTimeAsync,
            }));

            //AddDialog(new TextPrompt(DlgNameId, UserNameValidation));
            AddDialog(new NumberPrompt<int>(_dlgNumber));
            //AddDialog(new ChoicePrompt(DlgLanguageId, ChoiceValidataion));
            AddDialog(new ChoicePrompt(_dlgGetMenuOption));
            AddDialog(new DateTimePrompt(_dlgDateFrom, DateTimePromptValidator));
            AddDialog(new DateTimePrompt(_dlgDateTo, DateTimePromptValidator));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog); 
        }

        private async Task<DialogTurnResult> LeaveServiceIntro(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
            var userProfile = (UserProfile)stepContext.Options; 
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name);

            var LoanActionList = new List<string>();

            emp.Leaves.ForEach(m =>
            {
                var enumName = (LeaveType)m.LeaveTypeId;
                LoanActionList.Add(enumName.ToString());
            });

            LoanActionList.Add("Cancel");

            var messageFactory = new HrMessageFactory(emp);
            await stepContext.Context.SendActivityAsync(messageFactory.ShowLeaveBalances());

            return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the type of leave you want to apply."),
                Choices = ChoiceFactory.ToChoices(LoanActionList),
                RetryPrompt = MessageFactory.Text("Select from the List"),
                Style = ListStyle.SuggestedAction
            }, cancellationtoken);

        }

        private async Task<DialogTurnResult> EnterDateFromAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        { 
            var result = (FoundChoice)stepContext.Result;

            // check if cancelled
            if (result.Value.Contains("Cancel"))
            {
                return await stepContext.EndDialogAsync(null, cancellationtoken);
            }

            var userProfile = (UserProfile)stepContext.Options;

            if (userProfile.FiledLeave == null)
            {
                userProfile.FiledLeave = new LeaveAppViewModel();
            }

            var selected = (LeaveType)System.Enum.Parse(typeof(LeaveType), result.Value);
             
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name);
            var leaveBalance = emp.Leaves.Find(m => m.LeaveTypeId == selected);

            userProfile.FiledLeave.LeaveTypeId = selected;
            userProfile.FiledLeave.LeaveBalance = leaveBalance.LeaveBalance;

            var promptMessage = MessageFactory.Text("When is the Leave start date? (ex Jan 1 2021)");
            var repromptMessage = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(_dlgDateFrom, new PromptOptions
            {
                Prompt = promptMessage,
                RetryPrompt = repromptMessage
            }, cancellationtoken);
             
        }
        private async Task<DialogTurnResult> EnterDateToAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
           var result = (IList<DateTimeResolution>)stepContext.Result;
            
            var userProfile = (UserProfile)stepContext.Options;

            if (userProfile.FiledLeave != null && result.Count > 0)
            {
                userProfile.FiledLeave.DateFrom = result[0].Value;
            }
             
            var promptMessage = MessageFactory.Text("When is the Leave end date? (ex Jan 1 2021)"); 
            var repromptMessage = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(_dlgDateTo, new PromptOptions
            {
                Prompt = promptMessage,
                RetryPrompt = repromptMessage
            }, cancellationtoken);
        }

        private async Task<DialogTurnResult> SelectLeaveTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
            var result = (IList<DateTimeResolution>)stepContext.Result;
            var userProfile = (UserProfile)stepContext.Options;
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name);

            if (userProfile.FiledLeave != null && result.Count > 0)
                userProfile.FiledLeave.DateTo = result[0].Value;

            var validate = Compute(userProfile);
            var resultValue = validate.Value;
            var messageText = resultValue.Value;

            if (!validate.Key)
            {
                var LoanActionList = new List<string>() { "Yes", "Cancel" };

                return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
                {
                    Prompt = MessageFactory.Text(messageText),
                    Choices = ChoiceFactory.ToChoices(LoanActionList),
                    RetryPrompt = MessageFactory.Text("Select from the List"),
                    Style = ListStyle.SuggestedAction
                }, cancellationtoken);
            }
             
            if (resultValue.Key == 1)
            {
                var dayTypeList = new List<string>() { "Halfday", "Wholeday" };

                //var messageFactory = new HrMessageFactory(emp);
                //await stepContext.Context.SendActivityAsync(messageFactory.ShowLeaveBalances());

                return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Please select the day type."),
                    Choices = ChoiceFactory.ToChoices(dayTypeList),
                    RetryPrompt = MessageFactory.Text("Please select the day type."),
                    Style = ListStyle.SuggestedAction
                }, cancellationtoken);
            }

            return await stepContext.NextAsync(-1, cancellationtoken);
        }
         
        private async Task<DialogTurnResult> ConfirmResultToAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {  
            var userProfile = (UserProfile)stepContext.Options; 
            var emp = await EmployeeFiles.GetEmployee(userProfile.Name);

            if (int.TryParse(stepContext.Result.ToString(), out int id))
            {
                if (id < 0)
                    userProfile.FiledLeave.LeaveDayTypeId = LeaveDayType.Wholeday;
            }
            else
            {
                var result = (FoundChoice)stepContext.Result;
                var selected = (LeaveDayType)System.Enum.Parse(typeof(LeaveDayType), result.Value);
                userProfile.FiledLeave.LeaveDayTypeId = selected;
            }

            if (userProfile.FiledLeave.LeaveDayTypeId == LeaveDayType.Halfday)
                userProfile.FiledLeave.FiledLeave = 0.5m;

            var messageFactory = new HrMessageFactory(emp);
            var messageActivity = messageFactory.ShowFinalLeaveFile(userProfile.FiledLeave);
            await stepContext.Context.SendActivityAsync(messageActivity);

            var LoanActionList = new List<string>() { "Continue", "Cancel" };

            return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please click 'Continue' to confirm."),
                Choices = ChoiceFactory.ToChoices(LoanActionList),
                RetryPrompt = MessageFactory.Text("Please click 'Continue' to confirm or click 'Cancel' to undo your changes."),
                Style = ListStyle.SuggestedAction
            }, cancellationtoken); 
             


            //if (userProfile.FiledLeave != null && result.Count > 0)
            //    userProfile.FiledLeave.DateTo = result[0].Value;



            //var result = (IList<DateTimeResolution>)stepContext.Result;

            //var userProfile = (UserProfile)stepContext.Options;

            //if (userProfile.FiledLeave != null && result.Count > 0) 
            //    userProfile.FiledLeave.DateTo = result[0].Value; 

            //var validate = Compute(userProfile);
            //var messageText = validate.Value;


            //if (validate.Key)
            //{ 
            //    var emp = await EmployeeFiles.GetEmployee(userProfile.Name);
            //    var messageFactory = new HrMessageFactory(emp);
            //    var messageActivity = messageFactory.ShowFinalLeaveFile(userProfile.FiledLeave);
            //    await stepContext.Context.SendActivityAsync(messageActivity);

            //    var LoanActionList = new List<string>() { "Continue", "Cancel" };

            //    return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
            //    {
            //        Prompt = MessageFactory.Text("Please click 'Continue' to confirm."),
            //        Choices = ChoiceFactory.ToChoices(LoanActionList),
            //        RetryPrompt = MessageFactory.Text("Select from the List"),
            //        Style = ListStyle.SuggestedAction
            //    }, cancellationtoken);
            //}
            //else
            //{
            //    var LoanActionList = new List<string>() { "Yes", "Cancel" };

            //    return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
            //    {
            //        Prompt = MessageFactory.Text(messageText),
            //        Choices = ChoiceFactory.ToChoices(LoanActionList),
            //        RetryPrompt = MessageFactory.Text("Select from the List"),
            //        Style = ListStyle.SuggestedAction
            //    }, cancellationtoken);
            //}

        }

        private async Task<DialogTurnResult> FinalResultToAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
            var result = (FoundChoice)stepContext.Result; 
            var userProfile = (UserProfile)stepContext.Options;

            // check if cancelled
            if (result.Value.Contains("Yes"))
            {
                return await stepContext.BeginDialogAsync(nameof(EmployeeApplyLeaveDialog), userProfile, cancellationtoken);
            }
            else if (result.Value.Contains("Continue"))
            {
                var emp = await EmployeeFiles.GetEmployee(userProfile.Name);
                var messageFactory = new HrMessageFactory(emp);
                var messageActivity = messageFactory.MessageAfterContinue();
                await stepContext.Context.SendActivityAsync(messageActivity);

                return await stepContext.EndDialogAsync(null, cancellationtoken);
            }
            else
                return await stepContext.EndDialogAsync(null, cancellationtoken);

             
        }
          
        private KeyValuePair<bool, KeyValuePair<int, string>> Compute(UserProfile userProfile)
        {
            var result = new KeyValuePair<int, string>();
            var isSuccess = false;
            var message = string.Empty;
            var dateFrom = Convert.ToDateTime(userProfile.FiledLeave.DateFrom);
            var dateTo = Convert.ToDateTime(userProfile.FiledLeave.DateTo);
            var num = dateTo - dateFrom;
            var numOfLeave = num.Days + 1;
            //var filedLeave = Enum.GetName(typeof(LeaveType), userProfile.FiledLeave.LeaveTypeId);

            if (dateTo < dateFrom)
                message = "Sorry, you have entered an invalid date or out of scope. Do you want to repeat the steps?";

            var bal = userProfile.FiledLeave.LeaveBalance - userProfile.FiledLeave.FiledLeave;
            if (bal < numOfLeave)
            {
                message = $"It seems that you don't have enough leave balance to proceed with the next steps. Can you apply using other leave type?";
            }

            if (string.IsNullOrEmpty(message)) 
                isSuccess = true;

            result = new KeyValuePair<int, string>(numOfLeave, message);

            return new KeyValuePair<bool, KeyValuePair<int, string>>(isSuccess, result);
        }
         
        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }
            promptContext.Context.SendActivityAsync(RepromptMsgText, cancellationToken: cancellationToken);
            return Task.FromResult(false);
        }

    }
}
