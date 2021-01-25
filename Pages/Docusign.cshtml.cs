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
using DocuSign.eSign.Client;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Lingk_SAML_Example.Pages
{
    [Authorize]
    public class DocusignModel : PageModel
    {
        public string Url { get; set; }
        public string Name { get; set; }

        public string PresentAddress { get; set; }
        public string Upn { get; set; }
        private readonly string signerClientId = "1000";
        private readonly ILogger<DocusignModel> _logger;
        private const string accessToken = "eyJ0eXAiOiJNVCIsImFsZyI6IlJTMjU2Iiwia2lkIjoiNjgxODVmZjEtNGU1MS00Y2U5LWFmMWMtNjg5ODEyMjAzMzE3In0.AQoAAAABAAUABwAAn2cBFMHYSAgAAN-KD1fB2EgCAHQ58JSfLkRLllLTaBqeoVAVAAEAAAAYAAEAAAAFAAAADQAkAAAAYWE1M2RkNDYtMGFkOC00ZjdmLWIxOGItY2Q2MmUwN2IwM2RiIgAkAAAAYWE1M2RkNDYtMGFkOC00ZjdmLWIxOGItY2Q2MmUwN2IwM2RiMACA9KiOEMHYSDcAcvvK3LOB60yjeEg6qyqkvg.yzQ5VSV_cVFPomaPdVO-8Hmf6fSJvvhH1gSen9P7_-kTsFpA93AMkpFbOTJqC3w5aggKniBJs7_5p3rnQ6LS5YvWC_hJOuOT-evtazUmP16rsrH4LUrhHREW7Iz_LnInPCj8kj3RCiGHO-dw7c2JX642vsIBZd4Zpj1-EM1D2hJr1xOq3sLD9aJx2JfkN6Dpw1ghPrA5pb0QOLRvPpqSLhTV28liNhPftuc_IVdd0hmkV6CjzEFXw2r7F6V2zpQS5spp5xKyFWUmiPnX3pPBf6pXF3X_yvCHvZaXhktwU9phbXmxpHcAIZi4viZmi40Hh3m2SiyaxeprX8_M86Nymg";
        public DocusignModel(ILogger<DocusignModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            foreach (Claim claim in User.Claims)
            {
                if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                {
                    Name = claim.Value;
                }
                if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                {
                    Upn = claim.Value;
                }
                if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                {
                    PresentAddress = claim.Value;
                }
            }
            var url = CreateURL("yachikaralhan49@gmail.com", "Yachika Ralhan", "yachikaralhan49@gmail.com",
            "Yachika Ralhan", "d883b52c-5b94-41dc-8da5-546fb193e120", "aecbc359-1111-4e81-9823-1a3d08d9a221");
            Url = url;
        }

        public string CreateURL(string signerEmail, string signerName, string ccEmail,
         string ccName, string templateId, string accountId)
        {

            var apiClient = new ApiClient("https://demo.docusign.net/restapi/");
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);


            Text includedOnTemplate = new Text
            {
                TabLabel = "permAddress",
                Value = Upn
            };
            Text presentAddressTemplate = new Text
            {
                TabLabel = "presentAddress",
                Value = PresentAddress
            };

            // We can also add a new tab (field) to the ones already in the template
            Text addedField = new Text
            {
                DocumentId = "1",
                PageNumber = "1",
                Font = "helvetica",
                FontSize = "size14",
                TabLabel = "nameLabel",
                Height = "23",
                Width = "84",
                Required = "false",
                Bold = "true",
                Value = Name,
                Locked = "false"
            };

            // Add the tabs model (including the SignHere tab) to the signer.
            // The Tabs object wants arrays of the different field/tab types
            // Tabs are set per recipient/signer
            Tabs tabs = new Tabs
            {
                TextTabs = new List<Text> { includedOnTemplate, addedField, presentAddressTemplate },
            };

            // Create a signer recipient to sign the document, identified by name and email
            // We're setting the parameters via the object creation
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
                // Add the TemplateRole objects to utilize a pre-defined
                // document and signing/routing order on an envelope.
                // Template role names need to match what is available on
                // the correlated templateID or else an error will occur
                TemplateRoles = new List<TemplateRole> { signer, cc }
            };

            // Step 5: Call the eSignature REST API
            var envelopesApi = new EnvelopesApi(apiClient);
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeAttributes);

            // Step 6: Create the View Request
            var envelopeId = results.EnvelopeId;
            RecipientViewRequest viewRequest = new RecipientViewRequest();
            // Set the URL where you want the recipient to go once they are done signing;
            // this should typically be a callback route somewhere in your app.
            // The query parameter is included as an example of how
            // to save/recover state information during the redirect to
            // the DocuSign signing ceremony. It's usually better to use
            // the session mechanism of your web framework. Query parameters
            // can be changed/spoofed very easily
            viewRequest.ReturnUrl = "https://localhost:3002?state=123";

            // How has your app authenticated the user? In addition to your app's authentication,
            // you can include authentication steps from DocuSign; e.g., SMS authentication
            viewRequest.AuthenticationMethod = "none";

            // Recipient information must match the embedded recipient info
            // that we used to create the envelope
            viewRequest.Email = signerEmail;
            viewRequest.UserName = signerName;
            viewRequest.ClientUserId = signerClientId;

            // DocuSign recommends that you redirect to DocuSign for the
            // signing ceremony. There are multiple ways to save state.
            // To maintain your application's session, use the PingUrl
            // parameter. It causes the DocuSign Signing Ceremony web page
            // (not the DocuSign server) to send pings via AJAX to your app
            // seconds
            // NOTE: The pings will only be sent if the pingUrl is an HTTPS address

            ViewUrl results1 = envelopesApi.CreateRecipientView(accountId, results.EnvelopeId, viewRequest);
            //***********
            // Don't use an iframe with embedded signing requests!
            //***********
            // State can be stored/recovered using the framework's session or a
            // query parameter on the return URL (see the makeRecipientViewRequest method)
            string redirectUrl = results1.Url;
            return redirectUrl;
        }
    }
}