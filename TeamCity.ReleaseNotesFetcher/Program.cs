using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PowerArgs;

namespace TeamCity.ReleaseNotesFetcher
{
  [ArgExample("TeamCity.ReleaseNotesFetcher -t http://teamcity.yourcompany.com -u myusername -p mypassword -b 1234", "How to call this program")]
  public class ProgramArguments
  {
    /// <summary>
    /// The baseurl to your teamcity server from which we are going to download the releasenotes
    /// </summary>
    [ArgRequired]
    [ArgPosition(0)]
    [ArgDescription("The baseurl to your teamcity server from which we are going to download the releasenotes")]
    [ArgExample("http://teamcity.mycompany.com", "url")]
    public string TeamCityUrl { get; set; }

    /// <summary>
    /// Your TeamCity username 
    /// </summary>
    [ArgRequired]
    [ArgPosition(1)]
    [ArgDescription("Your TeamCity username with sufficient rights to download the releasenotes of builds")]
    public string UserName { get; set; }

    /// <summary>
    /// Your Teamcity password for downloading the releasenotes
    /// </summary>
    [ArgRequired]
    [ArgPosition(2)]
    [ArgDescription("Your Teamcity password [sorry TC does not support OAuth tokens...]")]
    public string Password { get; set; }

    /// <summary>
    /// The buildid of which to generate releasenotes
    /// </summary>
    [ArgRequired]
    [ArgPosition(3)]
    [ArgDescription("The build id of which to create releasenotes")]
    public int BuildId { get; set; }
    
    /// <summary>
    /// The github repo to use for the url building to the commit ids 'crunchie84/teamcity.releasenotesfetcher'
    /// </summary>
    [ArgRequired]
    [ArgPosition(4)]
    [ArgDescription("The github repo to use for the url building to the commit ids")]
    public string RepositoryName { get; set; }
  }

  class Program
  {
    const string BuildChangesRestApiUrl = @"{0}/httpAuth/app/rest/changes?build=id:{1}";
    const string ChangeDetailsRestApiUrl = @"{0}/httpAuth/app/rest/changes/id:{1}";
    static void Main(string[] args)
    {
      try
      {
        var parsedArgs = Args.Parse<ProgramArguments>(args);
        RunAsync(parsedArgs).Wait();
      }
      catch (ArgException ex)
      {
        Console.WriteLine(ex.Message);
        ArgUsage.GetStyledUsage<ProgramArguments>().Write();
      }
    }

    private static async Task RunAsync(ProgramArguments args)
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.BaseAddress = new Uri(args.TeamCityUrl);
        var authInfo = args.UserName + ":" + args.Password;
        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);

        var result = await httpClient.GetAsync(
          string.Format(CultureInfo.InvariantCulture, BuildChangesRestApiUrl, args.TeamCityUrl, args.BuildId));

        if (!result.IsSuccessStatusCode)
        {
          var body = await result.Content.ReadAsStringAsync();
          throw new ApplicationException(string.Format(CultureInfo.InvariantCulture,
            "Could not retrieve changes of build {0}:{1}\r\n{2}", args.BuildId, result.StatusCode, body));
        }

        //load it into xdoc
        var changesXdoc = XDocument.Load(await result.Content.ReadAsStreamAsync());

        var releaseNotes = changesXdoc.Root.Elements("change")
          .Select(el => int.Parse(el.Attribute("id").Value))
          .OrderBy(val => val)
          .Select(async changeId =>
          {
            var changeResponse =
              await
                httpClient.GetAsync(string.Format(CultureInfo.InvariantCulture, ChangeDetailsRestApiUrl,
                  args.TeamCityUrl, changeId));
            changeResponse.EnsureSuccessStatusCode();

            var xdoc = XDocument.Load(await changeResponse.Content.ReadAsStreamAsync());

            var username = xdoc.Root.Attribute("username").Value;
            var commitSha = xdoc.Root.Attribute("version").Value;
            var comment = xdoc.Root.Element("comment").Value;

            //20140717T160900+0200
            var buildDate = xdoc.Root.Attribute("date").Value;
            DateTime? buildDateParsed = null;
            DateTime theDate;
            if (DateTime.TryParseExact(buildDate, @"yyyyMMdd\THHmmsszz\0\0", CultureInfo.InvariantCulture,
              DateTimeStyles.AssumeLocal, out theDate))
            {
              buildDateParsed = theDate;
            }

            return string.Format(CultureInfo.InvariantCulture,
              "## [{0} - {1}](https://github.com/{2}/commit/{3}){5}{5}{4}",
              username, 
              buildDateParsed == null ? "" : buildDateParsed.Value.ToString("dd-MM-yyyy HH:mm:ss"), 
              args.RepositoryName,
              commitSha, 
              comment,
              Environment.NewLine);
          })
          .ToArray();

        Task.WaitAll(releaseNotes);

        Console.Write(string.Join(Environment.NewLine + Environment.NewLine, releaseNotes.Select(t => t.Result)));
      }
    }
  }
}
