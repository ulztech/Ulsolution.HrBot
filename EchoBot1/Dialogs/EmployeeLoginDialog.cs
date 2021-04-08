using Ulsolution.HrBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs; 
using Microsoft.Extensions.Logging; 
using System.Threading;
using System.Threading.Tasks;
using Ulsolution.HrBot.Data;
using Microsoft.Bot.Schema; 
using Ulsolution.HrBot.Common;

namespace Ulsolution.HrBot.Dialogs
{ 
    public class EmployeeLoginDialog: ComponentDialog
    {
        private const string PromptLoginMsgText = "Please key in your Employee ID.";
        private const string RepromptLoginMsgText = "I'm sorry, that's an invalid Employee ID/Password.\nPlease re-enter your Employee ID.";
          
        private readonly string _dlgUserNameId = "UserNameDialog";
        private readonly string _dlgUserPasswordId = "UserPasswordDialog";

        protected readonly ILogger Logger;

        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        public EmployeeLoginDialog(ConversationState conversationState, UserState userState, ILogger<EmployeeLoginDialog> logger) : base(nameof(EmployeeLoginDialog))
        {
            _conversationState = conversationState;
            _userState = userState;

            Logger = logger;

            AddDialog(new TextPrompt(_dlgUserNameId));
            AddDialog(new TextPrompt(_dlgUserPasswordId ));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt))); 
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                UserNameAsync,
                PasswordAsync, 
                FinalStepAsync 
            }));
              
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
             
        }

        private async Task<DialogTurnResult> UserNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        { 
            stepContext.Values[GlobalKeys.UserInfoKey] = new UserProfile();

            var employee = (EmployeeViewModel)stepContext.Options; 
            if (employee.LoginName == null)
            {
                var promptMessage = Microsoft.Bot.Builder.MessageFactory.Text(PromptLoginMsgText, PromptLoginMsgText, InputHints.ExpectingInput);

                return await stepContext.PromptAsync(_dlgUserNameId, new PromptOptions
                {
                    Prompt = promptMessage

                }, cancellationtoken); 
            }
            employee.LoginName = (string)stepContext.Result;
            return await stepContext.NextAsync(employee, cancellationtoken); 
        }
         
        private async Task<DialogTurnResult> PasswordAsync(WaterfallStepContext stepContext, CancellationToken cancellationtoken)
        { 
            var employee = (EmployeeViewModel)stepContext.Options; 
            var searchEmp = EmployeeFiles.GetEmployee(stepContext.Result.ToString());
              
            if (string.IsNullOrEmpty(employee.LoginName) && stepContext.Result.ToString().Length > 3)
            {  
                employee.LoginName = stepContext.Result.ToString();
            } 
             
            return await stepContext.PromptAsync(_dlgUserPasswordId, new PromptOptions
            {
                Prompt = Microsoft.Bot.Builder.MessageFactory.Text($"Please enter your passkey.")
            }, cancellationtoken);
                   
        }
         
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var employee = (EmployeeViewModel)stepContext.Options;
            var searchEmp = await EmployeeFiles.GetEmployee(employee.LoginName);

            var passkey = (string)stepContext.Result;

            // check user password 
            if (searchEmp != null && !string.IsNullOrEmpty(passkey))
            {
                var passwordOk =  await EmployeeFiles.CheckLogin(employee.LoginName, passkey);
                if (passwordOk)
                {

                    var userProfile = (UserProfile)stepContext.Values[GlobalKeys.UserInfoKey];
                    userProfile.Name = searchEmp.LoginName;
                    userProfile.UserResponseStatusId = Models.UserResponseStatus.ValidLogin;

                    var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                    await userStateAccessors.SetAsync(stepContext.Context,  userProfile, cancellationToken: cancellationToken);
                
                     
                    await stepContext.Context.SendActivityAsync($"Welcome back {searchEmp.EmployeeName}!", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(userProfile, cancellationToken);
                }  
            }

            await stepContext.Context.SendActivityAsync("Sorry, invalid username and password.", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    } 
}
