using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.Common;

/// <summary>
/// Main response object for balance request
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class BalanceResponse
{
    /// <summary>
    /// Requested chain id. 1 for real Ethereum
    /// </summary>
    public int chain_id { get; set; }

    /// <summary>
    /// Information about requested address
    /// </summary>
    public AddressInfo address { get; set; }

    /// <summary>
    /// Information about requested token
    /// </summary>
    public TokenInfo token { get; set; }

    /// <summary>
    /// Balance information
    /// </summary>
    public BalanceItem balance { get; set; }
}