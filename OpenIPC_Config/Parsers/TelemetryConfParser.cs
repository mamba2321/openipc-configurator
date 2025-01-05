using System;
using System.Collections.Generic;
using System.IO;

namespace OpenIPC_Config.Parsers;

public class UnitConfigParser : ConfigParserBase<UnitConfigParser>
{
    public string Unit { get; set; } = "drone";
    public string Serial { get; set; } = "/dev/ttyAMA0";
    public int Baud { get; set; } = 115200;
    public int Router { get; set; } = 0;
    public string Wlan { get; set; } = "wlan0";
    public int Bandwidth { get; set; } = 20;
    public int Stbc { get; set; } = 1;
    public int Ldpc { get; set; } = 1;
    public int McsIndex { get; set; } = 1;
    public int StreamRx { get; set; } = 16;
    public int StreamTx { get; set; } = 144;
    public int LinkId { get; set; } = 7669206;
    public string FrameType { get; set; } = "data";
    public int PortRx { get; set; } = 14651;
    public int PortTx { get; set; } = 14650;
    public int FecK { get; set; } = 1;
    public int FecN { get; set; } = 2;
    public int PoolTimeout { get; set; } = 0;
    public string GuardInterval { get; set; } = "long";
    public bool OneWay { get; set; } = false;
    public int Channels { get; set; } = 14;

    protected override void MapKeyToProperty(string key, string value)
    {
        switch (key)
        {
            case "unit": Unit = value; break;
            case "serial": Serial = value; break;
            case "baud": Baud = int.Parse(value); break;
            case "router": Router = int.Parse(value); break;
            case "wlan": Wlan = value; break;
            case "bandwidth": Bandwidth = int.Parse(value); break;
            case "stbc": Stbc = int.Parse(value); break;
            case "ldpc": Ldpc = int.Parse(value); break;
            case "mcs_index": McsIndex = int.Parse(value); break;
            case "stream_rx": StreamRx = int.Parse(value); break;
            case "stream_tx": StreamTx = int.Parse(value); break;
            case "link_id": LinkId = int.Parse(value); break;
            case "frame_type": FrameType = value; break;
            case "port_rx": PortRx = int.Parse(value); break;
            case "port_tx": PortTx = int.Parse(value); break;
            case "fec_k": FecK = int.Parse(value); break;
            case "fec_n": FecN = int.Parse(value); break;
            case "pool_timeout": PoolTimeout = int.Parse(value); break;
            case "guard_interval": GuardInterval = value; break;
            case "one_way": OneWay = bool.Parse(value); break;
            case "channels": Channels = int.Parse(value); break;
        }
    }

    protected override IEnumerable<(string Key, string Value, string Comment)> GetConfigProperties()
    {
        return new List<(string, string, string)>
        {
            ("unit", Unit, "Unit type: drone or gs"),
            ("serial", Serial, "Serial device path"),
            ("baud", Baud.ToString(), "Baud rate"),
            ("router", Router.ToString(), "Use mavfwd (0) or mavlink-routerd (1)"),
            ("wlan", Wlan, "Wireless interface"),
            ("bandwidth", Bandwidth.ToString(), "Channel bandwidth"),
            ("stbc", Stbc.ToString(), "Space-time block coding"),
            ("ldpc", Ldpc.ToString(), "Low-density parity-check"),
            ("mcs_index", McsIndex.ToString(), "Modulation and coding scheme index"),
            ("stream_rx", StreamRx.ToString(), "Stream RX value"),
            ("stream_tx", StreamTx.ToString(), "Stream TX value"),
            ("link_id", LinkId.ToString(), "Link ID"),
            ("frame_type", FrameType, "Frame type (data/control)"),
            ("port_rx", PortRx.ToString(), "Port for receiving data"),
            ("port_tx", PortTx.ToString(), "Port for transmitting data"),
            ("fec_k", FecK.ToString(), "Forward error correction K value"),
            ("fec_n", FecN.ToString(), "Forward error correction N value"),
            ("pool_timeout", PoolTimeout.ToString(), "Pool timeout"),
            ("guard_interval", GuardInterval, "Guard interval"),
            ("one_way", OneWay.ToString(), "Is one-way communication"),
            ("channels", Channels.ToString(), "RC override channels"),
        };
    }
}