using System.Diagnostics.CodeAnalysis;

namespace EthereumAPIBalance.WebApi.Models;

/// <summary>
/// Main response object to balance request
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class APIBalanceResponse : Common.BalanceResponse
{
    /// <summary>
    /// Information about requested address
    /// </summary>
    public new APIAddressInfo address { get; set; }

    /// <summary>
    /// Balance information
    /// </summary>
    public new APIBalanceItem balance { get; set; }
}