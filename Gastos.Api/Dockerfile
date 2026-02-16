FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./Gastos.Api/Gastos.Api.csproj
RUN dotnet publish ./Gastos.Api/Gastos.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5124
EXPOSE 5124
ENTRYPOINT ["dotnet", "Gastos.Api.dll"]
