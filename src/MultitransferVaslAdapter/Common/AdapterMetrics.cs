using Prometheus;

namespace MultitransferVaslAdapter.Common;

public static class AdapterMetrics
{
    public static readonly Counter VaslSuccess =
        Metrics.CreateCounter("vasl_success_total", "Successful Vasl operations",
            new CounterConfiguration { LabelNames = new[] { "operation" } });

    public static readonly Counter VaslRejected =
        Metrics.CreateCounter("vasl_rejected_total", "Rejected Vasl operations",
            new CounterConfiguration { LabelNames = new[] { "operation" } });
}
