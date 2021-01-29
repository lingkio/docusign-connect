using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using System.Text;
using System.Web;
using System.Net.Http.Headers;
using Lingk_SAML_Example.Common;
using Microsoft.Extensions.Options;
using System.Net.Http;
using DocuSign.eSign.Client;
using Lingk_SAML_Example.Utils;
using System.IO;
namespace Lingk_SAML_Example.Pages
{
    [Authorize]
    public class DocusignModel : PageModel
    {
        public string ClaimUrl = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
        public string Url { get; set; }
        public string Name { get; set; }

        public string PresentAddress { get; set; }
        public string Upn { get; set; }
        private readonly string signerClientId = "1000";
        private readonly ILogger<DocusignModel> _logger;
        private readonly LingkConfig _lingkConfig;
        public DocusignModel(ILogger<DocusignModel> logger, IOptions<LingkConfig> lingkConfig)
        {
            _logger = logger;
            _lingkConfig = lingkConfig.Value;
        }

        public void OnGet()
        {
            var name = GetClaimsByType("name");
            var emailAddress = GetClaimsByType("emailaddress");
            Url = CreateURL(emailAddress, name, emailAddress, name);
        }

        public string GetClaimsByType(string claimType)
        {
            return User.Claims.FirstOrDefault((claim) =>
            {
                return claim.Type == ClaimUrl + claimType;
            }).Value;
        }
        public string CreateURL(string signerEmail, string signerName, string ccEmail,
         string ccName)
        {
            var accountId = _lingkConfig.Envelopes[0].Account;
            var templateId = _lingkConfig.Envelopes[0].Template;
            var lingkEnvelopFilePath = AppDomain.CurrentDomain.BaseDirectory + "./lingkEnvelop.json";
            var envResp = LingkFile.CheckEnvelopExists(lingkEnvelopFilePath,
              new LingkEnvelop
              {
                  accountId = accountId,
                  templateId = templateId
              });
            if (envResp != null)
            {
                return envResp.recipientUrl;
            }
            string existingEnvelopeId = null;
            var apiClient = new ApiClient("https://demo.docusign.net/restapi/");
            var accessToken = DocusignHelper.GetAccessToken(_lingkConfig.DocusignCrd.PrivateKey);
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            var envelopeId = existingEnvelopeId;
            var envelopesApi = new EnvelopesApi(apiClient);
            if (envelopeId == null)
            {
                List<Text> textTabs = new List<Text>();
                for (int i = 0; i < _lingkConfig.Envelopes[0].Tabs.Length; i++)
                {
                    textTabs.Add(new Text
                    {
                        TabLabel = _lingkConfig.Envelopes[0].Tabs[i].Id,
                        Value = GetClaimsByType(_lingkConfig.Envelopes[0].Tabs[i].SourceDataField)
                    });
                };


                Tabs tabs = new Tabs
                {
                    TextTabs = textTabs,
                };

                TemplateRole signer = new TemplateRole
                {
                    Email = signerEmail,
                    Name = signerName,
                    RoleName = "signer",
                    ClientUserId = signerClientId, // Change the signer to be embedded
                    Tabs = tabs //Set tab values
                };

                TemplateRole cc = new TemplateRole
                {
                    Email = ccEmail,
                    Name = ccName,
                    RoleName = "cc"
                };

                // Step 4: Create the envelope definition
                EnvelopeDefinition envelopeAttributes = new EnvelopeDefinition
                {
                    // Uses the template ID received from example 08
                    TemplateId = templateId,

                    Status = "Sent",
                    TemplateRoles = new List<TemplateRole> { signer, cc }
                };

                EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeAttributes);
                envelopeId = results.EnvelopeId;
            };

            RecipientViewRequest viewRequest = new RecipientViewRequest();
            viewRequest.ReturnUrl = "https://localhost:3002?state=123";
            viewRequest.AuthenticationMethod = "none";
            viewRequest.Email = signerEmail;
            viewRequest.UserName = signerName;
            viewRequest.ClientUserId = signerClientId;

            ViewUrl results1 = envelopesApi.CreateRecipientView(accountId, envelopeId, viewRequest);

            string redirectUrl = results1.Url;
            LingkFile.AddDocusignEnvelop(lingkEnvelopFilePath,
             new LingkEnvelop
             {
                 envelopId = envelopeId,
                 accountId = accountId,
                 recipientUrl = redirectUrl,
                 templateId = templateId
             });
            return redirectUrl;
        }
    }
}