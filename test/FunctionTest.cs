using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;

using HelloWorldAot;

using Xunit;

namespace Tests;

public class FunctionTest
{
  private static readonly HttpClient Client = new();

  private static async Task<string> GetCallingIp()
  {
    Client.DefaultRequestHeaders.Accept.Clear();
    Client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

    var stringTask = Client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(false);

    var msg = await stringTask;
    return msg.Replace("\n", "");
  }

  [Fact]
  public async Task TestHelloWorldFunctionHandler()
  {
    var request = new APIGatewayHttpApiV2ProxyRequest();
    var context = new TestLambdaContext();
    var location = await GetCallingIp();
    var body = new Dictionary<string, string>
               {
                 { "message", "hello world" },
                 { "location", location }
               };

    var expectedResponse = new APIGatewayHttpApiV2ProxyResponse
                           {
                             Body = JsonSerializer.Serialize(body),
                             StatusCode = 200,
                             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                           };

    var response = await Function.FunctionHandler(request, context);

    // ReSharper disable Xunit.XunitTestWithConsoleOutput
    Console.WriteLine("Lambda Response: \n" + response.Body);
    Console.WriteLine("Expected Response: \n" + expectedResponse.Body);
    // ReSharper restore Xunit.XunitTestWithConsoleOutput

    Assert.Equal(expectedResponse.Body, response.Body);
    Assert.Equal(expectedResponse.Headers, response.Headers);
    Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
  }
}
