using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Lingk_SAML_Example.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Lingk_SAML_Example.Utils
{
    public static class LingkFile
    {
        public static void Create(string filePath, string data)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(filePath, System.IO.FileMode.Append)))
            {
                sw.WriteLine(data);
            }
        }
        public static void AddDocusignEnvelop(string filePath, LingkEnvelop envelope)
        {
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelop>>(jsonData) ?? new List<LingkEnvelop>();

            // Add any new employees
            envelopeList.Add(envelope);

            // Update json data string
            jsonData = JsonConvert.SerializeObject(envelopeList);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public static LingkEnvelop CheckEnvelopExists(string filePath, LingkEnvelop envelope)
        {
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelop>>(jsonData) ?? new List<LingkEnvelop>();

            var result = envelopeList.Find((e) =>
            {
                return e.accountId == envelope.accountId && e.templateId == envelope.templateId;
            });

           return result;
        }
    }
}