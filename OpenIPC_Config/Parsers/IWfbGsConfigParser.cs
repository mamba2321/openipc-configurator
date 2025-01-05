namespace OpenIPC_Config.Parsers;

public interface IWfbGsConfigParser
{
    string TxPower { get; set; }
    string GetUpdatedConfigString();
}