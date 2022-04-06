using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace Service.Controllers;

public class SmsController : TwilioController
{
    private readonly ILogger<SmsController> logger;

    public SmsController(ILogger<SmsController> logger)
    {
        this.logger = logger;
    }

    public IActionResult Index(SmsRequest smsRequest)
    {
        logger.LogInformation("SMS Received: {SmsId}", smsRequest.SmsSid);
        var response = new MessagingResponse();
        response.Message($"You sent: {smsRequest.Body}");
        return TwiML(response);
    }
}
