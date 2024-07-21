using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

public abstract class WebServiceClient
{
    public WebServiceClient(List<string> _ApiKeys)
    {
        ApiKeys = _ApiKeys;
        _Client = new HttpClient { Timeout = TimeOut };
        Initialize();
    }

    public WebServiceClient(List<string> _ApiKeys, List<WebProxy> _Proxys)
    {
        ApiKeys = _ApiKeys;
        Proxys = _Proxys;
        _Client = new HttpClient { Timeout = TimeOut };
        Initialize();
    }

    public WebServiceClient(List<WebProxy> _Proxys)
    {
        Proxys = _Proxys;
        _Client = new HttpClient { Timeout = TimeOut };
        Initialize();
    }

    public WebServiceClient()
    {
        _Client = new HttpClient { Timeout = TimeOut };
        Initialize();
    }

    public bool UseProxy = true;
    protected List<string> ApiKeys = new List<string>();
    protected List<WebProxy> Proxys = new List<WebProxy>();
    protected string _BaseUri;
    protected UriBuilder _UriBuilder = new UriBuilder();
    protected int CurrentApiKeyIndex;
    protected int CurrentProxyIndex;
    protected TimeSpan TimeOut = new TimeSpan(0, 0, 50);
    protected int MaxRetriesAfterException = 3;
    protected int MaxRetriesIfStatusNotOk = 3;
    protected bool TrySendWithoutProxy = true;
    protected TimeSpan DelayIfStatusIsNotOk = TimeSpan.FromMilliseconds(500);
    private HttpClient _Client;
    private Dictionary<WebProxy, HttpClient> _ProxyClients = new Dictionary<WebProxy, HttpClient>();

    protected async Task<HttpResponseMessage> GetResponse()
    {
        HttpResponseMessage _Response = await GetResponse(UseProxy, CurrentProxyIndex);        
        CurrentProxyIndex = NextCollectionIndex(Proxys, CurrentProxyIndex);
        return _Response;
    }

    protected abstract void Initialize();

    protected int NextCollectionIndex<T>(List<T> _List, int CurrentIndex)
    {
        int Index = CurrentIndex + 1;
        if (Index > _List.Count - 1)
            Index = 0;
        return Index;
    }

    private async Task<HttpResponseMessage> GetResponse(bool _UseProxy, int _ProxyIndex)
    {
        string _LastException = "";
        HttpResponseMessage Response = null;
        for (int i = 0; i < MaxRetriesAfterException; i++)
        {
            try
            {
                if (i == MaxRetriesAfterException - 1 && TrySendWithoutProxy)
                {
                    return await GetHttpClientInstance(false, _ProxyIndex).GetAsync(_UriBuilder.Uri);
                }
                Response = await GetHttpClientInstance(_UseProxy, _ProxyIndex).GetAsync(_UriBuilder.Uri);
                break;
            }
            catch (Exception e)
            {
                _LastException = e.Message;
            }
            if (i == MaxRetriesAfterException - 1)
            {
                throw new Exception($"Max retries after exception reached. Exception: {_LastException}");
            }
        }

        if (Response.StatusCode == HttpStatusCode.OK)
            return Response;

        HttpStatusCode _LastStatusCode = HttpStatusCode.OK;
        for (int i = 0; i < MaxRetriesIfStatusNotOk; i++)
        {
            try
            {
                Response = await GetHttpClientInstance(_UseProxy, _ProxyIndex).GetAsync(_UriBuilder.Uri);
                if (Response.StatusCode == HttpStatusCode.OK)
                    return Response;
                _LastStatusCode = Response.StatusCode;
            }
            catch
            {
                
            }
            await Task.Delay(DelayIfStatusIsNotOk);
        }
        throw new Exception($"Max retiries after not ok status reached. Status code: {_LastStatusCode}");
    }

    private HttpClient GetHttpClientInstance(bool _UseProxy, int _ProxyIndex)
    {
        if (Proxys.Count != 0 && _UseProxy)
        {
            WebProxy _Proxy = Proxys[_ProxyIndex];
            if (_ProxyClients.ContainsKey(_Proxy))
                return _ProxyClients[Proxys[_ProxyIndex]];
            HttpClientHandler _Handler = new HttpClientHandler
            {
                Proxy = _Proxy
            };
            HttpClient Client = new HttpClient(_Handler, true);
            Client.Timeout = TimeOut;
            _ProxyClients[_Proxy] = Client;
            return Client;
        }
        return _Client;
    }
}
