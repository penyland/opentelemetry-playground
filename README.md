# Opentelemetry Playground

This is a playground for OpenTelemetry. It contains a simple ASP.NET Core 8 web application that sends metrics to Prometheus.
Grafana is used to visualize the metrics.

## Prerequisites
- [dotnet 8 SDK preview](https://dotnet.microsoft.com/download/dotnet/8.0)
- [docker](https://www.docker.com/products/docker-desktop)
- [git](https://git-scm.com/downloads)

## Getting started

1. Clone the repository
2. Install or upgrade to the latest dotnet 8 SDK preview using: 'winget install Microsoft.DotNet.SDK.Preview'
3. Build the api and run it locally
4. Publish the api as a docker image

### Build and run the application locally
1. dotnet build
2. dotnet run

### Build and run as a docker image
1. dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer --output .\publish
2. docker run --name api -it --rm -p 5010:8080 -e "ASPNETCORE_ENVIRONMENT=Development" opentelemetryplayground-api:latest
   
### Provisioning prometheus and grafana
1. Run `docker-compose up`
2. Validate that prometheus is running at http://localhost:9090
3. Validate that grafana is running at http://localhost:3000
4. Login to grafana with `admin` and `admin`
5. Browse dashboards and open the 'ASP.NET Core' dashboard