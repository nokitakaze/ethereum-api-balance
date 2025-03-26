using EthereumAPIBalance.WebApi.Services;
using NokitaKaze.EthereumChainConfig;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine("Environment: {0}", environment);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // "EthereumAPIBalance.Common"
    var assemblies = new[] { "EthereumAPIBalance.WebApi" };
    foreach (var assembly in assemblies)
    {
        var filename = Path.Combine(AppContext.BaseDirectory, assembly + ".xml");
        options.IncludeXmlComments(filename);
    }
});

string? EVMConfigFilename;
{
    var folders = new string?[] { null, AppContext.BaseDirectory };
    EVMConfigFilename = new string[] { "custom-ethereum-config.json", "default-ethereum-config.json" }
        .SelectMany(pureFilename => folders
            .Select(folder => !string.IsNullOrEmpty(folder) ? Path.Combine(folder, pureFilename) : pureFilename))
        .FirstOrDefault(File.Exists);
    if (!string.IsNullOrEmpty(EVMConfigFilename))
    {
        Console.WriteLine("EVM config file: {0}", EVMConfigFilename);
    }
}

builder.Services.AddSingleton<BalanceService>();
builder.Services.AddSingleton<FeeService>();
builder.Services.AddSingleton(_ =>
{
    // ReSharper disable once ConvertToLambdaExpression
    return !string.IsNullOrEmpty(EVMConfigFilename)
        ? EthereumChainConfigService.CreateConfigFromFile(EVMConfigFilename)
        : EthereumChainConfigService.CreateConfigFromDefaultFile();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
