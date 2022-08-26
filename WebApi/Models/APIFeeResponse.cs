using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.WebApi.Models;

/// <summary>
/// Main response object for fee request
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class APIFeeResponse
{
    /// <summary>
    /// Requested chain id. 1 for real Ethereum
    /// </summary>
    public int chain_id { get; set; }

    /// <summary>
    /// Fee in gwei
    /// </summary>
    public decimal fee_gwei { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    public long update_time { get; set; }

    /// <summary>
    /// Is EIP-1559 enabled for the chain.
    /// https://github.com/ethereum/EIPs/blob/master/EIPS/eip-1559.md
    /// </summary>
    public bool eip1559_enabled { get; set; }

    /// <summary>
    /// EIP-1559: Low priority fee price in gwei
    /// </summary>
    public decimal priority_low_gwei { get; set; }

    /// <summary>
    /// EIP-1559: Medium priority fee price in gwei
    /// </summary>
    public decimal priority_medium_gwei { get; set; }

    /// <summary>
    /// EIP-1559: High priority fee price in gwei
    /// </summary>
    public decimal priority_high_gwei { get; set; }
}