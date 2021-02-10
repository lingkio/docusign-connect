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
            var result = Claims.FirstOrDefault((claim) =>
                                        {
                                            return claim.Type == LingkConst.ClaimsUrl + claimType ||
                                           claim.Type == LingkConst.ClaimsUrlWithYear + claimType ||
                                           claim.Type == LingkConst.IdentitiesDefault + claimType ||
                                            claim.Type == LingkConst.SamlClaimBaseUrl + claimType;
                                        });
            if (result != null && result.Value != null)
            {
                return result.Value;
            }
            return "";
        }
        public static Tab GetSelectedTab(this Envelope selectedEnvelope, string label)
        {
            return selectedEnvelope.Tabs.FirstOrDefault((tabsInYaml) =>
                           {
                               return tabsInYaml.Id.Trim() == label.Trim();
                           });
        }
    }
}
