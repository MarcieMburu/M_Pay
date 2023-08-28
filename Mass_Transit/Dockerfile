FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Mass_Transit.csproj", "Mass_Transit/"]
RUN dotnet restore "Mass_Transit/Mass_Transit.csproj"

COPY . Mass_Transit/.
RUN dotnet publish -c Release -o /app Mass_Transit/Mass_Transit.csproj 

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Mass_Transit.dll"]