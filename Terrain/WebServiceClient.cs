using System;
using System.Net.Http;
using System.Threading.Tasks;

public class WebServiceClient
{
    protected string ApiKey;
    protected string _BaseUri;
    protected UriBuilder _UriBuilder;
    protected HttpClient _HttpClient;

    public async Task<string> GetStringResponse()
    {
        return await (await _HttpClient.GetAsync(_UriBuilder.Uri)).Content.ReadAsStringAsync();
    }
}
