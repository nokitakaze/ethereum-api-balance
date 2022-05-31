using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using EthereumAPIBalance.WebApi.Models;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using NokitaKaze.EthereumChainConfig;

namespace EthereumAPIBalance.WebApi.Services;

public class BalanceService
{
    private protected readonly ILogger<BalanceService> Logger;
    private protected readonly EthereumChainConfigService EthereumChainConfigService;

    private protected readonly string DefaultAccountPrivateKey;

    public BalanceService(
        ILogger<BalanceService> logger,
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
    }

    public ICollection<string> GetRPCUrls(int chainId)
    {
        return EthereumChainConfigService.GetChainConfig(chainId).GetRPCUrls();
    }

    // ReSharper disable once InconsistentNaming
    private protected string? _erc20Abi;

    private protected async Task<string> GetERC20ABI()
    {
        if (_erc20Abi != null)
        {
            return _erc20Abi!;
        }

        var filename = Path.Combine(AppContext.BaseDirectory, "erc20-token-abi.json");
        _erc20Abi = await File.ReadAllTextAsync(filename);
        return _erc20Abi!;
    }

    private protected readonly Dictionary<(int, string), TokenInfo> TokenInfos = new();
    private protected readonly SemaphoreSlim TokenInfoSlim = new(1);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private protected class TokenInfo
    {
        public string name = string.Empty;
        public string symbol = string.Empty;
        public int decimals;
        public long decimLong;
        public decimal decimLongR;
    }

    private protected Nethereum.Web3.Accounts.Account GetDefaultAccount(int chainId)
    {
        return new Nethereum.Web3.Accounts.Account(DefaultAccountPrivateKey, new BigInteger(chainId));
    }

    private protected async Task<TokenInfo?> GetTokenInformation(int chainId, string tokenAddress)
    {
        await TokenInfoSlim.WaitAsync();
        try
        {
            var abiText = await GetERC20ABI();
            var key = (chainId, tokenAddress);

            if (TokenInfos.ContainsKey(key))
            {
                return TokenInfos[key];
            }

            var urls = GetRPCUrls(chainId);
            foreach (var url in urls)
            {
                var client = GetDefaultAccount(chainId);
                var web3 = new Web3(client, url);
                var contract = web3.Eth.GetContract(abiText, tokenAddress);

                var decimals = await contract
                    .GetFunction("decimals")
                    .CallAsync<int>();
                if (decimals == 0)
                {
                    return null;
                }

                var decimLong = Enumerable
                    .Repeat(0, decimals)
                    .Aggregate(1L, (a, _) => a * 10);
                var decimLongR = 1m / decimLong;

                var name = await contract
                    .GetFunction("name")
                    .CallAsync<string>();
                var symbol = await contract
                    .GetFunction("symbol")
                    .CallAsync<string>();

                var response = new TokenInfo
                {
                    name = name,
                    symbol = symbol,
                    decimals = decimals,
                    decimLong = decimLong,
                    decimLongR = decimLongR,
                };

                TokenInfos[key] = response;

                return response;
            }

            throw new Exception("Can not obtain token information");
        }
        finally
        {
            TokenInfoSlim.Release();
        }
    }

    #region Balance

    private protected async Task<Models.APIBalanceResponse> GetBalanceWithUrl(
        string address,
        string? token,
        int chainId,
        string web3Url
    )
    {
        var client = GetDefaultAccount(chainId);
        var web3 = new Web3(client, web3Url);
        var chainConfig = EthereumChainConfigService.GetChainConfig(chainId);

        if (token == string.Empty)
        {
            token = null;
        }

        Common.TokenInfo info;
        Models.APIBalanceItem balance;
        if (token != null)
        {
            // ERC-20
            var abiText = await GetERC20ABI();
            var contract = web3.Eth.GetContract(abiText, token);

            var rawInfo = await GetTokenInformation(chainId, token);
            if (rawInfo == null)
            {
                // Non existent coin
                info = new Common.TokenInfo
                {
                    asset_type = Common.TokenInfo.AssetType.NotExist,
                    name = string.Empty,
                    symbol = string.Empty,
                    decimals = 0,
                };
                balance = FormatBalanceItem(BigInteger.Zero, 0);
            }
            else
            {
                var balanceWei = await contract
                    .GetFunction("balanceOf")
                    .CallAsync<BigInteger>(address);

                info = new Common.TokenInfo
                {
                    asset_type = Common.TokenInfo.AssetType.ERC20,
                    name = rawInfo.name,
                    symbol = rawInfo.symbol,
                    decimals = rawInfo.decimals,
                };
                balance = FormatBalanceItem(balanceWei, rawInfo.decimals);
            }
        }
        else
        {
            // Ethereum itself
            var response = await web3.Eth.GetBalance.SendRequestAsync(address);
            var balanceWei = response.Value;

            balance = FormatBalanceItem(balanceWei, 18);
            info = new Common.TokenInfo
            {
                asset_type = Common.TokenInfo.AssetType.BaseCoin,
                name = chainConfig.currencyName,
                symbol = chainConfig.currencyName,
                decimals = 18,
            };
        }

        Models.APIAddressInfo addressInfoItem;
        {
            var addressUrl = chainConfig.explorerUrl!.GetAddressURL(address);
            var balanceUrl = chainConfig.explorerUrl!.GetBalanceURL(address, token);
            addressInfoItem = new Models.APIAddressInfo
            {
                address = address.ToLowerInvariant(),
                address_url = addressUrl,
                address_url_token = balanceUrl,
            };
        }

        return new Models.APIBalanceResponse
        {
            chain_id = chainId,
            address = addressInfoItem,
            balance = balance,
            token = info,
        };
    }

    private protected async Task<Models.APIBalanceResponse> GetBalanceWithUrlAndRetries(
        string address,
        string? token,
        int chainId,
        string web3Url
    )
    {
        var chainConfig = EthereumChainConfigService.GetChainConfig(chainId);
        Exception? lastException = null;
        for (var i = 0; i < chainConfig.rpcCallRetryAttempt; i++)
        {
            try
            {
                return await GetBalanceWithUrl(address, token, chainId, web3Url);
            }
            catch (Exception e)
            {
                lastException = e;
            }
        }

        throw lastException!;
    }

    public static Models.APIBalanceItem FormatBalanceItem(BigInteger balanceWei, int decimals)
    {
        if (balanceWei.IsZero)
        {
            return new Models.APIBalanceItem
            {
                wei_integer = BigInteger.Zero,
                pow_string = "0",
                pow_decimal = 0,
            };
        }

        var balanceReal = AmountConverter.GetPoweredFromWei(decimals, balanceWei);

        var fullItem = new Models.APIBalanceItem
        {
            wei_integer = balanceWei,
            pow_string = balanceReal.ToString("F" + decimals).TrimEnd('0'),
            pow_decimal = balanceReal
        };

        if (fullItem.pow_string.EndsWith("."))
        {
            fullItem.pow_string = fullItem.pow_string[..^1];
        }

        return fullItem;
    }

    public async Task<Models.APIBalanceResponse> GetBalance(
        string address,
        string? token,
        int chainId = 1
    )
    {
        var urls = GetRPCUrls(chainId);
        if (token == string.Empty)
        {
            token = null;
        }

        Logger.LogInformation(
            "Get value for address {address}. Token: {token}. Chain Id: {chainId}",
            address, token, chainId
        );

        foreach (var url in urls)
        {
            try
            {
                var response = await GetBalanceWithUrlAndRetries(address, token, chainId, url);
                Logger.LogInformation(
                    "Get value for address {address}. Token: {token}. Chain Id: {chainId} = {balance} ({weiBalance})",
                    address,
                    token,
                    chainId,
                    response.balance.pow_string,
                    response.balance.wei_string
                );

                return response;
            }
            catch (APIException e)
            {
                Logger.LogInformation(
                    "Can not obtain information from {url} for address {address}. Exception: {exception}",
                    url, address, e
                );
            }
            catch (RpcClientUnknownException e)
            {
                Logger.LogInformation(
                    "RpcClientUnknownException. Can not obtain information from {url} for address {address}. Exception: {exception}",
                    url, address, e
                );
            }
        }

        throw new APIException();
    }

    #endregion

    #region Next Nonce

    private async Task<APINonceResponse> GetNextNonceWithUrl(
        string address,
        int chainId,
        string web3Url
    )
    {
        var chainConfig = EthereumChainConfigService.GetChainConfig(chainId);
        Models.APIAddressInfo addressInfoItem;
        {
            var addressUrl = chainConfig.explorerUrl!.GetAddressURL(address);
            var balanceUrl = chainConfig.explorerUrl!.GetBalanceURL(address);
            addressInfoItem = new Models.APIAddressInfo
            {
                address = address.ToLowerInvariant(),
                address_url = addressUrl,
                address_url_token = balanceUrl,
            };
        }

        var client = GetDefaultAccount(chainId);
        var web3 = new Web3(client, web3Url);
        var response = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);

        return new APINonceResponse
        {
            chain_id = chainId,
            address = addressInfoItem,
            nonce_id = response.ToLong(),
        };
    }

    private async Task<APINonceResponse> GetNextNonceWithUrlAndRetries(
        string address,
        int chainId,
        string web3Url
    )
    {
        var chainConfig = EthereumChainConfigService.GetChainConfig(chainId);
        Exception? lastException = null;
        for (var i = 0; i < chainConfig.rpcCallRetryAttempt; i++)
        {
            try
            {
                return await GetNextNonceWithUrl(address, chainId, web3Url);
            }
            catch (Exception e)
            {
                lastException = e;
            }
        }

        throw lastException!;
    }

    public async Task<APINonceResponse> GetNextNonce(string address, int chainId = 1)
    {
        Logger.LogInformation(
            "Get sent transaction count for address {address}. Chain Id: {chainId}",
            address, chainId
        );
        var urls = GetRPCUrls(chainId);

        foreach (var url in urls)
        {
            try
            {
                var response = await GetNextNonceWithUrlAndRetries(address, chainId, url);
                Logger.LogInformation(
                    "Get sent transaction count for address {address}. Chain Id: {chainId}",
                    address, chainId
                );

                return response;
            }
            catch (APIException e)
            {
                Logger.LogInformation(
                    "Can not obtain information from {url} for address {address}. Exception: {exception}",
                    url, address, e
                );
            }
            catch (RpcClientUnknownException e)
            {
                Logger.LogInformation(
                    "RpcClientUnknownException. Can not obtain information from {url} for address {address}. Exception: {exception}",
                    url, address, e
                );
            }
        }

        throw new APIException();
    }

    #endregion
}