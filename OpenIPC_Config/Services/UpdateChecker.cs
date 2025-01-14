using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace OpenIPC_Config.Services;

public class UpdateChecker
{
    private readonly HttpClient _httpClient;
    private readonly string _latestJsonUrl;

    public UpdateChecker(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _latestJsonUrl = configuration["UpdateChecker:LatestJsonUrl"];
    }

    
    public async Task<(bool HasUpdate, string ReleaseNotes, string DownloadUrl, string NewVersion)> CheckForUpdateAsync(string currentVersion)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(_latestJsonUrl);
            var updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(response);

            if (updateInfo != null && IsNewerVersion(updateInfo.Version, currentVersion))
            {
                return (true, updateInfo.ReleaseNotes, updateInfo.DownloadUrl, updateInfo.Version);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error during update check: {ex.Message}");
        }

        return (false, string.Empty, string.Empty, string.Empty);
    }

    private bool IsNewerVersion(string newVersion, string currentVersion)
    {
        // Helper function to remove the "-v" prefix and extract the version number
        string ExtractVersionNumber(string version)
        {
            const string prefix = "v";
            return version.StartsWith(prefix) ? version.Substring(prefix.Length) : version;
        }
        
        // Helper function to remove the "release-v" prefix and extract the version number
        string ExtractGHVersionNumber(string version)
        {
            const string prefix = "release-v";
            return version.StartsWith(prefix) ? version.Substring(prefix.Length) : version;
        }

        // Extract and parse the version numbers
        return Version.TryParse(ExtractGHVersionNumber(newVersion), out var newVer) &&
               Version.TryParse(ExtractVersionNumber(currentVersion), out var currVer) &&
               newVer > currVer;
    }

    public class UpdateInfo
    {
        public string Version { get; set; }
        [JsonProperty("release_notes")]
        public string ReleaseNotes { get; set; }
        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
    }
}
