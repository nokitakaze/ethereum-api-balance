using System.Diagnostics.CodeAnalysis;
using Nethereum.Util;

namespace EthereumAPIBalance.WebApi.Models;

/// <summary>
/// Information about requested address
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class APIAddressInfo : Common.AddressInfo
{
    /// <summary>
    /// Address in Checksum format
    /// </summary>
    public new string address_checksum => new AddressUtil().ConvertToChecksumAddress(address);
}