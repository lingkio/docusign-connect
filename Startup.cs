using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using Lingk_SAML_Example.Controllers;
using YamlDotNet.Serialization;
using System.IO;
using Lingk_SAML_Example.Pages;

namespace Lingk_SAML_Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var reader = new StreamReader("./example-docusign.yaml");
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(reader);

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);
            System.IO.File.WriteAllText("./setting.json", json);
            var builder = new ConfigurationBuilder()
          .AddJsonFile("./setting.json");
            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));


            services.Configure<LingkConfig>(Configuration); 
            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                saml2Configuration.AllowedAudienceUris.Add(this.Configuration["authn:saml:issuerId"]);

                var entityDescriptor = new EntityDescriptor();
                //entityDescriptor.ReadIdPSsoDescriptorFromFile(:oasis:names:tc:SAML:2.0:bindings:HTTP-POST' Location='https://lingk-int.auth0.com/samlp/ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='E-Mail Address'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Given Name'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Name'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Surname'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Name ID'/></IDPSSODescriptor></EntityDescriptor>");
                entityDescriptor.ReadIdPSsoDescriptorFromFile(this.Configuration["authn:saml:metadataLocal"]);
                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                    saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                }
                else
                {
                    throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                }
            });
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            services.AddSaml2();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSaml2();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
