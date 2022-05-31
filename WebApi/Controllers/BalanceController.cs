using Microsoft.AspNetCore.Mvc;
using EthereumAPIBalance.WebApi.Models;
using EthereumAPIBalance.WebApi.Services;

namespace EthereumAPIBalance.WebApi.Controllers;

[ApiController]
[Route("balance")]
public class BalanceController : ControllerBase
{
    private protected readonly ILogger<BalanceController> Logger;
    private protected readonly BalanceService BalanceService;

    public BalanceController(
        ILogger<BalanceController> logger,
        BalanceService balanceService
    )
    {
        Logger = logger;
        BalanceService = balanceService;
    }

    [HttpGet("{address}")]
    public Task<APIBalanceResponse> GetBalance(string address)
    {
        return BalanceService.GetBalance(address, null);
    }

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

    [HttpGet("{chainId:int}/{address}")]
    public Task<APIBalanceResponse> GetBalance(int chainId, string address)
    {
        return BalanceService.GetBalance(address, null, chainId);
    }

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