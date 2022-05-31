using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.Common;

/// <summary>
/// Information about requested address
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class AddressInfo
{
    /// <summary>
    /// Address
    /// </summary>
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// Address in Checksum format
    /// </summary>
    public string address_checksum { get; set; } = string.Empty;

    /// <summary>
    /// URL for address in public explorers
    /// </summary>
    public string address_url { get; set; } = string.Empty;

    /// <summary>
    /// URL for address in public explorers
    /// </summary>
    public string address_url_token { get; set; } = string.Empty;
}