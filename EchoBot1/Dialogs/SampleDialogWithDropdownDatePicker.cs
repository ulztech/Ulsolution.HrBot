using Ulsolution.HrBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ulsolution.HrBot.Dialogs
{
    public class SampleDialogWithDropdownDatePicker: ComponentDialog
    {
        protected readonly ILogger Logger;
        private readonly string DlgMainId = "MainDialog";
        private readonly string DlgNameId = "NameDlg";
        private readonly string DlgMobileId = "MobileDlg";
        private readonly string DlgLanguageId = "LanguageListDlg";
        private readonly string DlgDateTimeId = "DateTimeDlg";
        // Dependency injection uses this constructor to instantiate MainDialog
        public SampleDialogWithDropdownDatePicker(ILogger<SampleDialogWithDropdownDatePicker> logger)
            : base(nameof(SampleDialogWithDropdownDatePicker))
        { 
            Logger = logger;
              
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                UserNameAsync,
                GetUserNameAsync,
                MobileNumberAsync,
                SelectLanguageList,
                DateTimeAsync,
            }));

            AddDialog(new TextPrompt(DlgNameId, UserNameValidation));
            AddDialog(new NumberPrompt<int>(DlgMobileId, MobileNumberValidation));
            AddDialog(new ChoicePrompt(DlgLanguageId, ChoiceValidataion));
            AddDialog(new DateTimePrompt(DlgDateTimeId));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
              
        }
         
        private async Task<DialogTurnResult> UserNameAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            var employee = (EmployeeViewModel)stepcontext.Options;
 
                return await stepcontext.PromptAsync(DlgNameId, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Hello !!!, Please enter your name")

                }, cancellationtoken);
     
        }

        private Task<bool> ChoiceValidataion(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private Task<bool> UserNameValidation(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        { 

            return Task.FromResult(true);
        }

        private async Task<bool> MobileNumberValidation(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            if (!promptcontext.Recognized.Succeeded)
            {
                await promptcontext.Context.SendActivityAsync("Hello, Please enter the valid mobile no",
                    cancellationToken: cancellationtoken);

                return false;
            }

            int count = Convert.ToString(promptcontext.Recognized.Value).Length;
            if (count != 10)
            {
                await promptcontext.Context.SendActivityAsync("Hello , you are missing some number !!!",
                    cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> GetUserNameAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            var name = (string)stepcontext.Result;
              
            return await stepcontext.PromptAsync(DlgMobileId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter the mobile No"),
                RetryPrompt = MessageFactory.Text("Enter Valid mobile No")
            }, cancellationtoken);
        }


        private async Task<DialogTurnResult> MobileNumberAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationtoken)
        {
            var mobileNo = stepContext.Result;

            var newMovieList = new List<string> { " Tamil ", " English ", " kaanda " };

            return await stepContext.PromptAsync(DlgLanguageId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the Language"),
                Choices = ChoiceFactory.ToChoices(newMovieList),
                RetryPrompt = MessageFactory.Text("Select from the List"),
                Style = ListStyle.HeroCard 
            }, cancellationtoken);
        }

        private async Task<DialogTurnResult> SelectLanguageList(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;

            return await stepContext.PromptAsync(DlgDateTimeId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the Date")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> DateTimeAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            var datetime = stepcontext.Result;

            return await stepcontext.EndDialogAsync(cancellationToken: cancellationtoken);
        }



    }
}
