using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.Common;

/// <summary>
/// Main response object to balance request
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class NonceResponse
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
    /// Next nonce id
    /// </summary>
    public long nonce_id { get; set; }
}