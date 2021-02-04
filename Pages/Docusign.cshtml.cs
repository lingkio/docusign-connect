using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using Lingk_SAML_Example.DTO;
using Microsoft.Extensions.Options;
using System.Net.Http;
using DocuSign.eSign.Client;
using Lingk_SAML_Example.Utils;
using System.IO;
using Microsoft.AspNetCore.Http;
using Lingk_SAML_Example.LingkFileSystem;
using Lingk_SAML_Example.Constants;
using Lingk_SAML_Example.Libs;
using Lingk_SAML_Example.DatabaseConnectors;

namespace Lingk_SAML_Example.Pages
{
    [Authorize]
    public class DocusignModel : PageModel
    {
        public string Url { get; set; }
        public string ErrorMessage { get; set; }
        public string Name { get; set; }
        private string accountId;
        private string templateId;
        public string PresentAddress { get; set; }
        public string Upn { get; set; }
        private readonly string signerClientId = "1000";
        private readonly ILogger<DocusignModel> _logger;
        private Lingk_SAML_Example.DTO.Envelope selectedEnvelop;
        private LingkCredentials lingkCredentials;
        public DocusignModel(ILogger<DocusignModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var requestedTemplate = HttpContext.Session.GetString(LingkConst.SessionKey);
            if (requestedTemplate == null)
            {
                ErrorMessage = "Please correct the url to create the envelop eg. /adddrop";
                return;
            }
            selectedEnvelop = LingkYaml.LingkYamlConfig.Envelopes.Where(env =>
               env.Url.ToLower() == requestedTemplate.ToLower()
            ).FirstOrDefault();
            if (selectedEnvelop == null)
            {
                ErrorMessage = requestedTemplate + " url does not exists in yaml configuration";
                return;
            }
            this.lingkCredentials = DocusignHelper.GetAccessToken(LingkYaml.LingkYamlConfig.LingkProject);
            if (lingkCredentials.credentialsJson.accountId == null)
            {
                ErrorMessage = "You don't have access for the account " + lingkCredentials.credentialsJson.accountId;
                return;
            }
            ErrorMessage = null;
            accountId = lingkCredentials.credentialsJson.accountId;
            templateId = selectedEnvelop.Template;

            var name = GetClaimsByType("name");//This is to add signer
            var emailAddress = GetClaimsByType("emailaddress");
            Url = CreateURL(emailAddress, name, emailAddress, name);
        }
        public string GetDataFromPostgres(Tab foundTab)
        {
            var link = selectedEnvelop.LinkFromSamlToProvider.Split("|");
            var identifierVallue = GetClaimsByType(link[0]);
            return DbConnector.GetDataFromPostgres(foundTab, link[1], identifierVallue);
        }
        public string GetClaimsByType(string claimType)
        {
            return User.Claims.FirstOrDefault((claim) =>
                           {
                               return claim.Type == LingkConst.ClaimsUrl + claimType;
                           }).Value;
        }
        public string CreateURL(string signerEmail, string signerName, string ccEmail,
         string ccName)
        {
            var lingkEnvelopFilePath = LingkConst.LingkFileSystemPath;
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
            var apiClient = new ApiClient(lingkCredentials.credentialsJson.isSandbox ? LingkConst.DocusignDemoUrl : LingkConst.DocusignProdUrl);
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + this.lingkCredentials.docuSignToken.access_token);

            var envelopeId = existingEnvelopeId;
            var envelopesApi = new EnvelopesApi(apiClient);
            if (envelopeId == null)
            {
                Tabs tabs = GetValidTabs();

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
            viewRequest.ReturnUrl = selectedEnvelop.DocusignReturnUrl;
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
        //TODO: need to add try catch and seprate out the logic
        public Tabs GetValidTabs()
        {
            Tabs tabs = new Tabs();
            var apiClient = new ApiClient(lingkCredentials.credentialsJson.isSandbox ? LingkConst.DocusignDemoUrl : LingkConst.DocusignProdUrl);
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + this.lingkCredentials.docuSignToken.access_token);
            var templateApi = new TemplatesApi(apiClient);
            Tabs results = templateApi.GetDocumentTabsAsync(accountId, templateId, "1").Result;

            var validTabs = results.GetType()
                            .GetProperties() //get all properties on object
                            .Select(pi => pi.GetValue(results))
                            .Where(value => value != null).ToList(); //get value for the propery
            Dictionary<Type, int> typeDict = new Dictionary<Type, int>
            {
                {typeof(List<Text>),0},
                {typeof(List<SignHere>),1}
            };

            validTabs.ForEach((tab) =>
            {
                Type type = tab.GetType();
                var key = typeDict.ContainsKey(type) ? typeDict[type] : -1;
                switch (key)
                {
                    case 0:
                        List<Text> textTabs = new List<Text>();
                        List<Text> docusignTabs = tab as List<Text>;
                        docusignTabs.ForEach((docTextTab) =>
                       {
                           var foundTab = selectedEnvelop.Tabs.FirstOrDefault((tabsInYaml) =>
                            {
                                return tabsInYaml.Id == docTextTab.TabLabel;
                            });
                           if (foundTab != null)
                           {
                               textTabs.Add(new Text
                               {
                                   TabLabel = foundTab.Id,
                                   Value = foundTab.Provider.ToLower() == DBType.Postgres.ToString().ToLower() ?
                                   GetDataFromPostgres(foundTab) :
                                   GetClaimsByType(foundTab.SourceDataField)
                               });
                           }
                       });
                        tabs.TextTabs = textTabs;
                        break;
                    case 1:
                        //TODO: need to verify the below login
                        SignHere signHere = new SignHere
                        {
                            AnchorString = "/sn1/",
                            AnchorUnits = "pixels",
                            AnchorYOffset = "10",
                            AnchorXOffset = "20"
                        };
                        tabs.SignHereTabs = new List<SignHere> { signHere };
                        break;
                    case -1:
                        Console.WriteLine("Tab " + type.ToString() + " is not implemented");
                        break;
                    default:
                        break;
                }
            });
            return tabs;
        }
    }
}