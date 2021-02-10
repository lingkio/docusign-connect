using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using Docusign_Connect.DTO;
using DocuSign.eSign.Client;
using Microsoft.AspNetCore.Http;
using Docusign_Connect.Constants;
using Docusign_Connect.Libs;

namespace Docusign_Connect.Pages
{
    [Authorize]
    public class DocusignModel : PageModel
    {
        public string DocusignUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string Name { get; set; }
        private string accountId;
        private string templateId;
        public string PresentAddress { get; set; }
        public string Upn { get; set; }
        private readonly string signerClientId = "1000";
        private readonly ILogger<DocusignModel> _logger;
        private Docusign_Connect.DTO.Envelope selectedEnvelope;
        private LingkCredentials lingkCredentials;
        private string location;
        private ApiClient apiClient;
        Dictionary<Type, int> typeDict;
        public DocusignModel(ILogger<DocusignModel> logger)
        {
            _logger = logger;
            typeDict = new Dictionary<Type, int>
            {
                {typeof(List<Text>),0},
                {typeof(List<SignHere>),1},
                {typeof(List<Checkbox>),2},
                {typeof(List<RadioGroup>),3},
                {typeof(List<List>),4}
            };
        }

        public void OnGet()
        {
            location = HttpContext.Session.GetString(LingkConst.SessionKey);
            if (location == null)
            {
                ErrorMessage = "Please correct the url to create the envelope eg. /adddrop";
                return;
            }
            selectedEnvelope = LingkYaml.LingkYamlConfig.Envelopes.Where(env =>
               env.Url.ToLower() == location.ToLower()
            ).FirstOrDefault();
            if (selectedEnvelope == null)
            {
                ErrorMessage = location + " url does not exists in yaml configuration";
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

            apiClient = new ApiClient(lingkCredentials.credentialsJson.isSandbox ? LingkConst.DocusignDemoUrl : LingkConst.DocusignProdUrl);
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + this.lingkCredentials.docuSignToken.access_token);

            var name = User.Claims.GetClaimsByType("name");//This is to add signer
            var emailAddress = User.Claims.GetClaimsByType("emailaddress");
            DocusignUrl = CreateURL(emailAddress, name, emailAddress, name);
            HttpContext.Session.SetString(LingkConst.SessionKey, "");
            Response.Redirect(DocusignUrl);
        }
        public string GetDataFromPostgres(Tab foundTab)
        {
            if (selectedEnvelope.LinkFromSamlToProvider == null)
            {
                _logger.LogWarning("LinkFromSamlToProvider not found in yaml for envelop url " + location);
                return "";
            }
            var link = selectedEnvelope.LinkFromSamlToProvider.Split("|");
            if (link == null || link.Length != 2)
            {
                _logger.LogWarning("LinkFromSamlToProvider value are not correct for envelop url " + location);
                return "";
            }
            var identifierValue = User.Claims.GetClaimsByType(link[0]);
            if (identifierValue == null)
            {
                _logger.LogWarning("LinkFromSamlToProvider identifierValue not found in Saml claims for envelop url " + location);
                return "";

            }
            var idValue = identifierValue;
            if (identifierValue.Contains('|'))
            {
                var actualValue = identifierValue.Split("|");
                idValue = actualValue[0];
                if (actualValue.Length > 1)
                {
                    idValue = actualValue[1];
                }
            }
            var data = DbConnector.GetDataFromPostgres(foundTab, link[1], idValue);
            if (data == "")
            {
                _logger.LogWarning("No value found for " + foundTab.Provider + " with id " + link[1] + " and value " + idValue + " for field " + foundTab.SourceDataField);
            }
            return data;
        }

        public string GetTabValue(Tab foundTab)
        {
            if (foundTab.SourceDataField == null || foundTab.Id == null)
            {
                _logger.LogWarning("Either Id or SourceDataField is missing in yaml configuration for one of the field in envelope url " + location);
                return "";
            }
            if (foundTab.Provider == null || foundTab.Provider.ToLower() == "saml")
            {
                var result = User.Claims.GetClaimsByType(foundTab.SourceDataField.Trim());
                if (result == "")
                {
                    _logger.LogWarning(foundTab.SourceDataField + " is not implemented for tab " + foundTab.Id);
                }
                return result;
            }
            if (foundTab.Provider.ToLower() == DBType.Postgres.ToString().ToLower())
            {
                return GetDataFromPostgres(foundTab);
            }
            _logger.LogWarning(foundTab.SourceDataField + " is not implemented for tab " + foundTab.Id);
            return "";
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
                 templateId = templateId,
                 envelopePath = location
             });
            return redirectUrl;
        }
        public List<string> GetValidDocuments()
        {
            var templateApi = new TemplatesApi(apiClient);
            TemplateDocumentsResult results = templateApi.ListDocumentsAsync(accountId, templateId).Result;
            if (results != null && results.TemplateDocuments != null)
            {
                return results.TemplateDocuments.Select((doc) =>
                 {
                     return doc.DocumentId;
                 }).ToList();
            }
            throw new Exception("No document found for configuration");
        }
        public Tabs GetValidTabs()
        {
            var docs = GetValidDocuments();
            Tabs tabs = new Tabs();
            List<Text> textTabs = new List<Text>();
            List<Checkbox> CheckboxTabs = new List<Checkbox>();
            List<List> listTabs = new List<List>();
            List<RadioGroup> radioTabs = new List<RadioGroup>();
            docs.ForEach((docId) =>
            {
                var templateApi = new TemplatesApi(apiClient);
                Tabs results = templateApi.GetDocumentTabsAsync(accountId, templateId, docId).Result;

                //Get only those tabs which is set while template creation
                var validTabs = results.GetType()
                                .GetProperties() //get all properties on object
                                .Select(pi => pi.GetValue(results))
                                .Where(value => value != null).ToList(); //get value for the property     

                validTabs.ForEach((tab) =>
                {
                    Type type = tab.GetType();
                    var key = typeDict.ContainsKey(type) ? typeDict[type] : -1;
                    switch (key)
                    {
                        case 0:
                            tabs.TextTabs = GetTextTabs(tab as List<Text>, textTabs);
                            break;
                        case 1:
                            tabs.SignHereTabs = new List<SignHere> { };
                            break;
                        case 2:
                            tabs.CheckboxTabs = GetCheckboxTabs(tab as List<Checkbox>, CheckboxTabs);
                            break;
                        case 3:
                            tabs.RadioGroupTabs = GetRadioGroupTabs(tab as List<RadioGroup>, radioTabs);
                            break;
                        case 4:
                            tabs.ListTabs = GetListTabs(tab as List<List>, listTabs);
                            break;
                        case -1:
                            _logger.LogWarning("Tab " + type.ToString() + " is not implemented");
                            break;
                        default:
                            break;
                    }
                });
            });
            return tabs;
        }
        public List<Text> GetTextTabs(List<Text> existingTab, List<Text> textTabs)
        {
            List<Text> docusignTabs = existingTab;
            docusignTabs.ForEach((docTextTab) =>
           {
               var foundTab = selectedEnvelope.GetSelectedTab(docTextTab.TabLabel);
               if (foundTab != null)
               {
                   textTabs.Add(new Text
                   {
                       TabLabel = foundTab.Id,
                       Value = GetTabValue(foundTab)
                   });
               }
           });
            return textTabs;
        }
        public List<Checkbox> GetCheckboxTabs(List<Checkbox> existingTab, List<Checkbox> CheckboxTabs)
        {
            List<Checkbox> docusignCheckboxTabs = existingTab;
            docusignCheckboxTabs.ForEach((docCheckboxTab) =>
           {
               var foundTab = selectedEnvelope.GetSelectedTab(docCheckboxTab.TabLabel);
               if (foundTab != null)
               {
                   CheckboxTabs.Add(new Checkbox
                   {
                       TabLabel = foundTab.Id,
                       Selected = GetTabValue(foundTab)
                   });
               }
           });
            return CheckboxTabs;
        }
        public List<List> GetListTabs(List<List> existingTab, List<List> listTabs)
        {
            List<List> docusignListTabs = existingTab;
            docusignListTabs.ForEach((docListTab) =>
           {
               var foundTab = selectedEnvelope.GetSelectedTab(docListTab.TabLabel);
               if (foundTab != null)
               {
                   listTabs.Add(new List
                   {
                       TabLabel = foundTab.Id,
                       Value = GetTabValue(foundTab)
                   });
               }
           });
            return listTabs;
        }
        public List<RadioGroup> GetRadioGroupTabs(List<RadioGroup> existingTab, List<RadioGroup> radioTabs)
        {
            List<RadioGroup> docusignRadioTabs = existingTab;
            docusignRadioTabs.ForEach((docRadioTab) =>
           {
               var foundTab = selectedEnvelope.GetSelectedTab(docRadioTab.GroupName);
               if (foundTab != null)
               {
                   radioTabs.Add(new RadioGroup
                   {
                       GroupName = foundTab.Id,
                       Radios = new List<Radio> { new Radio
                                   {
                                       Value = GetTabValue(foundTab),
                                       Selected = "true"
                                    } }
                   });
               }
           });
            return radioTabs;
        }

    }
}