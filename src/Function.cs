using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace HelloWorldAot;

public class Function
{
  private static readonly HttpClient Client = new();

  /// <summary>
  /// The main entry point for the Lambda function. The main function is called once during the Lambda init phase.
  /// It initializes the .NET Lambda runtime client passing in the function handler to invoke for each Lambda event and the JSON serializer to use for converting Lambda JSON format to the .NET types.
  /// </summary>
  private static async Task Main()
  {
    var handler = FunctionHandler;
    await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
      .Build()
      .RunAsync();
  }

  private static async Task<string> GetCallingIp()
  {
    Client.DefaultRequestHeaders.Accept.Clear();
    Client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

    var msg = await Client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(false);

    return msg.Replace("\n", "");
  }

  public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
  {
    var location = await GetCallingIp();
    var body = new Dictionary<string, string>
               {
                 { "message", "hello world" },
                 { "location", location }
               };

    return new APIGatewayHttpApiV2ProxyResponse
           {
             Body = JsonSerializer.Serialize(body, typeof(Dictionary<string, string>), LambdaFunctionJsonSerializerContext.Default),
             StatusCode = 200,
             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
           };
  }
}

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
  // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time which can deserialize our class and properties.
  // However, we must attribute this class to tell it what types to generate serialization code for.
  // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}
