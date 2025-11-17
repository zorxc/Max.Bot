using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Configuration;
using Max.Bot.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Max.Bot.Polling;

/// <summary>
/// Default ASP.NET Core controller that receives webhook callbacks from MAX.
/// </summary>
[ApiController]
[Route("api/max/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IUpdatePipeline _pipeline;
    private readonly IUpdateHandler _handler;
    private readonly MaxBotOptions _options;
    private readonly ILogger<WebhookController>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookController"/> class.
    /// </summary>
    public WebhookController(IUpdatePipeline pipeline, IUpdateHandler handler, MaxBotOptions options, ILogger<WebhookController>? logger = null)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming webhook updates.
    /// </summary>
    [HttpPost]
    public virtual async Task<IActionResult> PostAsync([FromBody] Update update, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!ValidatePayloadSize())
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        if (!ValidateSignature())
        {
            return Unauthorized();
        }

        await _pipeline.ProcessWebhookAsync(update, _handler, HttpContext.RequestServices, cancellationToken).ConfigureAwait(false);
        return Ok(new { success = true });
    }

    private bool ValidatePayloadSize()
    {
        if (Request.ContentLength is null)
        {
            return true;
        }

        var maxBytes = _options.Webhook.MaxBodySizeKilobytes * 1024L;
        if (Request.ContentLength > maxBytes)
        {
            _logger?.LogWarning("Webhook payload rejected: {Length} bytes exceeds limit {Limit} bytes.", Request.ContentLength, maxBytes);
            return false;
        }

        return true;
    }

    private bool ValidateSignature()
    {
        var secret = _options.Webhook.SecretToken;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return true;
        }

        if (!Request.Headers.TryGetValue(_options.Webhook.SignatureHeaderName, out var headerValues))
        {
            _logger?.LogWarning("Webhook payload rejected: missing header {HeaderName}.", _options.Webhook.SignatureHeaderName);
            return false;
        }

        var provided = headerValues.ToString();
        if (!SecureEquals(provided, secret))
        {
            _logger?.LogWarning("Webhook payload rejected: signature validation failed.");
            return false;
        }

        return true;
    }

    private static bool SecureEquals(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        if (providedBytes.Length != expectedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}



