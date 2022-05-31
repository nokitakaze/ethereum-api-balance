using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.Common;

/// <summary>
/// Information about requested token
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class TokenInfo
{
    /// <summary>
    /// Token full name.
    /// For example: Tether
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// Token symbol (ticker) name.
    /// For example: USDT
    /// </summary>
    public string? symbol { get; set; }

    /// <summary>
    /// Decimals count in the asset
    /// </summary>
    public int decimals { get; set; }

    /// <summary>
    /// Type of the asset
    /// </summary>
    public AssetType asset_type { get; set; } = AssetType.NotExist;

    /// <summary>
    /// Type of the asset
    /// </summary>
    public enum AssetType
    {
        /// <summary>
        /// Token doesn't exist
        /// </summary>
        NotExist = -1,

        /// <summary>
        /// Base coin for network
        /// </summary>
        BaseCoin = 0,

        /// <summary>
        /// ERC-20 (BEP-20) token
        /// </summary>
        ERC20 = 20,
    }
}