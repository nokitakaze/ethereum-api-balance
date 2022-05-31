using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.Common;

/// <summary>
/// Balance information
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class BalanceItem
{
    /// <summary>
    /// Absolute unit count in decimal string format
    /// </summary>
    public string wei_string { get; set; } = string.Empty;

    /// <summary>
    /// Absolute unit count in integer format
    /// </summary>
    public decimal wei_decimal { get; set; }

    /// <summary>
    /// Real unit count in decimal string format
    /// </summary>
    public string pow_string { get; set; } = string.Empty;

    /// <summary>
    /// Real unit count in real format
    /// </summary>
    public decimal pow_decimal { get; set; }
}