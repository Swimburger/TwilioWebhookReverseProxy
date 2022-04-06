using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ReverseProxy.Controllers;

public class SmsController : Controller
{
    private static readonly HttpClient HttpClient;
    private readonly ILogger<SmsController> logger;
    private readonly string twilioWebhookServiceUrl;

    static SmsController()
    {
        // don't do this in production!
        var insecureHttpClientHandler = new HttpClientHandler();
        insecureHttpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
        HttpClient = new HttpClient(insecureHttpClientHandler);
    }

    public SmsController(ILogger<SmsController> logger, IConfiguration configuration)
    {
        this.logger = logger;
        twilioWebhookServiceUrl = configuration["TwilioWebhookServiceUrl"];
    }

    public async Task Index()
    {
        using var serviceRequest = new HttpRequestMessage(HttpMethod.Post, twilioWebhookServiceUrl);
        foreach (var header in Request.Headers)
        {
            serviceRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
        
        serviceRequest.Content = new FormUrlEncodedContent(
            Request.Form.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToString()
            )
        );
        
        var serviceResponse = await HttpClient.SendAsync(serviceRequest);

        Response.ContentType = "application/xml";
        var headersDenyList = new HashSet<string>()
        {
            "Content-Length",
            "Date",
            "Transfer-Encoding"
        };
        foreach (var header in serviceResponse.Headers)
        {
            if(headersDenyList.Contains(header.Key)) continue;
            logger.LogInformation("Header: {Header}, Value: {Value}", header.Key, string.Join(',', header.Value));
            Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
        }

        await serviceResponse.Content.CopyToAsync(Response.Body);
    }
}
