using System.Collections.Generic;
using System.IO;
using Docusign_Connect.Constants;
using Docusign_Connect.DTO;
using Newtonsoft.Json;

namespace Docusign_Connect.Libs
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

        public static List<LingkEnvelope> ReadDocusignEnvelopesFromFileSystem(string filePath)
        {
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var envelopeList = JsonConvert.DeserializeObject<List<LingkEnvelope>>(jsonData) ?? new List<LingkEnvelope>();

            return envelopeList;
        }
    }
}