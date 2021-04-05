using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Common;
using Ulsolution.HrBot.Models;

namespace Ulsolution.HrBot.Dialogs
{ 
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationState conversationState, UserState userState, EmployeeLeaveServiceDialog employeeLoanServiceDialog, EmployeeLoginDialog employeeDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _conversationState = conversationState;
            _userState = userState;

            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(employeeDialog);
            AddDialog(employeeLoanServiceDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync, 
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());
             
            if (userProfile.UserResponseStatusId == UserResponseStatus.New || 
                userProfile.UserResponseStatusId == UserResponseStatus.InvalidLogin)
            {
                var employeeDetails = new EmployeeViewModel();
                return await stepContext.BeginDialogAsync(nameof(EmployeeLoginDialog), employeeDetails, cancellationToken);
            }
              
            return await stepContext.BeginDialogAsync(nameof(EmployeeLeaveServiceDialog), userProfile, cancellationToken);
        }
         
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            //// the Result here will be null.
            //if (stepContext.Result is BookingDetails result)
            //{
            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.

            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //}

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

    }
}
