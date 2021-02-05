using System;
using System.Collections.Generic;
using System.IO;
using Lingk_SAML_Example.Constants;
using Lingk_SAML_Example.DTO;
using Newtonsoft.Json;

namespace Lingk_SAML_Example.LingkFileSystem
{
    public static class LingkFile
    {
        public static void Create(string filePath, string data)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(filePath,
            filePath == LingkConst.TempSettingsPath ? System.IO.FileMode.Create : System.IO.FileMode.Append)))
            {
                sw.WriteLine(data);
            }
        }
        public static void AddDocusignEnvelope(string filePath, LingkEnvelope envelope)
        {
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelope>>(jsonData) ?? new List<LingkEnvelope>();

            // Add any new envelope
            envelopeList.Add(envelope);

            // Update json data string
            jsonData = JsonConvert.SerializeObject(envelopeList);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public static LingkEnvelope CheckEnvelopeExists(string filePath, LingkEnvelope envelope)
        {
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelope>>(jsonData) ?? new List<LingkEnvelope>();

            var result = envelopeList.Find((e) =>
            {
                return e.accountId == envelope.accountId && e.templateId == envelope.templateId;
            });

            return result;
        }
    }
}