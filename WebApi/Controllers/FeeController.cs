using EthereumAPIBalance.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using EthereumAPIBalance.WebApi.Services;

namespace EthereumAPIBalance.WebApi.Controllers;

/// <summary>
/// Get fee for chains
/// </summary>
[ApiController]
[Route("fee")]
public class FeeController : ControllerBase
{
    private protected readonly ILogger<FeeController> Logger;
    private protected readonly FeeService FeeService;

    /// <summary>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="feeService"></param>
    public FeeController(
        ILogger<FeeController> logger,
        FeeService feeService
    )
    {
        Logger = logger;
        FeeService = feeService;
    }

    /// <summary>
    /// Get fee in gwei for address in requested chain
    /// </summary>
    /// <param name="chainId">Chain ID</param>
    /// <returns></returns>
    [HttpGet("{chainId:int}")]
    public Task<APIFeeResponse> GetNonce(int chainId)
    {
        return FeeService.GetFee(chainId);
    }
}