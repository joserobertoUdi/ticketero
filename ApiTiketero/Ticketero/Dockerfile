FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Ticketero.Api/Ticketero.Api.csproj"
WORKDIR "/src/src/Ticketero.Api"
RUN dotnet build "Ticketero.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ticketero.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ticketero.Api.dll"]