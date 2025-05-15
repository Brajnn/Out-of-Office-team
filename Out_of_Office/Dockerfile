FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app


COPY *.sln .
COPY Out_of_Office.Application/*.csproj ./Out_of_Office.Application/
COPY Out_of_Office.Infrastructure/*.csproj ./Out_of_Office.Infrastructure/
COPY Out_of_Office.Domain/*.csproj ./Out_of_Office.Domain/
COPY Out_of_Office/*.csproj ./Out_of_Office/

RUN dotnet restore

# Kopiowanie reszty plików i publikowanie aplikacji
COPY . .
WORKDIR /app/Out_of_Office
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Out_of_Office.dll"]