using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using Docusign_Connect.DTO;
using Microsoft.AspNetCore.Rewrite;
using Docusign_Connect.Middleware;
using Docusign_Connect.Constants;
using Docusign_Connect.Libs;

namespace Docusign_Connect
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            if (Configuration["ASPNETCORE_YAML_CONFIG"] == null)
            {
                throw new Exception("ASPNETCORE_YAML_CONFIG need to be passed");
            }
            var builder = new ConfigurationBuilder()
                   .SetBasePath(env.ContentRootPath)
                   .AddYamlFile(Configuration["ASPNETCORE_YAML_CONFIG"], optional: false)
                   .AddEnvironmentVariables();
            Configuration = builder.Build();
            
            var lingkConfig = new LingkConfig();
            Configuration.Bind(lingkConfig);
            LingkYaml.LingkYamlConfig = lingkConfig;
            LingkFile.Create("");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.Configure<LingkConfig>(Configuration);

            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                saml2Configuration.AllowedAudienceUris.Add(this.Configuration["authn:saml:issuerId"]);

                var entityDescriptor = new EntityDescriptor();
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
            //enable session before MVC
            app.UseSession();
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
            app.UseMiddleware<RedirecterMiddleware>();
            var options = new RewriteOptions()
            .AddRedirect("(.*)", "auth/login");

            app.UseRewriter(options);
        }

    }

}
