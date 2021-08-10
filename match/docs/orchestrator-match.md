# Orchestrator matching API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial API for matching PII data across all participating states.

The orchestrator matching API is implemented in the `Piipan.Match.Orchestrator` project and deployed to an Azure Function App.

To query the API:
1. A JSON `POST` request that conforms to the [OpenApi spec](openapi.md) is sent to the orchestrator API endpoint.
1. The `POST` event triggers a function named `Query` in the orchestrator Function App.
    - If the request is unauthorized (does not include a valid bearer token), the function returns a `401` response.
    - If the request is not valid (malformed, missing required data, etc), the function returns a `400` response. Currently no error messaging is included in the response.
    - If the request is valid, the function queries the per-state APIs for matches. A `200` response is returned containing any matching records.
    - If there is an issue connecting to or querying any of the per-state APIs, the orchestrator returns a `500` response.

## Environment variables

The following environment variables are required by the orchestrator and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `StateApiUriStrings` | [details](../../docs/iac.md#\:\~\:text=StateApiUriStrings) |

## Binding to state APIs

The orchestrator treats per-state APIs as backing services. When running the [IaC](../../docs/iac.md):
- Per-state URIs are compiled into a JSON list and saved as an environment variable
- The orchestrator's system-assigned identity is given an authorized application role (which will be checked by the state API upon receiving requests)

At runtime, the app requests an authentication token from the state app's Active Directory application object. The token is then included as an authorization header (`Authorization: Bearer {token}`) in the request sent to the state API. For more detail on this process see [Securing internal APIs](../../docs/securing-internal-apis.md).

## Local development

Local development is achieved by connecting a locally running instance of the orchestrator API to remote instances of the bound resources (per-state APIs). When running locally, `Startup.cs` conditionally configures the `Piipan.Shared.Autentication.AuthorizedJsonApiClient` dependency to use Azure CLI credentials when obtaining access tokens for the per-state APIs. To make use of this functionality:

1. Run `func azure functionapp fetch-app-settings <remote orchestrator name>` to ensure you have up-to-date local settings configured in `local.settings.json`.
1. Run `func settings add DEVELOPMENT true` to add a `"DEVELOPMENT"` setting with a value of `"true"` to `local.settings.json`. This triggers the orchestrator to use your Azure CLI credentials when authenticating with the state APIs.
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `StateApi.Query` role for the remote state Function App(s) and authorize the Azure CLI.

With the orchestrator running locally (`func start` or `dotnet watch msbuild /t:RunFunctions`), any requests to the local endpoint will now use the user account authorized with Azure CLI to obtain access tokens from the per-state APIs.

A true local development approach with locally run instances of the per-state APIs, and participant records database does not yet exist.

### App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Remote testing

To test the orchestrator remotely:
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `OrchestratorApi.Query` role for the remote orchestrator Function App and authorize the Azure CLI.
1. Retrieve a token for your user using the Azure CLI: `az account get-access-token --resource <orchestrator application ID URI>`.
1. Send a request to the remote endpoint—perhaps using a tool like Postman or `curl`—and include the access token in the Authorization header: `Authorization: Bearer {token}`.
