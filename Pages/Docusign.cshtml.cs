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
        private Lingk_SAML_Example.DTO.Envelope selectedEnvelope;
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
                ErrorMessage = "Please correct the url to create the envelope eg. /adddrop";
                return;
            }
            selectedEnvelope = LingkYaml.LingkYamlConfig.Envelopes.Where(env =>
               env.Url.ToLower() == requestedTemplate.ToLower()
            ).FirstOrDefault();
            if (selectedEnvelope == null)
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
            templateId = selectedEnvelope.Template;

            var name = GetClaimsByType("name");//This is to add signer
            var emailAddress = GetClaimsByType("emailaddress");
            Url = CreateURL(emailAddress, name, emailAddress, name);
        }
        public string GetDataFromPostgres(Tab foundTab)
        {
            var link = selectedEnvelope.LinkFromSamlToProvider.Split("|");
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
            var lingkEnvelopeFilePath = LingkConst.LingkFileSystemPath;
            var envResp = LingkFile.CheckEnvelopeExists(lingkEnvelopeFilePath,
              new LingkEnvelope
              {
                  accountId = accountId,
                  templateId = templateId
              });
            if (envResp != null)
            {
                return envResp.recipientUrl;
            }

            var apiClient = new ApiClient(lingkCredentials.credentialsJson.isSandbox ? LingkConst.DocusignDemoUrl : LingkConst.DocusignProdUrl);
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + this.lingkCredentials.docuSignToken.access_token);

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

            var envelopesApi = new EnvelopesApi(apiClient);
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeAttributes);

            RecipientViewRequest viewRequest = new RecipientViewRequest();
            viewRequest.ReturnUrl = selectedEnvelope.DocusignReturnUrl;
            viewRequest.AuthenticationMethod = "none";
            viewRequest.Email = signerEmail;
            viewRequest.UserName = signerName;
            viewRequest.ClientUserId = signerClientId;
            var envelopeId = results.EnvelopeId;
            ViewUrl results1 = envelopesApi.CreateRecipientView(accountId, envelopeId, viewRequest);

            string redirectUrl = results1.Url;
            LingkFile.AddDocusignEnvelope(lingkEnvelopeFilePath,
             new LingkEnvelope
             {
                 envelopeId = envelopeId,
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
                {typeof(List<SignHere>),1},
                {typeof(List<Checkbox>),2},
                {typeof(List<RadioGroup>),3},
                {typeof(List<List>),4}
            };
            //TODO: This need to be refactored as there should be batter way of doing this
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
                           var foundTab = selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
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
                        tabs.SignHereTabs = new List<SignHere> { };
                        break;
                    case 2:
                        List<Checkbox> CheckboxTabs = new List<Checkbox>();
                        List<Checkbox> docusignCheckboxTabs = tab as List<Checkbox>;
                        docusignCheckboxTabs.ForEach((docCheckboxTab) =>
                       {
                           var foundTab = selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
                            {
                                return tabsInYaml.Id == docCheckboxTab.TabLabel;
                            });
                           if (foundTab != null)
                           {
                               CheckboxTabs.Add(new Checkbox
                               {
                                   TabLabel = foundTab.Id,
                                   Selected = foundTab.Provider.ToLower() == DBType.Postgres.ToString().ToLower() ?
                                   GetDataFromPostgres(foundTab) :
                                   GetClaimsByType(foundTab.SourceDataField)
                               });
                           }
                       });
                        tabs.CheckboxTabs = CheckboxTabs;
                        break;
                    case 3:
                        List<RadioGroup> radioTabs = new List<RadioGroup>();
                        List<RadioGroup> docusignRadioTabs = tab as List<RadioGroup>;
                        docusignRadioTabs.ForEach((docRadioTab) =>
                       {
                           var foundTab = selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
                            {
                                return tabsInYaml.Id == docRadioTab.GroupName;
                            });
                           if (foundTab != null)
                           {
                               radioTabs.Add(new RadioGroup
                               {
                                   GroupName = foundTab.Id,
                                   Radios = new List<Radio> { new Radio { Value = foundTab.Provider.ToLower() == DBType.Postgres.ToString().ToLower() ?
                                   GetDataFromPostgres(foundTab) :
                                   GetClaimsByType(foundTab.SourceDataField), Selected = "true" } }
                               });
                           }
                       });
                        tabs.RadioGroupTabs = radioTabs;
                        break;
                    case 4:
                        List<List> listTabs = new List<List>();
                        List<List> docusignListTabs = tab as List<List>;
                        docusignListTabs.ForEach((docListTab) =>
                       {
                           var foundTab = selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
                            {
                                return tabsInYaml.Id == docListTab.TabLabel;
                            });
                           if (foundTab != null)
                           {
                               listTabs.Add(new List
                               {
                                   TabLabel = foundTab.Id,
                                   Value = foundTab.Provider.ToLower() == DBType.Postgres.ToString().ToLower() ?
                                   GetDataFromPostgres(foundTab) :
                                   GetClaimsByType(foundTab.SourceDataField) 
                               });
                           }
                       });
                        tabs.ListTabs = listTabs;
                        break;
                    case -1:
                        _logger.LogWarning("Tab " + type.ToString() + " is not implemented");
                        break;
                    default:
                        break;
                }
            });
            return tabs;
        }
    }
}