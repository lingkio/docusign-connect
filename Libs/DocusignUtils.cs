using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Docusign_Connect.Constants;
using Docusign_Connect.DTO;

namespace Docusign_Connect.Libs
{
    public static class DocusignUtils
    {
        public static string GetClaimsByType(this IEnumerable<Claim> Claims, string claimType)
        {
            return Claims.FirstOrDefault((claim) =>
                                        {
                                            return claim.Type == LingkConst.ClaimsUrl + claimType;
                                        }).Value;
        }
        public static Tab GetSelectedTab(this Envelope selectedEnvelope, string label)
        {
            return selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
                           {
                               return tabsInYaml.Id == label;
                           });
        }
    }
}
