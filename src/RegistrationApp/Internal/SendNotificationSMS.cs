namespace RegistrationApp.Internal
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Models;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public static class SendNotificationSMS
    {
        [FunctionName("SendNotificationSMS")]
        public static void Run(
            [QueueTrigger("tosendnotification", Connection = "registrationstorage_STORAGE")] Customer customer,
            [TwilioSms(
                From = "ENTER_FROM_PHONE_NUMBER - PROVIDED BY TWILIO",
                Body = "New customer {Name} {Surname}!")]
            out CreateMessageOptions messageOptions,
            ILogger log)
        {
            log.LogInformation($"SendNotificationSMS function processed: {customer.Name} {customer.Surname}");
            messageOptions = new CreateMessageOptions(new PhoneNumber("ENTER_YOUR_PHONE_NUMBER"));
        }
    }
}
