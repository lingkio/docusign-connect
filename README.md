# DocuSign Redirector

### How to run

``docker build -t lingk_redirectore:0.0.1 .``

#### For linux  
  
  
Generate Certificate: 

``dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p { password here }
dotnet dev-certs https --trust``

Run docker image:

``docker run --rm -it -p 3000:80 -p 3002:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="test" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v ${HOME}/.aspnet/https:/https/ lingk_redirectore:0.0.1``

#### For Windows

Generate Certificate: 

``dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p { password here }
dotnet dev-certs https --trust``

Run docker image:

``docker run --rm -it -p 3000:80 -p 3002:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ lingk_redirectore:0.0.1``


# Resources
* Hosting ASP.NET Core images with Docker over HTTPS https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-5.0