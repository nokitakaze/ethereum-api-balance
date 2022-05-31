using EthereumAPIBalance.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using EthereumAPIBalance.WebApi.Services;

namespace EthereumAPIBalance.WebApi.Controllers;

[ApiController]
[Route("nonce")]
public class NonceController : ControllerBase
{
    private protected readonly ILogger<NonceController> Logger;
    private protected readonly BalanceService BalanceService;

    public NonceController(
        ILogger<NonceController> logger,
        BalanceService balanceService
    )
    {
        Logger = logger;
        BalanceService = balanceService;
    }

    [HttpGet("{address}")]
    public Task<APINonceResponse> GetNonce(string address)
    {
        return BalanceService.GetNextNonce(address);
    }

    [HttpGet("{chainId:int}/{address}")]
    public Task<APINonceResponse> GetNonce(int chainId, string address)
    {
        return BalanceService.GetNextNonce(address, chainId);
    }
}