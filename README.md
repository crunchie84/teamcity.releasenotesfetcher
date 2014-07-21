# teamcity.releasenotesfetcher

CommandLine tool to fetch releasenotes based on teamcity buildId and output it to the console. The app will fetch the information from the teamcity rest api with the given credentials

```
Usage: TeamCity.ReleaseNotesFetcher options

   OPTION              TYPE       POSITION   DESCRIPTION
   -TeamCityUrl (-T)   string*    0          The baseurl to your teamcity server from which we are going to download the releasenotes
   -UserName (-U)      string*    1          Your TeamCity username with sufficient rights to download the releasenotes of builds
   -Password (-P)      string*    2          Your Teamcity password [sorry TC does not support OAuth tokens...]
   -BuildId (-B)       integer*   3          The build id of which to create releasenotes

   EXAMPLE: TeamCity.ReleaseNotesFetcher -t http://teamcity.yourcompany.com -u myusername -p mypassword -b 1234
   How to call this program
```

## Output (example)

janedoe - 2014-07-21 21:17 - asd123adfa123asfasfd
this is my commit (oldest)

johndoe - 2014-07-21 22:27 - asd123adfa123asfasfd
this is my commit (little newer)

johndoe - 2014-07-21 23:37 - asa123adfa123asfasfd
this is my commit (newest)