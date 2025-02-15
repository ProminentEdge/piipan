# Query Tool application

## Prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [npm](https://npmjs.com)

## Local development
To run the app locally:
1. Install the .NET Core SDK development certificate so the app will load over HTTPS ([details](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-3.1&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)):
```
    dotnet dev-certs https --trust
```

2. Set the `OrchApiUri` environment variable to a local or remote instance of `Piipan.Match.Orchestrator`:
```
export OrchApiUri=https://tts-func-orchestrator-dev.azurewebsites.net/api/v1/
```

3. If using a remote orchestrator URI, follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `OrchestratorApi.Query` role for the remote orchestrator Function App and authorize the Azure CLI.

4. Run the app using the `dotnet run` CLI command:
```
    cd query-tool/src/Piipan.QueryTool
    dotnet run
```
Alternatively, use the `watch` command to update the app upon file changes:
```
    cd query-tool/src/Piipan.QueryTool
    dotnet watch run
```

5. Visit https://localhost:5001


### Building Assets

The app's UI uses [USWDS](https://designsystem.digital.gov/), which necessitates a dependency on NPM. If you make changes to the app's SCSS, you'll need to install USWDS (and related dependencies), by running:
```
    cd query-tool/src/Piipan.QueryTool
    npm install
```

After installing the dependencies but before making any changes to the SCSS (in `query-tool/src/Piipan.QueryTool/wwwroot/sass/`), run:
```
    npx gulp watch
```

Gulp will then watch for changes to the SCSS and compile them into the main CSS file.

[Instructions for updating Node dependencies](../../docs/node.md)

## Testing

Tests will be run on the continuous integration server, but
to manually run tests locally use the `dotnet test` command:
```
    cd query-tool/tests/Piipan.QueryTool.Tests
    dotnet test
```

## Deployment

The app is deployed from [CircleCI](https://app.circleci.com/pipelines/github/18F/piipan) upon updates to `main`.

Upon deployment, the app can be viewed via a trusted network at `https://<app_name>.azurewebsites.net/`. Currently, only the GSA network block is trusted.

### Deployment approach

The app is deployed from CircleCI using the [ZIP deployment method](https://docs.microsoft.com/en-us/azure/app-service/deploy-zip). The app is first built using `dotnet publish -o <build_directory>` and the output is zipped using `cd <build_directory> && zip -r <build_name>.zip .`. Note: the zip file contains the *contents* of the output directory but not the directory itself.

The zip file is then pushed to Azure:

```
    az webapp deployment source config-zip -g <resource_group> -n <app_name> --src <path_to_build>.zip
```
