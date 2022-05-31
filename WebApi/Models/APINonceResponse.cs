using EthereumAPIBalance.Common;

namespace EthereumAPIBalance.WebApi.Models;

public class APINonceResponse : NonceResponse
{
    /// <summary>
    /// Information about requested address
    /// </summary>
    public new APIAddressInfo address { get; set; }
}