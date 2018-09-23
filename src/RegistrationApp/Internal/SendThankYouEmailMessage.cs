namespace RegistrationApp.Internal
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Models;
    using SendGrid.Helpers.Mail;

    public static class SendThankYouEmailMessage
    {
        [FunctionName("SendThankYouEmailMessage")]
        public static void Run(
            [QueueTrigger("tosendemail", Connection = "registrationstorage_STORAGE")] Customer customer,
            [SendGrid(
                To = "{Email}",
                Subject = "Thank you!",
                Text = "Hi {Name}, Thank you for registering!!!!",
                From = "ENTER_FROM_EMAIL_ADDRESS"
            )]
            out SendGridMessage message,
            ILogger log)
        {
            log.LogInformation($"SendThankYouEmailMessage function processed: {customer.Name} {customer.Surname}");
            message = new SendGridMessage();
        }
    }
}
