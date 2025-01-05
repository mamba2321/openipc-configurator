using System.Collections.Generic;

namespace OpenIPC_Config.Parsers;

public class WfbConfParser : ConfigParserBase<WfbConfParser>
{
    public string Unit { get; set; } = "drone";
    public string Wlan { get; set; } = "wlan0";
    public string Region { get; set; } = "00";
    public int Channel { get; set; } = 64;
    public int TxPower { get; set; } = 1;
    public int DriverTxPowerOverride { get; set; } = 25;
    public int Bandwidth { get; set; } = 20;
    public int Stbc { get; set; } = 0;
    public int Ldpc { get; set; } = 0;
    public int McsIndex { get; set; } = 2;
    public int Stream { get; set; } = 0;
    public int LinkId { get; set; } = 7669206;
    public int UdpPort { get; set; } = 5600;
    public int RcvBuf { get; set; } = 456000;
    public string FrameType { get; set; } = "data";
    public int FecK { get; set; } = 8;
    public int FecN { get; set; } = 12;
    public int PoolTimeout { get; set; } = 0;
    public string GuardInterval { get; set; } = "long";

    protected override void MapKeyToProperty(string key, string value)
    {
        switch (key)
        {
            case "unit": Unit = value; break;
            case "wlan": Wlan = value; break;
            case "region": Region = value; break;
            case "channel": Channel = int.Parse(value); break;
            case "txpower": TxPower = int.Parse(value); break;
            case "driver_txpower_override": DriverTxPowerOverride = int.Parse(value); break;
            case "bandwidth": Bandwidth = int.Parse(value); break;
            case "stbc": Stbc = int.Parse(value); break;
            case "ldpc": Ldpc = int.Parse(value); break;
            case "mcs_index": McsIndex = int.Parse(value); break;
            case "stream": Stream = int.Parse(value); break;
            case "link_id": LinkId = int.Parse(value); break;
            case "udp_port": UdpPort = int.Parse(value); break;
            case "rcv_buf": RcvBuf = int.Parse(value); break;
            case "frame_type": FrameType = value; break;
            case "fec_k": FecK = int.Parse(value); break;
            case "fec_n": FecN = int.Parse(value); break;
            case "pool_timeout": PoolTimeout = int.Parse(value); break;
            case "guard_interval": GuardInterval = value; break;
        }
    }

    protected override IEnumerable<(string Key, string Value, string Comment)> GetConfigProperties()
    {
        return new List<(string, string, string)>
        {
            ("unit", Unit, "Unit type: drone or gs"),
            ("wlan", Wlan, "Wireless interface (default: wlan0)"),
            ("region", Region, "Region code (default: 00)"),
            ("channel", Channel.ToString(), "Channel number or frequency (default: 64)"),
            ("txpower", TxPower.ToString(), "Transmission power"),
            ("driver_txpower_override", DriverTxPowerOverride.ToString(), "Driver-specific TX power override"),
            ("bandwidth", Bandwidth.ToString(), "Channel bandwidth (default: 20MHz)"),
            ("stbc", Stbc.ToString(), "Space-Time Block Coding (default: 0)"),
            ("ldpc", Ldpc.ToString(), "Low-Density Parity Check (default: 0)"),
            ("mcs_index", McsIndex.ToString(), "Modulation and Coding Scheme index"),
            ("stream", Stream.ToString(), "Stream ID"),
            ("link_id", LinkId.ToString(), "Link identifier"),
            ("udp_port", UdpPort.ToString(), "UDP port for video data"),
            ("rcv_buf", RcvBuf.ToString(), "Receive buffer size"),
            ("frame_type", FrameType, "Frame type (default: data)"),
            ("fec_k", FecK.ToString(), "FEC K value"),
            ("fec_n", FecN.ToString(), "FEC N value"),
            ("pool_timeout", PoolTimeout.ToString(), "Pool timeout"),
            ("guard_interval", GuardInterval, "Guard interval (default: long)")
        };
    }
}