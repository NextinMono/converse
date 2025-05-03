using Octokit;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Converse
{
    internal class UpdateChecker
    {
        public static bool UpdateAvailable = false;
        public static DateTime GetBuildDate()
        {
            var assembly = Assembly.GetEntryAssembly();
            var attribute = assembly
              .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf("+build");
                if (index > 0)
                {
                    value = value[(index + "+build".Length)..];

                    return DateTime.ParseExact(
                        value,
                      "yyyy-MM-ddTHH:mm:ss:fffZ",
                      CultureInfo.InvariantCulture);
                }
            }
            return default;
        }
        public static void CheckUpdate()
        {
            try
            {
                GitHubClient client = new GitHubClient(new Octokit.ProductHeaderValue("kunai-update-checker"));
                var latestRelease = client.Repository.Release.GetLatest("NextinMono", "converse");
                latestRelease.Wait();
                if (latestRelease.Result.PublishedAt > GetBuildDate())
                {
                    UpdateAvailable = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Octokit update checker failed.\n {ex.Message}");
            }
        }
    }
}
