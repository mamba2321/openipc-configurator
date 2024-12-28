using System;
using System.Collections.Generic;
using System.IO;

namespace OpenIPC_Config.Parsers;

public class VdecConfParser : ConfigParserBase<VdecConfParser>
{
    public int Port { get; set; } = 5000;
    public string Codec { get; set; } = "h264";
    public string Format { get; set; } = "stream";
    public string Mode { get; set; } = "720p60";
    public int MavlinkPort { get; set; } = 14750;
    public string Osd { get; set; } = "simple";
    public string Records { get; set; } = string.Empty;
    public string Extra { get; set; } = string.Empty;
    public string OsdElements { get; set; } = string.Empty;

    protected override void MapKeyToProperty(string key, string value)
    {
        switch (key)
        {
            case "port": Port = int.Parse(value); break;
            case "codec": Codec = value; break;
            case "format": Format = value; break;
            case "mode": Mode = value; break;
            case "mavlink_port": MavlinkPort = int.Parse(value); break;
            case "osd": Osd = value; break;
            case "records": Records = value; break;
            case "extra": Extra = value.Trim('"'); break;
            case "osd_elements": OsdElements = value.Trim('"'); break;
        }
    }

    protected override IEnumerable<(string Key, string Value, string Comment)> GetConfigProperties()
    {
        return new List<(string, string, string)>
        {
            ("port", Port.ToString(), "Port for video rx (default: 5000)"),
            ("codec", Codec, "Codec: h264 or h265 (default: h264)"),
            ("format", Format, "Incoming data format: stream or frame (default: stream)"),
            ("mode", Mode, "Screen output mode:"),
            ("mavlink_port", MavlinkPort.ToString(), "Mavlink port for telemetry (default: 14750)"),
            ("osd", Osd, "Osd: none, simple, or custom (default: simple)"),
            ("records", Records, "Records archive"),
            ("extra", $"\"{Extra}\"", "Extra param (write in one line split by space)"),
            ("osd_elements", $"\"{OsdElements}\"", "Custom OSD elements"),
        };
    }
}