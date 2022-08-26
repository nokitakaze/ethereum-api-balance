using System.Collections.Concurrent;
using System.Numerics;
using EthereumAPIBalance.WebApi.Models;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using NokitaKaze.EthereumChainConfig;

namespace EthereumAPIBalance.WebApi.Services;

/// <summary>
/// Main singleton service for the entire program
/// </summary>
public class FeeService
{
    private protected readonly ILogger<FeeService> Logger;
    private protected readonly EthereumChainConfigService EthereumChainConfigService;

    private protected readonly string DefaultAccountPrivateKey;
    private protected readonly IReadOnlyDictionary<int, SemaphoreSlim> GetFeeSlim;

    private protected readonly IDictionary<int, (DateTimeOffset time, decimal fee)> FeeByChain =
        new Dictionary<int, (DateTimeOffset time, decimal fee)>();

    private protected readonly IReadOnlyCollection<int> Chains;

    /// <summary>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="ethereumChainConfigService"></param>
    public FeeService(
        ILogger<FeeService> logger,
        EthereumChainConfigService ethereumChainConfigService
    )
    {
        Logger = logger;
        EthereumChainConfigService = ethereumChainConfigService;
        {
            var rnd = new Random();
            var privateBytes = new byte[32];
            rnd.NextBytes(privateBytes);

            DefaultAccountPrivateKey = string.Concat(privateBytes.Select(t => t.ToString("x2")));
        }

        Chains = EthereumChainConfigService
            .GetChainIds()
            .OrderByDescending(chainId => chainId == 1)
            .ThenByDescending(chainId => chainId == 56)
            .ThenByDescending(chainId => chainId == 5)
            .ThenBy(t => t)
            .ToArray();

        GetFeeSlim = new ConcurrentDictionary<int, SemaphoreSlim>(Chains
            .ToDictionary(t => t, _ => new SemaphoreSlim(1)));
        FeeIsReady = new ConcurrentDictionary<int, ManualResetEvent>(Chains
            .ToDictionary(t => t, _ => new ManualResetEvent(false)));
        EffectiveFee = new ConcurrentDictionary<int, Dictionary<int, decimal>>(Chains
            .ToDictionary(t => t, _ => new Dictionary<int, decimal>()));
        LastFeeRequest = new ConcurrentDictionary<int, DateTimeOffset>(Chains
            .ToDictionary(t => t, _ => DateTimeOffset.MinValue));
        Task.Run(StartCronTask);
    }

    #region Cron

    private protected readonly ConcurrentDictionary<int, DateTimeOffset> LastFeeRequest;
    private protected readonly IReadOnlyDictionary<int, ManualResetEvent> FeeIsReady;

    private protected async Task StartCronTask()
    {
        // LastFeeRequest
        while (true)
        {
            try
            {
                await StartCronTask_Iteration();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error when processing update");
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private protected async Task StartCronTask_Iteration()
    {
        var cacheTime = GetCacheTimeForFee();

        var collectedChains = new HashSet<int>();
        foreach (var item in LastFeeRequest
                     .Where(x => x.Value >= DateTimeOffset.UtcNow - TimeSpan.FromHours(1)))
        {
            var chainId = item.Key;
            if (FeeByChain.ContainsKey(chainId) && (FeeByChain[chainId].time > DateTimeOffset.UtcNow - cacheTime))
            {
                collectedChains.Add(chainId);
                continue;
            }

            Logger.LogInformation("Update fee for chain {ChainId}", chainId);

            FeeIsReady[chainId].Reset();
            await UpdateFee(chainId);
            collectedChains.Add(chainId);
        }

        foreach (var chainId in FeeIsReady.Keys.Where(x => !collectedChains.Contains(x)))
        {
            FeeIsReady[chainId].Reset();
        }
    }

    private protected async Task UpdateFee(int chainId)
    {
        var urls = GetRPCUrls(chainId);
        while (true)
        {
            foreach (var url in urls)
            {
                var feeD = await GetFeeFromUrl(chainId, url);
                if (feeD == null)
                {
                    continue;
                }

                FeeByChain[chainId] = (DateTimeOffset.UtcNow, feeD.Value);
                Logger.LogInformation("Chain #{ChainId}. New fee = {FeeGwei:F8} gwei",
                    chainId, FeeByChain[chainId].fee);
                FeeIsReady[chainId].Set();
                return;
            }
        }
    }

    #endregion

    /// <summary>
    /// Get RPC urls for requested chain
    /// </summary>
    /// <param name="chainId"></param>
    /// <returns></returns>
    public ICollection<string> GetRPCUrls(int chainId)
    {
        return EthereumChainConfigService.GetChainConfig(chainId).GetRPCUrls();
    }

    /// <summary>
    /// Is EIP-1559 enable for the chain
    /// </summary>
    /// <param name="chainId"></param>
    /// <returns></returns>
    public bool IsChainEIP1559(int chainId)
    {
        return EthereumChainConfigService.GetChainConfig(chainId).EIP1559Enabled;
    }

    private protected TimeSpan GetCacheTimeForFee()
    {
        // TODO configuration
        return TimeSpan.FromSeconds(60);
    }

    private protected int GetBlockCount()
    {
        // TODO configuration
        return 5;
    }

    private protected readonly IReadOnlyDictionary<int, Dictionary<int, decimal>> EffectiveFee;

    private protected Task<decimal?> GetFeeFromUrl(int chainId, string rpcUrl)
    {
        return IsChainEIP1559(chainId)
            ? GetFeeFromUrl_EIP1559(chainId, rpcUrl)
            : GetFeeFromUrl_NoEIP1559(chainId, rpcUrl);
    }

    private protected async Task<decimal?> GetFeeFromUrl_NoEIP1559(int chainId, string rpcUrl)
    {
        var client = new Nethereum.Web3.Accounts.Account(DefaultAccountPrivateKey, new BigInteger(chainId));
        var web3 = new Web3(client, rpcUrl);
        try
        {
            var responseChainId = await web3.Eth.ChainId.SendRequestAsync();
            if (responseChainId!.Value != chainId)
            {
                Logger.LogWarning("RPC URL {RpcUrl} contain wrong chain #{ActualChainId}. Expected {ExpectedChainId}",
                    rpcUrl, responseChainId.Value, chainId);
                return null;
            }

            var lastBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var lastBlockNumberInt = (int)lastBlockNumber.Value;

            var effectiveFees = new List<decimal>();
            for (var blockId = lastBlockNumberInt; blockId >= lastBlockNumberInt - GetBlockCount() + 1; blockId--)
            {
                var block = await web3
                    .Eth
                    .Blocks
                    .GetBlockWithTransactionsHashesByNumber
                    .SendRequestAsync(new HexBigInteger(blockId));
                if (blockId == lastBlockNumberInt)
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value);
                    if (dt < DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
                    {
                        return null;
                    }
                }

                if (EffectiveFee[chainId].ContainsKey(blockId))
                {
                    effectiveFees.Add(EffectiveFee[chainId][blockId]);
                    continue;
                }

                var fees = new List<BigInteger>();
                foreach (var txHash in block.TransactionHashes.Skip(1))
                {
                    var txReceipt = await web3
                        .Eth
                        .Transactions
                        .GetTransactionReceipt
                        .SendRequestAsync(txHash);
                    fees.Add(txReceipt.EffectiveGasPrice.Value);
                }

                if (!fees.Any())
                {
                    effectiveFees.Add(0);
                    continue;
                }

                fees = fees.OrderBy(t => t).ToList();

                var effectiveFeeWei = fees[(int)Math.Floor(fees.Count * 0.1)];
                var effectiveFeeGwei = (1m / 1_000_000_000m) * (ulong)effectiveFeeWei;
                Logger.LogInformation("Block #{BlockId} Chain {ChainId} has effective fee {EffectiveFeeGwei} gwei",
                    blockId, chainId, effectiveFeeGwei);
                EffectiveFee[chainId][blockId] = effectiveFeeGwei;
                effectiveFees.Add(effectiveFeeGwei);
            }

            return effectiveFees.Max();
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "{Exception}", e.Message);
            return null;
        }
    }

    private protected async Task<decimal?> GetFeeFromUrl_EIP1559(int chainId, string rpcUrl)
    {
        var client = new Nethereum.Web3.Accounts.Account(DefaultAccountPrivateKey, new BigInteger(chainId));
        var web3 = new Web3(client, rpcUrl);
        try
        {
            var responseChainId = await web3.Eth.ChainId.SendRequestAsync();
            if (responseChainId!.Value != chainId)
            {
                Logger.LogWarning("RPC URL {RpcUrl} contain wrong chain #{ActualChainId}. Expected {ExpectedChainId}",
                    rpcUrl, responseChainId.Value, chainId);
                return null;
            }

            var lastBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var lastBlockNumberInt = (int)lastBlockNumber.Value;

            var effectiveFees = new List<decimal>();
            for (var blockId = lastBlockNumberInt; blockId >= lastBlockNumberInt - GetBlockCount() + 1; blockId--)
            {
                var block = await web3
                    .Eth
                    .Blocks
                    .GetBlockWithTransactionsHashesByNumber
                    .SendRequestAsync(new HexBigInteger(blockId));

                if (blockId == lastBlockNumberInt)
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value);
                    if (dt < DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
                    {
                        return null;
                    }
                }

                if (EffectiveFee[chainId].ContainsKey(blockId))
                {
                    effectiveFees.Add(EffectiveFee[chainId][blockId]);
                    continue;
                }

                var effectiveFeeGwei = (1m / 1_000_000_000m) * (ulong)block.BaseFeePerGas.Value;
                effectiveFees.Add(effectiveFeeGwei);
                EffectiveFee[chainId][blockId] = effectiveFeeGwei;
                effectiveFees.Add(effectiveFeeGwei);
            }

            return effectiveFees.Max();
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "{Exception}", e.Message);
            return null;
        }
    }

    private protected static (decimal low, decimal medium, decimal high) GetPriorityFeeGwei(int chainId)
    {
        return chainId switch
        {
            1 => (1, 2, 5),
            5 => (1m / 1_000_000_000, 1m / 1_000_000_000, 1m / 1_000_000_000),
            _ => (0, 0, 0)
        };
    }

    /// <summary>
    /// Get fee (in gwei) for transaction
    /// </summary>
    /// <param name="chainId"></param>
    /// <returns>Fee in gwei</returns>
    public async Task<APIFeeResponse> GetFee(int chainId)
    {
        Logger.LogInformation(
            "Get fee for chain {ChainId}",
            chainId
        );

        var semaphore = GetFeeSlim[chainId];
        try
        {
            await semaphore.WaitAsync();

            if (!FeeByChain.ContainsKey(chainId) || !FeeIsReady[chainId].WaitOne(0))
            {
                LastFeeRequest[chainId] = DateTimeOffset.UtcNow;
                await Task.Run(() => { FeeIsReady[chainId].WaitOne(); });
            }

            var (lowPriority, mediumPriority, highPriority) = GetPriorityFeeGwei(chainId);

            return new APIFeeResponse()
            {
                chain_id = chainId,
                fee_gwei = FeeByChain[chainId].fee,
                update_time = FeeByChain[chainId].time.ToUnixTimeSeconds(),
                eip1559_enabled = IsChainEIP1559(chainId),
                priority_low_gwei = lowPriority,
                priority_medium_gwei = mediumPriority,
                priority_high_gwei = highPriority,
            };
        }
        finally
        {
            semaphore.Release();
        }
    }
}