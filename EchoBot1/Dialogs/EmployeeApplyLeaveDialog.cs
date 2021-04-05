using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Data;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Dialogs
{
    public class EmployeeApplyLeaveDialog : ComponentDialog
    {
        protected readonly ILogger Logger; 
        private readonly string _dlgNumber = "_dlgNumberId";
        private readonly string _dlgDate = "_dlgDateId";
        private readonly string _dlgGetMenuOption = "_dlgGetMenuOptionId";
        private const string RepromptMsgText = "I'm sorry, to make your booking please enter a date including Day Month and Year.";

        public EmployeeApplyLeaveDialog(ILogger<EmployeeApplyLeaveDialog> logger) : base(nameof(EmployeeApplyLeaveDialog))
        {
            Logger = logger;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LeaveServiceIntro,
                EnterDateFromAsync,
                EnterDateToAsync
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
            AddDialog(new DateTimePrompt(_dlgDate, DateTimePromptValidator));

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

            return await stepContext.PromptAsync(_dlgGetMenuOption, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the type of leave that you want to apply?"),
                Choices = ChoiceFactory.ToChoices(LoanActionList),
                RetryPrompt = MessageFactory.Text("Select from the List"),
                Style = ListStyle.SuggestedAction
            }, cancellationtoken);

        }

        private async Task<DialogTurnResult> EnterDateFromAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        {
           var userProfile = (UserProfile)stepContext.Options;

            if (userProfile.FiledLeave == null)
            {
                userProfile.FiledLeave = new LeaveAppViewModel();
            }

            var result = (FoundChoice)stepContext.Result;
            var selected = (LeaveType)System.Enum.Parse(typeof(LeaveType), result.Value);
            userProfile.FiledLeave.LeaveTypeId = selected;
              
            var promptMessage = MessageFactory.Text("Enter date from");
            var repromptMessage = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(_dlgDate, new PromptOptions
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
             
            var promptMessage = MessageFactory.Text("Enter date to");

            return await stepContext.PromptAsync(_dlgDate, new PromptOptions
            {
                Prompt = promptMessage
            }, cancellationtoken);
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
