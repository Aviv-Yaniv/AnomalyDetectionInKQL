using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RangeAnomalyDetectionInKQL
{
    class Program
    {
        private const string c_RangeAnomaliesFile = "RangeAnomalies.kql";

        private const string c_anomaliesNamesColumn = "AnomaliesNames";

        private const string c_realsInRangeFunctions =
@"let build_range_anomaly_details_structured = (metric_name:string, anomaly_description:string, expected:real, actual:real, timestamp:string)
{
    pack('metric_name', metric_name, 'anomaly_description', anomaly_description, 'expected', expected, 'actual', actual, 'timestamp', timestamp)
};
let is_range_reals_by_stats = (metric_name:string, vec:dynamic, timestamps:dynamic, lower:real = real(-inf), upper:real = real(+inf))
{
    let vec_stats = series_stats_dynamic(vec);
    let min = vec_stats['min'];
    let max = vec_stats['max'];
    let min_message = iff(min   < lower, tostring(build_range_anomaly_details_structured(metric_name=metric_name, anomaly_description='below_min', expected=lower, actual=vec_stats['min'], timestamp=tostring(timestamps[toint(vec_stats['min_idx'])]))), '');
    let max_message = iff(upper < max  , tostring(build_range_anomaly_details_structured(metric_name=metric_name, anomaly_description='above_max', expected=upper, actual=vec_stats['max'], timestamp=tostring(timestamps[toint(vec_stats['max_idx'])]))), '');
    strcat(iff(min_message == '', '', strcat(min_message, ',')), max_message)
};
let in_range_anomaly_reals = (metric_name:string, vec:dynamic, timestamps:dynamic, lower:real = real(-inf), upper:real = real(+inf))
{
        let is_no_range_specified = (lower == real(-inf)) and (upper == real(+inf));
        iff(is_no_range_specified, '', is_range_reals_by_stats(metric_name, vec, timestamps, lower, upper))
};";

        private static IEnumerable<string> ParseColumns(string columns)
        {
            return columns.Split(',').Select(x => x.Trim()).ToArray();
        }

        private static string GetColumnPackName(string columnName)
        {
            return $"p{columnName}";
        }

        private static string GetColumnPackSummarizedName(string columnName)
        {
            return $"s_{GetColumnPackName(columnName)}";
        }
        private static string GetColumnRangeAnomaliesName(string columnName)
        {
            return $"RangeAnomalyDetails_{GetColumnPackName(columnName)}";
        }

        private static string GetColumnsPacks(IEnumerable<string> columnNames)
        {
            StringBuilder sbColumnPacks = new StringBuilder();
            foreach (var columnName in columnNames)
            {
                sbColumnPacks.Append($"| extend {GetColumnPackName(columnName)} = pack_array({columnName}) \n");
            }
            return sbColumnPacks.ToString();
        }

        private static string GetColumnsPacksSummarized(IEnumerable<string> columnNames, string uniqueColumn)
        {
            StringBuilder sbColumnPacks = new StringBuilder();
            sbColumnPacks.Append("| summarize ");
            sbColumnPacks.Append(string.Join(", \n\t\t", columnNames.Select(columnName => $"{GetColumnPackSummarizedName(columnName)} = make_set({GetColumnPackName(columnName)})")));
            sbColumnPacks.Append($" by {uniqueColumn}\n");
            return sbColumnPacks.ToString();
        }

        private static string GetRangesOnPacksSummarized(IEnumerable<string> columnNames, string timeColumn)
        {
            StringBuilder sbColumnPacks = new StringBuilder();
            sbColumnPacks.Append("| extend ");
            sbColumnPacks.Append(string.Join(", \n\t\t", columnNames.Select(columnName => $"{GetColumnRangeAnomaliesName(columnName)} = in_range_anomaly_reals('{columnName}', {GetColumnPackSummarizedName(columnName)}, {GetColumnPackSummarizedName(timeColumn)})")));
            sbColumnPacks.Append("\n");
            return sbColumnPacks.ToString();
        }

        private static string ExtendGetAnomaliesNames(IEnumerable<string> columnNames)
        {
            StringBuilder sbColumnPacks = new StringBuilder();
            sbColumnPacks.Append($"| extend {c_anomaliesNamesColumn} = strcat(");
            sbColumnPacks.Append(string.Join("", columnNames.Select(columnName => $"iff({GetColumnRangeAnomaliesName(columnName)} != '', strcat({GetColumnRangeAnomaliesName(columnName)}, ','), ''), ")));
            sbColumnPacks.Append("'')\n");
            return sbColumnPacks.ToString();
        }

        private static IEnumerable<string> GetColumnNamesWithTimeCols(IEnumerable<string> columnNames, string timeColumn)
        {
            var columnNamesCopy = columnNames.ToList();
            columnNamesCopy.Add(timeColumn);
            return columnNamesCopy;
        }

        private static string GenerateAnomalyDetectionQuery(RangeAnomaliesQueryGenerationParameters rangeAnomaliesQueryGenerationParameters)
        {
            IEnumerable<string> columnNames = ParseColumns(rangeAnomaliesQueryGenerationParameters.columnNames);
            StringBuilder sbValuesAnomalyDetection = new StringBuilder();
            IEnumerable<string> columnNamesWithTimeCols = GetColumnNamesWithTimeCols(columnNames, rangeAnomaliesQueryGenerationParameters.timeColumn);

            sbValuesAnomalyDetection.Append(c_realsInRangeFunctions);
            sbValuesAnomalyDetection.Append("\n");
            sbValuesAnomalyDetection.Append($"{rangeAnomaliesQueryGenerationParameters.tableName}\n");
            sbValuesAnomalyDetection.Append($"| where {rangeAnomaliesQueryGenerationParameters.timeColumn} > {rangeAnomaliesQueryGenerationParameters.lookback}\n");
            sbValuesAnomalyDetection.Append(GetColumnsPacks(columnNamesWithTimeCols));
            sbValuesAnomalyDetection.Append(GetColumnsPacksSummarized(columnNamesWithTimeCols, rangeAnomaliesQueryGenerationParameters.uniqueColumn));
            sbValuesAnomalyDetection.Append(GetRangesOnPacksSummarized(columnNames, rangeAnomaliesQueryGenerationParameters.timeColumn));
            sbValuesAnomalyDetection.Append(ExtendGetAnomaliesNames(columnNames));
            sbValuesAnomalyDetection.Append($"| where {c_anomaliesNamesColumn} != ''\n");
            sbValuesAnomalyDetection.Append($"| project {rangeAnomaliesQueryGenerationParameters.uniqueColumn}, {c_anomaliesNamesColumn}, currentTime=now(), lookback={rangeAnomaliesQueryGenerationParameters.lookback}");

            return sbValuesAnomalyDetection.ToString();
        }

        static void Main(string[] args)
        {
            // ParametersFile.WriteParametersFile(new RangeAnomaliesQueryGenerationParameters());
            Console.WriteLine("*** Range Based Anomaly Detection Query Generation Tool ***");
            Console.WriteLine($"Reading properties from: {ParametersFile.c_parametersFilePath}");
            var rangeAnomaliesQueryGenerationParameters = ParametersFile.ReadParametersFile();
            var rangeAnomalyDetectionQuery = GenerateAnomalyDetectionQuery(rangeAnomaliesQueryGenerationParameters);
            File.WriteAllText(c_RangeAnomaliesFile, rangeAnomalyDetectionQuery);
        }
    }
}
