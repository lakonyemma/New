FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Workwise.Web/Workwise.Web.csproj Workwise.Web/
RUN dotnet restore Workwise.Web/Workwise.Web.csproj
COPY Workwise.Web/ Workwise.Web/
RUN dotnet publish Workwise.Web/Workwise.Web.csproj -c Release -o /out --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Workwise.Web.dll"]