using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lingk_SAML_Example.Identity;
using Microsoft.Extensions.Options;
using System.Security.Authentication;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using RestSharp;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Lingk_SAML_Example.Pages;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using Lingk_SAML_Example.DTO;

namespace Lingk_SAML_Example.Controllers
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration config;
        private readonly LingkConfig _lingkConfig;
        // public static IConfiguration Configuration { get; }
        public AuthController(IOptions<Saml2Configuration> configAccessor, IOptions<LingkConfig> lingkConfig)
        {
            config = configAccessor.Value;
            config.SignatureAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            config.CertificateValidationMode = X509CertificateValidationMode.None;
            config.RevocationMode = X509RevocationMode.NoCheck;
            _lingkConfig = lingkConfig.Value;
        }

        [Route("SamlRegistration")]
        public IActionResult SamlRegistration()
        {
            var apiClient = new HttpClient();
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("client_id", _lingkConfig.LingkProject.ClientId));
            nvc.Add(new KeyValuePair<string, string>("client_secret", _lingkConfig.LingkProject.ClientSecret));
            nvc.Add(new KeyValuePair<string, string>("audience", "https://lingk-int.auth0.com/api/v2/"));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            var req = new HttpRequestMessage(HttpMethod.Post, "https://lingk-int.auth0.com/oauth/token") { Content = new FormUrlEncodedContent(nvc) };

            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = apiClient.SendAsync(req).Result;

            var json = response.Content.ReadAsStringAsync().Result;
            var token = JsonConvert.DeserializeObject<Token>(json);
            if (response != null)
            {
                var client = new RestClient("https://lingk-int.auth0.com/api/v2/connections");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + token.access_token);
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/json", "{ \"strategy\": \"samlp\", \"name\": \"Lingk-SAML-TEST\", \"enabled_clients\":[\"ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn\"], \"options\": { \"metadataXml\": \"<EntityDescriptor xmlns='urn:oasis:names:tc:SAML:2.0:metadata' entityID='urn:lingk-int.auth0.com'><IDPSSODescriptor protocolSupportEnumeration='urn:oasis:names:tc:SAML:2.0:protocol'><KeyDescriptor use='signing'><KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'><X509Data><X509Certificate>MIIDDzCCAfegAwIBAgIJAPRS4gVfsx2pMA0GCSqGSIb3DQEBCwUAMB4xHDAaBgNVBAMME2xpbmdrLWludC5hdXRoMC5jb20wHhcNMTUwOTE5MTAyODUwWhcNMjkwNTI4MTAyODUwWjAeMRwwGgYDVQQDDBNsaW5nay1pbnQuYXV0aDAuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtGGWMi1BrdPmmE9NphrzumUTdceFfyL0RNVVslxivbfqftb7tJtzSpT3aG87jECAtPv8mJLEkrWL1dl66SLMHEZPZMblpmw+Gp6aQlDAybFgmCVCCG39S4GmJiwd/K08js0Z9awTqtOWq5EsWUPYpou5p/ZiA0qbA2MD3Aflss8TGF9h26GZ3NhKLJ3q2cCRuasFklLEBuNPk8YWZmlJFeTRM9krukvJSjM2zL/EcaZKHcJ7h6ZuUz/IHGOLwsDybu34tPsimc7m36c0AA63MsiToXO6BIWbRxbtYoUFLdaRK8xir1lTnyo5PQyu88HDKQKKcFNQWmCWkeXHP5lrAwIDAQABo1AwTjAdBgNVHQ4EFgQUbrXleY7WpgGTvL9V2vPh3VR6OLQwHwYDVR0jBBgwFoAUbrXleY7WpgGTvL9V2vPh3VR6OLQwDAYDVR0TBAUwAwEB/zANBgkqhkiG9w0BAQsFAAOCAQEAqiRAZKt1rnVnDHrc1oFd/US7ywDwMBRmJ5/TlMXRAlvibA/A3Utni2l/ZuhnP2skGeN/RLvEW4PimCdRioU5/u6kxvON6kckN2NOmEFbCqAJbBg+vYAhfSoIcVbi+ABLawzv5go7e3Ctfx9fyh0MnmimAaPDnpxqGxavk61qblOToj669go7oj+/SfpG2Mny9HDNpJiidLaklWOFfKvqpWQU0qfBRQEf852HOYLZe8CmLoogI8AT9HrlufoQOnaSn8SJOmiDbem4S11sdV9mOqP/5Tx+WBm5BrLnAi+TQIvZkxOzq+qM71s/l7zc85k44F5xnXPuKMmABhwmZVAy0g==</X509Certificate></X509Data></KeyInfo></KeyDescriptor><SingleLogoutService Binding='urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect' Location='https://lingk-int.auth0.com/samlp/ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn/logout'/><SingleLogoutService Binding='urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST' Location='https://lingk-int.auth0.com/samlp/ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn/logout'/><NameIDFormat>urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress</NameIDFormat><NameIDFormat>urn:oasis:names:tc:SAML:2.0:nameid-format:persistent</NameIDFormat><NameIDFormat>urn:oasis:names:tc:SAML:2.0:nameid-format:transient</NameIDFormat><SingleSignOnService Binding='urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect' Location='https://lingk-int.auth0.com/samlp/ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn'/><SingleSignOnService Binding='urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST' Location='https://lingk-int.auth0.com/samlp/ZzbpkpYIE0OXMfnOvm3yePjdrW3E7nrn'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='E-Mail Address'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Given Name'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Name'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Surname'/><Attribute xmlns='urn:oasis:names:tc:SAML:2.0:assertion' Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier' NameFormat='urn:oasis:names:tc:SAML:2.0:attrname-format:uri' FriendlyName='Name ID'/></IDPSSODescriptor></EntityDescriptor>\" } }", ParameterType.RequestBody);
                IRestResponse responseData = client.Execute(request);
                return Redirect("~/auth/login");
            }
            return Redirect("~/auth/login");
        }

        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("~/Docusign");
            }
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config)).ToActionResult();
        }

        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }
            binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

            var relayStateQuery = binding.GetRelayStateQuery();
            var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");
            return Redirect(returnUrl);
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return Redirect("~/");
        }

    }
}