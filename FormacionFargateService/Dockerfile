#FROM mcr.microsoft.com/dotnet/sdk:3.1 
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#COPY bin/Debug/netcoreapp3.1/publish/ ./
#ENTRYPOINT ["dotnet", "FormacionFargateService.dll"]

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
EXPOSE 80
COPY bin/Debug/netcoreapp3.1/publish/ ./
ENTRYPOINT ["dotnet", "FormacionFargateService.dll"]