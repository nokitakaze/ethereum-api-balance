using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Serialization;

namespace EthereumAPIBalance.WebApi.Models;

/// <summary>
/// Balance information
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class APIBalanceItem : Common.BalanceItem
{
    /// <summary>
    /// Absolute unit count in decimal string format
    /// </summary>
    public new string wei_string => wei_integer.ToString();

    [JsonIgnore]
    public BigInteger wei_integer { get; set; }

    /// <summary>
    /// Absolute unit count in integer format
    /// </summary>
    public new decimal wei_decimal => (decimal)wei_integer;
}