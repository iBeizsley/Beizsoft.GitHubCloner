# Beizsoft.GitHubCloner

The application clones all of the repositories (including forks) belonging to the provided user, or all of the repositories belonging to a provided organization which the user has access to.

Repositories are stored either in the directory provided in the first command line argument, or, by default, in 'output' from the working directory the application is run against. The directory structure will look like this:
```
[outputPath]/
   [user]/
       [RepositoryName]
       [RepositoryName]
       [RepositoryName]
   [organization]/
       [RepositoryName]
       [RepositoryName]
       [RepositoryName]
```

If the repository has already been cloned to the provided location, the application will attempt to perform a fetch on origin. If any repo fails to clone or fetch, an error message will be output (and added to the error messages output at completion). The program will attempt to clone or update all repositories it finds, even if some fail.

By default, the application uses the `appsettings.json` file in the executable's directory. Alternatively, the second command line parameter can be used to specify a file path to a different `appsettings.json` file. This enables you to, for example, sync to multiple drives from a single executable, or sync archived repositories or forks to a separate location.

## Appsettings.json

```
{
  "GetRepositortiesForOrganization": null,
  "ApiKey": "TH8TH293GEFUB3F83FB8EGB",
  "ApiPath": "https://api.github.com",
  "Forked": "NoForkedRepositories",
  "User": "iBeizsley",
  "Archived": 0
}
```

- **GetRepositortiesForOrganization**: `null` for user repositories, or the name of the organization to get repositories for (eg: `"TestOrganization"`).

- **ApiKey**: A personal access token for GitHub with access to all of the repositories you want to be able to clone.

- **ApiPath**: The path to the GitHub API used to enumerate repositories.

- **Archived**: An enum to define whether archived (or unarchived) repositories should be included.

  - "All" or 0
  - "NoArchivedRepositories" or 1
  - "OnlyArchivedRepositories" or 2

- **Forked**: As above, an enum to define whether forked repositories should be included.

  - "All" or 0
  - "NoForkedRepositories" or 1
  - "OnlyForkedRepositories" or 2

- **User**: The user to authenticate as. If no `GetRepositoriesForOrganization` is specified, this will be the user whose repositories are pulled.