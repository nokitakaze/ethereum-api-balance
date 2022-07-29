using EthereumAPIBalance.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using EthereumAPIBalance.WebApi.Services;

namespace EthereumAPIBalance.WebApi.Controllers;

/// <summary>
/// Get nonce for address
/// </summary>
[ApiController]
[Route("nonce")]
public class NonceController : ControllerBase
{
    private protected readonly ILogger<NonceController> Logger;
    private protected readonly BalanceService BalanceService;

    /// <summary>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="balanceService"></param>
    public NonceController(
        ILogger<NonceController> logger,
        BalanceService balanceService
    )
    {
        Logger = logger;
        BalanceService = balanceService;
    }

    /// <summary>
    /// Get next nonce id for address in main Ethereum
    /// </summary>
    /// <param name="address">Address</param>
    /// <returns></returns>
    [HttpGet("{address}")]
    public Task<APINonceResponse> GetNonce(string address)
    {
        return BalanceService.GetNextNonce(address);
    }

    /// <summary>
    /// Get next nonce id for address in requested chain
    /// </summary>
    /// <param name="chainId">Chain ID</param>
    /// <param name="address">Address</param>
    /// <returns></returns>
    [HttpGet("{chainId:int}/{address}")]
    public Task<APINonceResponse> GetNonce(int chainId, string address)
    {
        return BalanceService.GetNextNonce(address, chainId);
    }
}