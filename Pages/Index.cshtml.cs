using System.Collections.Generic;
using Docusign_Connect.Constants;
using Docusign_Connect.DTO;
using Docusign_Connect.Libs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Docusign_Connect.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<LingkEnvelope> Envelopes;
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var session = HttpContext.Session.GetString(LingkConst.SessionKey);
            if (session != null && session != "")
            {
                Response.Redirect("/Docusign");
            }
            Envelopes = LingkFile.ReadDocusignEnvelopesFromFileSystem(LingkConst.LingkFileSystemPath);
        }
    }
}
