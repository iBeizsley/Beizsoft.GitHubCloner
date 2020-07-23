using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Octokit;
using Octokit.Internal;
using ArgumentException = System.ArgumentException;
using Credentials = Octokit.Credentials;
using Repository = LibGit2Sharp.Repository;

namespace Beizsoft.GitHubCloner
{
    internal static class Program
    {
        private static readonly object ConsoleLock = new object();

        private static async Task<int> Main(string[] args)
        {
            var initialColour = Console.ForegroundColor;
            var path = args.Length == 0 ? "./output/" : args[0];
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                WriteLines(ConsoleColor.Red, "Invalid output path specified:", ex.ToString());
                return 1;
            }

            var configuration = new Configuration();
            try
            {
                new ConfigurationBuilder().AddJsonFile(args.Length == 2 ? args[1] : "appsettings.json").Build().Bind(configuration);
                if (string.IsNullOrWhiteSpace(configuration.ApiKey))
                {
                    throw new ArgumentException($"No {nameof(configuration.ApiKey)} specified.", nameof(configuration.ApiKey));
                }

                if (string.IsNullOrWhiteSpace(configuration.User))
                {
                    throw new ArgumentException($"No {nameof(configuration.User)} specified.", nameof(configuration.User));
                }

                if (configuration.ApiPath is null)
                {
                    throw new ArgumentNullException(nameof(configuration.ApiPath), $"No {nameof(configuration.ApiPath)} specified.");
                }
            }
            catch (Exception ex)
            {
                WriteLines(ConsoleColor.Red, "Invalid appsettings.json file:", ex.ToString());
                return 1;
            }

            var client = new GitHubClient(new Connection(
                new ProductHeaderValue(configuration.User),
                configuration.ApiPath,
                new InMemoryCredentialStore(new Credentials(configuration.ApiKey))));

            IReadOnlyDictionary<string, string> repositories;
            try
            {
                var repositoryList = await client.Repository.GetAllForCurrent();
                repositoryList = string.IsNullOrWhiteSpace(configuration.GetRepositortiesForOrganization) ?
                    repositoryList.Where(r => r.Owner.Login == configuration.User).ToArray() :
                    repositoryList.Where(r => r.Owner.Login == configuration.GetRepositortiesForOrganization).ToArray();

                if (!configuration.IncludeArchived)
                {
                    repositoryList = repositoryList.Where(r => !r.Archived).ToArray();
                }

                repositories = repositoryList.ToDictionary(r => r.FullName, r => r.CloneUrl);
            }
            catch (Exception ex)
            {
                WriteLines(ConsoleColor.Red, $"Failed to get repositories for {(string.IsNullOrWhiteSpace(configuration.GetRepositortiesForOrganization) ? configuration.User : configuration.GetRepositortiesForOrganization)}.", ex.Message);
                return 1;
            }

            if (repositories.Count == 0)
            {
                WriteLines(ConsoleColor.Red, "No repositories found.");
                return 1;
            }

            WriteLines(ConsoleColor.Green, $"Found the following {repositories.Count} repositories:", string.Join(Environment.NewLine, repositories.Keys.Select(r => $"    - {r}")), string.Empty);
            LibGit2Sharp.Credentials CredentialsProvider(string _, string __, SupportedCredentialTypes ___) => new UsernamePasswordCredentials
            {
                Username = configuration.ApiKey,
                Password = string.Empty,
            };

            var issues = new Dictionary<string, string>();
            Parallel.ForEach(repositories, r =>
            {
                var (name, url) = r;
                WriteLines(initialColour, $"Checking whether {name} has already been cloned.");
                try
                {
                    var repoPath = Path.Combine(path, name);
                    if (Directory.Exists(Path.Combine(repoPath, ".git")))
                    {
                        WriteLines(initialColour, $"{name} has been cloned; fetching to get the latest changes.");
                        var repository = new Repository(repoPath);
                        Commands.Fetch(
                            repository,
                            "origin",
                            repository.Refs.Select(r => r.CanonicalName),
                            new FetchOptions { CredentialsProvider = CredentialsProvider },
                            null);
                        WriteLines(ConsoleColor.Green, $"{name} was updated successfully.");
                    }
                    else
                    {
                        WriteLines(initialColour, $"{name} has not been cloned; cloning into new folder.");
                        Repository.Clone(
                            url,
                            repoPath,
                            new CloneOptions { CredentialsProvider = CredentialsProvider });
                        WriteLines(ConsoleColor.Green, $"{name} was cloned successfully.");
                    }
                }
                catch (Exception ex)
                {
                    WriteLines(ConsoleColor.Red, $"Failed to clone or update {name}:", ex.ToString());
                    issues[name] = ex.ToString();
                }
            });

            if (issues.Count == 0)
            {
                return 0;
            }

            WriteLines(ConsoleColor.Red, string.Empty, "The following repositories had issues:", string.Join(Environment.NewLine, issues.Select(i => $"{i.Key}: {i.Value}{Environment.NewLine}")));
            return 1;
        }

        private static void WriteLines(ConsoleColor colour, params string[] lines)
        {
            lock (ConsoleLock)
            {
                var originalColour = Console.ForegroundColor;
                Console.ForegroundColor = colour;
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }

                Console.ForegroundColor = originalColour;
            }
        }

        private sealed class Configuration
        {
            public string GetRepositortiesForOrganization { get; set; }

            public bool IncludeArchived { get; set; }

            public string ApiKey { get; set; }

            public string User { get; set; }

            public Uri ApiPath { get; set; }
        }
    }
}
