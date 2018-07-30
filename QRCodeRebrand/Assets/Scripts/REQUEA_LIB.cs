#if !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using UnityEngine;





/// <summary>
/// CONTRIBUTION : Due to the fact taht the NuGet package for Highcharts export client is not compatible, a part of its source code was reused and adapt here.
/// Check https://github.com/sochix/highcharts-export-client/blob/master/ for more details.
/// </summary>

public class REQUEA_LIB : MonoBehaviour
{

    /* ///////// DOCUMENTATION \\\\\\\\\\\
     This is the library done in the context of QRCode reading from HoloLens.
     It directly exploit the APIService in Dynapage, also done in the same context.
     It also reuse the JSon library from Unity in order to serialize most of it's data.         
    */


    //Global variable
#region
    private HttpClientHandler handler = new HttpClientHandler();
    private HttpClient client = new HttpClient();
    private HighchartsClient HighchartsClient = null;
    private HighchartsSetting HighchartsSettings = null;

    // Roads
    private string ServerURL = "http://10.9.30.57:8080";
    private string HighchartsUrl = "http://export.highcharts.com";
    private string EntitiesURL = "/dysoweb/rest/iotAPIService";
    private string GlobalInformationServices = "/GetDeviceGlobalInformationJSON";

    //This point is a bit tricky : 
    //https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication
    //In fact, it's the user and the password of the Dysoweb plateform put like this : username:password , encode in 64-base. 
    //Put it in the field Authentification in header in order to be authenticate on api call.
    //Should be change depending the auth on the API's hoster and define acroos all the API usage
    private static string username = "RequeaDev";
    private static string password = "Mandoralen38";
    private string KeyAccess = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
    #endregion

    //Local API private variable (here for cache request implementation)
    #region
    private static int MAXCACHESIZE = 10;
    private static DateTime MAXCACHEDURATION;
    private Dictionary<string,Dictionary<DateTime,string>> cache = new Dictionary<string, Dictionary<DateTime, string>>();
#endregion

    //Network and HTTP request tools
#region

    private async Task<Sensor> SensorResponse(WebRequest adress)
    {

        adress.Credentials = new NetworkCredential(username, password);
        adress.ContentType = "application/json";
        adress.Method = "GET";
        try
        {
            var response = await adress.GetResponseAsync();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return JObject.Parse(result).ToObject<Sensor>();
            }
        }
        catch (HttpRequestException e)
        {
            print(e.InnerException.Message);
            return null;
        }


    }

    private async Task<JObject> JSONResponse(WebRequest adress)
    {
        adress.Credentials = new NetworkCredential(username, password);
        adress.ContentType = "application/json";
        adress.Method = "GET";
        try
        {
            var response = await adress.GetResponseAsync();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return JObject.Parse(result);
            }
        }
        catch (HttpRequestException e)
        {
            print(e.InnerException.Message);
            return null;
        }

    }

    private async Task<string> RawStringResponse(HttpWebRequest adress)
    {

        adress.Credentials = new NetworkCredential(username, password);
        adress.ContentType = "application/json";
        adress.Method = "GET";
        try
        {
            var response = await adress.GetResponseAsync();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
        catch (HttpRequestException e)
        {
            print(e.InnerException.Message);
            return null;
        }

    }

    private async Task<Texture2D> DownloadImageAsTexture(string adress)
    {

        Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        Action<Texture2D> resultAction = (texture) => { tex = texture; };
       
        return tex;

    }

    private async Task<Texture2D> DownloadHighchartsAsTexture(string data)
    {
        //Init have to be done there and not in Start function due to the fact that the LIB object doesn't exisst as Game Object (so, Start is never called)
        if (HighchartsClient == null && HighchartsSettings == null)
        {
            HighchartsSettings = new HighchartsSetting();
            HighchartsSettings.IsAsyncCall = true;
            HighchartsSettings.ServerAddress = HighchartsUrl;
            HighchartsClient = new HighchartsClient(HighchartsSettings);
            print("Highcharts init in Requea lib done");

        }
        Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        try
        {
            string link = await HighchartsClient.GetChartImageLinkFromOptionsAsync(data);
            print(link);
            
            using (WWW www = new WWW(link))
            {
                while (!www.isDone)
                {
                    //Wait... I know it's an horrific solution but the only one working after 50+ different test. No waiter, getter, or observable allowed on Unity classes, and cannot make yield work correctly their
                }

                www.LoadImageIntoTexture(tex);
            }
            
            
        }
        catch (Exception e)
        {
            print("exception in api highchart call :" + e.Message);
        }
        
        return tex;
    }
    



#endregion

    //Data process tools
#region
    private string HighcartParserForACaptor(Sensor s, int captor)


    {
        string parser = "";
        foreach (Measure m in s.result[captor].measures)
        {
            if (parser == "")
                parser += "[" + m.date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + "," + m.value + "]";
            else
                parser += ",[" + m.date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + "," + m.value + "]";
        }

        string dataGraph = "{ title : { text : \"" + s.result[captor].sensor_title + "(" + s.result[captor].measure_title + ")\" }, xAxis : { type : \"datetime\", min :  " + s.result[captor].measures[0].date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + " ,max : " + s.result[captor].measures[s.result[captor].measures.Length - 1].date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + "  }, yAxis : { title : { text : \"Value\" } }, series : [{ name : \"" + s.result[captor].measure_title + "\", color : \"#4286f4\", type : \"spline\", data : [" + parser + "] }] }";
        JObject test = JObject.Parse(dataGraph);
        dataGraph = test.ToString();
        return dataGraph;
    }
#endregion

//Public calls
#region
public async Task<Sensor> GetSensorResponse(string s)
    {
        HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ServerURL + EntitiesURL + GlobalInformationServices + "/" + s);
        return await SensorResponse(httpWebRequest);

    }
    public async Task<JObject> GetJSONResponse(string s)
    {
        HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ServerURL + EntitiesURL + GlobalInformationServices + "/" + s);
        return await JSONResponse(httpWebRequest);

    }
    public async Task<string> GetRawStringResponse(string s)
    {
        HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ServerURL + EntitiesURL + GlobalInformationServices + "/" + s);
        return await RawStringResponse(httpWebRequest);
    }
    public async Task<Texture2D> GetImageFromUrl(string url)
    {
        return await DownloadImageAsTexture(url);
    }
    public async Task<Texture2D> GetChartsFromData(string data)
    {
        return await DownloadHighchartsAsTexture(data);
    }
    public async Task<Texture2D> GetChartsFromSensorOnSpecifiedCaptor(Sensor s, int captor)
    {
        return await DownloadHighchartsAsTexture(HighcartParserForACaptor(s, captor));
    }
#endregion

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}


/// <summary>
/// Highcharts client code, taken from the Git repo and adapt a bit
/// </summary>
#region

public interface IHighchartsClient
{
    Task<byte[]> GetChartImageFromOptionsAsync(string options);

    Task<string> GetChartImageLinkFromOptionsAsync(string options);

}

public class HighchartsSetting
{
    /// <summary>
    /// Address of server where installed highcharts-export module. 
    /// http://www.highcharts.com/docs/export-module/setting-up-the-server
    /// </summary>
    public string ServerAddress { get; set; }

    /// <summary>
    /// The content-type of the file to output. 
    /// Can be one of 'image/png', 'image/jpeg', 'application/pdf', or 'image/svg+xml'.
    /// </summary>
    public string ExportImageType { get; set; }

    /// <summary>
    /// Set the exact pixel width of the exported image or pdf.
    /// This overrides the -scale parameter. The maximum allowed width is 2000px
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    /// To scale the original SVG. 
    /// For example, if the chart.width option in the chart configuration is set to 600 and the scale is set to 2,
    /// the output raster image will have a pixel width of 1200. 
    /// So this is a convenient way of increasing the resolution without decreasing the font size and line widths in the chart.
    /// This is ignored if the -width parameter is set. 
    /// For now we allow a maximum scaling of 4. This is for ensuring a good repsonse time. 
    /// Scaling is a bit resource intensive.
    /// </summary>
    public int ScaleFactor { get; set; }

    /// <summary>
    /// The constructor name. Can be one of 'Chart' or 'StockChart'.
    /// This depends on whether you want to generate Highstock or basic Highcharts.
    /// Only applicable when using this in combination with the options parameter.
    /// </summary>
    public string ConstructorName { get; set; }

    /// <summary>
    /// Can be of true or false. Default is false.
    /// When setting async to true a download link is returned to the client, instead of an image.
    /// This download link can be reused for 30 seconds. After that, the file will be deleted from the server.
    /// </summary>
    public bool IsAsyncCall { get; set; }

    public HighchartsSetting()
    {
        ImageWidth = 2000;
        ExportImageType = "png";
        ScaleFactor = 1;
    }
}

public class HighchartsClient : MonoBehaviour, IHighchartsClient, IDisposable
{
    private readonly HighchartsSetting _settings;
    private readonly HttpClient _httpClient;

    public HighchartsClient(string serverAddress)
    {
        _httpClient = new HttpClient();
        _settings = new HighchartsSetting
        {
            ServerAddress = serverAddress
        };
    }

    public HighchartsClient(HighchartsSetting settings)
    {
        _httpClient = new HttpClient();
        _settings = settings;
    }

    private async Task<HttpResponseMessage> MakeRequest(Dictionary<string, string> settings)
    {
        var response = await _httpClient
            .PostAsync(_settings.ServerAddress, new FormUrlEncodedContent(settings))
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return response;
    }

    private Dictionary<string, string> GetRequestSettings(string data, bool getLink, bool isSvg)
    {
        return new Dictionary<string, string>()
            {
                { "async", getLink ? "true" : "false"},
                { "type", _settings.ExportImageType},
                { "width", _settings.ImageWidth.ToString() },
                { "options", data },
                { "scaleFactor", _settings.ScaleFactor.ToString() }
            };
    }

    public async Task<byte[]> GetChartImageFromOptionsAsync(string options)
    {
        var request = GetRequestSettings(options, getLink: false, isSvg: false);
        var response = await MakeRequest(request).ConfigureAwait(false);

        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    public async Task<string> GetChartImageLinkFromOptionsAsync(string options)
    {

        var request = GetRequestSettings(options, getLink: true, isSvg: false);

        var response = await MakeRequest(request).ConfigureAwait(false);
        var filePath = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        print(filePath);
        return _settings.ServerAddress + "/" + filePath;
    }



    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
#endregion




#endif