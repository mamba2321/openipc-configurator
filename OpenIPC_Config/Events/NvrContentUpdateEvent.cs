using Prism.Events;

namespace OpenIPC_Config.Events;

public class NvrContentUpdateChangeEvent : PubSubEvent<NvrContentUpdatedMessage>
{
}

public class NvrContentUpdatedMessage
{
    public string VdecContent { get; set; }
    public string WfbConfContent { get; set; }
    public string GsKeyContent { get; set; }
    public string TelemetryConfContent { get; set; }
}