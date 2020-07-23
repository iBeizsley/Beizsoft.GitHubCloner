# Beizsoft.GitHubCloner

The application clones all of the repositories (including forks) belonging to the provided user, or all of the repositories belonging to a provided organization which the user has access to. There is a flag to ignore archived repositories.

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
