using Microsoft.AspNetCore.Mvc;
using EthereumAPIBalance.WebApi.Models;
using EthereumAPIBalance.WebApi.Services;

namespace EthereumAPIBalance.WebApi.Controllers;

/// <summary>
/// Get balance for address
/// </summary>
[ApiController]
[Route("balance")]
public class BalanceController : ControllerBase
{
    private protected readonly ILogger<BalanceController> Logger;
    private protected readonly BalanceService BalanceService;

    /// <summary>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="balanceService"></param>
    public BalanceController(
        ILogger<BalanceController> logger,
        BalanceService balanceService
    )
    {
        Logger = logger;
        BalanceService = balanceService;
    }

    /// <summary>
    /// Get ether balance for address in Ethereum main net
    /// </summary>
    /// <param name="address">Address</param>
    /// <returns></returns>
    [HttpGet("{address}")]
    public Task<APIBalanceResponse> GetBalance(string address)
    {
        return BalanceService.GetBalance(address, null);
    }

    /// <summary>
    /// Get ERC-20 token balance for address in Ethereum main net
    /// </summary>
    /// <param name="address">Address</param>
    /// <param name="tokenAddress">ERC-20 token address</param>
    /// <returns></returns>
    [HttpGet("{address}/{tokenAddress}")]
    public Task<APIBalanceResponse> GetBalance(string address, string tokenAddress)
    {
        tokenAddress = tokenAddress.Trim();

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (tokenAddress == string.Empty)
        {
            return BalanceService.GetBalance(address, null);
        }

        return BalanceService.GetBalance(address, tokenAddress);
    }

    /// <summary>
    /// Get main coin balance for address in requested chain
    /// </summary>
    /// <param name="chainId">Chain ID</param>
    /// <param name="address">Address</param>
    /// <returns></returns>
    [HttpGet("{chainId:int}/{address}")]
    public Task<APIBalanceResponse> GetBalance(int chainId, string address)
    {
        return BalanceService.GetBalance(address, null, chainId);
    }

    /// <summary>
    /// Get ERC-20/BEP-20/etc balance for address in requested chain
    /// </summary>
    /// <param name="chainId">Chain ID</param>
    /// <param name="address">Address</param>
    /// <param name="tokenAddress">ERC-20 token address</param>
    /// <returns></returns>
    [HttpGet("{chainId:int}/{address}/{tokenAddress}")]
    public Task<APIBalanceResponse> GetBalance(int chainId, string address, string tokenAddress)
    {
        tokenAddress = tokenAddress.Trim();

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (tokenAddress == string.Empty)
        {
            return BalanceService.GetBalance(address, null, chainId);
        }

        return BalanceService.GetBalance(address, tokenAddress, chainId);
    }
}