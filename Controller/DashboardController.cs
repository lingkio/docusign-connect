using Docusign_Connect.Constants;
using Docusign_Connect.Libs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Docusign_Connect.Controllers
{
    [AllowAnonymous]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        public DashboardController()
        {
        }

        [Route("Delete/{envelopeId}")]
        public IActionResult Delete(string envelopeId = null)
        {
            var Envelopes = LingkFile.ReadEnvelopesFromFileSystem();
            var itemToRemove = Envelopes.Single(e => e.envelopeId == envelopeId);
            Envelopes.Remove(itemToRemove);
            LingkFile.UpdateEnvelopes(Envelopes);
            return Redirect("~/");
        }
    }
}