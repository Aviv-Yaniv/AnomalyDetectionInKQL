using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RangeAnomalyDetectionInKQL
{
    public static class ParametersFile
    {
        public static readonly string c_parametersFilePath = "parameters.json";

        public static void WriteParametersFile(RangeAnomaliesQueryGenerationParameters parameters)
        {
            var dictionary = ObjectExtensions.ObjectToDictionary<RangeAnomaliesQueryGenerationParameters>(parameters);
            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(c_parametersFilePath, json);
        }

        public static RangeAnomaliesQueryGenerationParameters ReadParametersFile()
        {
            Func<string, string, string> setDefaultIfNotSpecified = (value, default_value) => string.IsNullOrWhiteSpace(value) ? default_value : value;
            var json = setDefaultIfNotSpecified(File.ReadAllText(c_parametersFilePath), "{}");
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return ObjectExtensions.DictionaryToObject<RangeAnomaliesQueryGenerationParameters>(dictionary);
        }
    }
}
