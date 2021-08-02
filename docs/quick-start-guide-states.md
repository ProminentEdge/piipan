# Quickstart Guide for States

> This documentation is for state use.

> ⚠️  Under construction

A high-level view of the system architecture can be found [here](../README.md).

## System Status

APIs are in Alpha stage and under active development. APIs are available for state testing now.

## APIs Overview

In order to participate, states will need to:

1. Upload participant data to the system
1. Conduct matches against the system

These three elements translate to two main API calls that states will integrate into their existing eligibility systems and workflows:

1. States will upload participant data through a scheduled [CSV upload](./openapi/generated/bulk-api/openapi.md#bulk-api-upload) (CSV formatting instructions can be found [here](https://github.com/18F/piipan/blob/main/etl/docs/bulk-import.md))
2. States will conduct matches through [Active Matching](./openapi/generated/duplicate-participation-api/openapi.md#duplicate-participation-api-match)

### Environments

Separate endpoints and credentials will be provided for each environment.

| Environment | Purpose |
|---|---|
| Testing | For initial testing of the integration; fake data only |
| Staging | For testing with actual data at scale; data is not used in production |
| Production | Actual data used in the production system |

### Endpoints Overview

Endpoints are separated into two logical APIs:

#### Bulk upload API

[Detailed documentation](./openapi/generated/bulk-api/openapi.md)

| Endpoint | Description | Request Type |
|---|---|---|
| `/upload/:filename` | uploads bulk participant data to the system | PUT |

#### Duplicate participation API

[Detailed documentation](./openapi/generated/duplicate-participation-api/openapi.md)

| Endpoint | Description | Request Type |
|---|---|---|
| `/query` | query for active matches | POST |

## Authentication

States will be issued API keys that are placed into request headers to authenticate a web service call. The bulk upload API requires a separate API key from the duplicate participation API.

Example using cURL:

```
curl --request PUT '<uri>' --header 'Ocp-Apim-Subscription-Key: <api-key>'
```

## Sample records

To allow States to test the query endpoint, the Piipan test environment currently includes three sample states that are populated from the [example CSV](https://github.com/18F/piipan/blob/main/etl/docs/csv/example.csv).  Queries for any of the individuals in that sample file should result in a match.

## Record retention

Save API responses received from the duplicate participation API for 3 years.

API responses that are used for SNAP eligibility determinations are subject to the requirements of 7 CFR 272.1(f).

## Feedback

Got any feedback for us? We track API issues through [Github Issues](https://github.com/18F/piipan/issues).

We also have a Microsoft Teams channel for daily communication with state agency engineers.
