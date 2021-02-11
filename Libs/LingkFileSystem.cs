using System.Collections.Generic;
using System.IO;
using Docusign_Connect.Constants;
using Docusign_Connect.DTO;
using Newtonsoft.Json;

namespace Docusign_Connect.Libs
{
    public static class LingkFile
    {
        public static string filePath = LingkConst.LingkFileSystemPath;
        public static void Create(string data)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(filePath, System.IO.FileMode.Append)))
            {
                sw.WriteLine(data);
            }
        }
        public static void AddDocusignEnvelope(LingkEnvelope envelope)
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

        public static void UpdateEnvelopes(List<LingkEnvelope> envelopes)
        {
            // Update json data string
            var jsonData = JsonConvert.SerializeObject(envelopes);
            System.IO.File.WriteAllText(filePath, jsonData);
        }


        public static LingkEnvelope CheckEnvelopeExists(LingkEnvelope envelope)
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

        public static List<LingkEnvelope> ReadEnvelopesFromFileSystem()
        {
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelope>>(jsonData) ?? new List<LingkEnvelope>();

            return envelopeList;
        }
    }
}