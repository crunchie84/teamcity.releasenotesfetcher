# TeamCity.ReleaseNotesFetcher

CommandLine tool to fetch releasenotes based on teamcity buildId and output it to the console. The app will fetch the information from the teamcity rest api with the given credentials

```
Usage: TeamCity.ReleaseNotesFetcher options

   OPTION                 TYPE       POSITION   DESCRIPTION
   -TeamCityUrl (-T)      string*    0          The baseurl to your teamcity server from which we are going to download the releasenotes
   -UserName (-U)         string*    1          Your TeamCity username with sufficient rights to download the releasenotes of builds
   -Password (-P)         string*    2          Your Teamcity password [sorry TC does not support OAuth tokens...]
   -BuildId (-B)          integer*   3          The build id of which to create releasenotes
   -RepositoryName (-R)   string*    4          The github repo to use for the url building to the commit ids

   EXAMPLE: TeamCity.ReleaseNotesFetcher -T http://teamcity.yourcompany.com -U mytcusername -P mytcpassword -B 1234 -R crunchie84/teamcity.releasenotesfetcher
   When executing this program it will go to the passed teamcity server's REST API to retrieve the pending changes of the given build id and return those to the console in markdown
 format.
```

# Output (example)

## [janedoe - 2014-07-21 21:17](https://github.com/crunchie84/teamcity.releasenotesfetcher/commit/c96e213a1c54d5d0ff47b91c7c163928b4b389c4)

this is my commit (oldest)


## [johndoe - 2014-07-21 22:27](https://github.com/crunchie84/teamcity.releasenotesfetcher/commit/c96e213a1c54d5d0ff47b91c7c163928b4b389c4)

this is my commit (little newer)


## [johndoe - 2014-07-21 23:37](https://github.com/crunchie84/teamcity.releasenotesfetcher/commit/c96e213a1c54d5d0ff47b91c7c163928b4b389c4)

this is my commit (newest)
