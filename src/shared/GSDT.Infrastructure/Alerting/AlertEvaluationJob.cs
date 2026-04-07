using System.Net.Http.Json;

namespace GSDT.Infrastructure.Alerting;

/// <summary>
/// Hangfire recurring job — evaluates all enabled AlertRules every minute.
/// For each rule:
///   1. Queries Prometheus HTTP API (instant query) for the metric value.
///   2. If condition is breached → RecordBreach() + notify via configured channel.
///   3. If condition clears → ClearBreach().
/// Saves state changes to AlertingDbContext after each evaluation cycle.
/// Registered as a recurring job in AlertingRegistration.RegisterRecurringJobs().
/// </summary>
public sealed class AlertEvaluationJob(
    AlertingDbContext db,
    IHttpClientFactory httpClientFactory,
    WebhookUrlValidator urlValidator,
    IAuditLogger auditLogger,
    ILogger<AlertEvaluationJob> logger)
{
    private const string PrometheusClientName = "prometheus";

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var rules = await db.AlertRules
            .Where(r => r.Enabled)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            logger.LogDebug("AlertEvaluationJob: no enabled rules — skipping");
            return;
        }

        logger.LogInformation("AlertEvaluationJob: evaluating {Count} rule(s)", rules.Count);

        var httpClient = httpClientFactory.CreateClient(PrometheusClientName);

        foreach (var rule in rules)
        {
            try
            {
                await EvaluateRuleAsync(rule, httpClient, cancellationToken);
            }
            catch (Exception ex)
            {
                // Do not let one rule failure abort the entire job cycle
                logger.LogError(ex, "AlertEvaluationJob: error evaluating rule {RuleId} ({Name})",
                    rule.Id, rule.Name);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------

    private async Task EvaluateRuleAsync(
        AlertRule rule,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var metricValue = await QueryPrometheusAsync(rule.MetricName, httpClient, cancellationToken);

        if (metricValue is null)
        {
            logger.LogWarning(
                "AlertEvaluationJob: no data for metric '{Metric}' (rule '{Name}')",
                rule.MetricName, rule.Name);
            return;
        }

        if (rule.IsBreached(metricValue.Value))
        {
            rule.RecordBreach();

            logger.LogWarning(
                "Alert breached: {Name} | metric={Metric} value={Value} condition={Cond} threshold={Threshold} breaches={N}",
                rule.Name, rule.MetricName, metricValue.Value,
                rule.Condition, rule.Threshold, rule.ConsecutiveBreaches);

            await auditLogger.LogCommandAsync(
                action: "AlertBreached",
                moduleName: "Alerting",
                resourceType: "AlertRule",
                resourceId: rule.Id.ToString(),
                cancellationToken: cancellationToken);

            await SendNotificationAsync(rule, metricValue.Value);
        }
        else
        {
            if (rule.ConsecutiveBreaches > 0)
            {
                rule.ClearBreach();
                logger.LogInformation(
                    "Alert cleared: {Name} | metric={Metric} value={Value}",
                    rule.Name, rule.MetricName, metricValue.Value);
            }
        }
    }

    /// <summary>
    /// Queries Prometheus instant query API: GET /api/v1/query?query={metric}
    /// Returns the first scalar/vector value, or null if no data.
    /// </summary>
    private async Task<double?> QueryPrometheusAsync(
        string metricName,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"api/v1/query?query={Uri.EscapeDataString(metricName)}";
            var response = await httpClient.GetFromJsonAsync<PrometheusQueryResponse>(
                url, cancellationToken);

            if (response?.Status != "success") return null;

            // result[0].value[1] is the string-formatted metric value for instant queries
            var firstResult = response.Data?.Result?.FirstOrDefault();
            if (firstResult?.Value is { Count: >= 2 })
            {
                var rawValue = firstResult.Value[1]?.ToString();
                if (double.TryParse(rawValue,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsed))
                    return parsed;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to query Prometheus for metric '{Metric}'", metricName);
            return null;
        }
    }

    private async Task SendNotificationAsync(AlertRule rule, double metricValue)
    {
        var message = $"[ALERT] {rule.Name}: {rule.MetricName} = {metricValue} " +
                      $"({rule.Condition} {rule.Threshold}) — breach #{rule.ConsecutiveBreaches}";

        switch (rule.NotifyChannel.ToUpperInvariant())
        {
            case "LOG":
                // Already logged above — explicit Log channel just ensures it is always written
                logger.LogCritical("AlertNotification: {Message}", message);
                break;

            case "WEBHOOK":
                if (!string.IsNullOrEmpty(rule.NotifyTarget))
                    await PostWebhookAsync(rule.NotifyTarget, message);
                break;

            case "EMAIL":
                // Email delivery delegated to notification module (future integration point)
                logger.LogWarning(
                    "AlertNotification EMAIL channel not yet wired — target={Target} msg={Msg}",
                    rule.NotifyTarget, message);
                break;

            default:
                logger.LogWarning(
                    "AlertNotification: unknown channel '{Channel}' for rule {Name}",
                    rule.NotifyChannel, rule.Name);
                break;
        }
    }

    private async Task PostWebhookAsync(string url, string message)
    {
        // SSRF validation before outgoing request (fix H2-BE-Sec)
        var check = await urlValidator.ValidateAsync(url);
        if (check.IsFailed)
        {
            logger.LogWarning(
                "Alert webhook SSRF blocked: {Url} — {Reasons}",
                url, string.Join("; ", check.Errors.Select(e => e.Message)));
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var payload = new { text = message, timestamp = DateTime.UtcNow };
            using var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("Alert webhook delivery failed: {Status} {Url}", response.StatusCode, url);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Alert webhook post threw for url {Url}", url);
        }
    }
}
