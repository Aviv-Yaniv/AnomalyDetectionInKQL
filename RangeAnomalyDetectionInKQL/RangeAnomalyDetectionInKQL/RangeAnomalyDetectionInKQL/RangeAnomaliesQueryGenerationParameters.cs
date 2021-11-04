using System;
using System.Collections.Generic;
using System.Text;

namespace RangeAnomalyDetectionInKQL
{
    public class RangeAnomaliesQueryGenerationParameters
    {
        public string lookback { get; set; }     = "ago(6h)";
        public string tableName { get; set; }    = "TableName";
        public string timeColumn { get; set; }   = "TimestampColumn";
        public string uniqueColumn { get; set; } = "UniqueIdColumn";
        public string columnNames { get; set; }  = "Column1, Column2, Column3";
    }
}
