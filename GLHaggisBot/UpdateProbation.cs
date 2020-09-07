using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json.Linq;

namespace GLHaggisBot
{
    public class UpdateProbation
    {
        static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
        private readonly UserCredential _credential;
        private readonly String _spreadsheetId;
        private readonly SheetsService _service;
        private static readonly RegularExpressions Regex = new RegularExpressions();

        // Properties file
        private static readonly JObject Prop =
            JObject.Parse(
                File.ReadAllText(@"properties.json"));


        public UpdateProbation()
        {
            
        }
    }
}