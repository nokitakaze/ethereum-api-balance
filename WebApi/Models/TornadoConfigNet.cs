using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.WebApi.Models;

// Todo refactor away from this project
public class TornadoConfigNet
{
    public ExplorerUrl explorerUrl;
    public Dictionary<string, RpcUrl> rpcUrls;
    public string currencyName;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RpcUrl
    {
        public string name;
        public string url;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ExplorerUrl
    {
        public string tx;
        public string address;
        public string block;
    }
}