<div id="top"></div>

<h3 align="center">Tool For Anomaly Detection in KQL</h3>

<div>
  <p align="center">
    Covering both missing rows gaps and invalid values anomalies
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>      
    </li>
    <li><a href="#GapsAnomalyDetectionInKQL">Gaps Anomaly Detection In KQL</a></li>
    <li><a href="#RangeAnomalyDetectionInKQL">Range Anomaly Detection In KQL</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

Anomaly Detection Tool Covers the Following Scenarios; <br> 
1) Gaps Anomaly Detection : When tables should have row each `dt` time, and we want to discover missing rows <br>
2) Range Anomaly Detection : When specific column should have values within a given range, and we are interested at what time invalid values is reported (e.g. negative `CPU` due to overflow) <br>

After successful demonstration in multiple use cases that discovered edge cases and saved manual work, and interest from outside of the team, we release this tool so you can benefit from it too. 

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- GAPS ANOMALY DETECTION IN KQL -->
## Gaps Anomaly Detection In KQL
Input is:
1. `tableName` : The name of the table to search for gaps anomalies
2. `dt` : The interval on which a new row should arrive
3. `anomaly_type` : String placeholder for display the anomaly type in the output table
4. `time_col` : The time column
5. `unique_id_col` : The unique id column (e.g. ID number)
6. `endtime` : The end time, that we shall look before it to search for anomalies
7. `window` : Used to aggregate amount of rows based on a common time slot
8. `window_records_pct` : The percent from the actual amount of expected records thresold, that below it is considered an anomaly (e.g. if less than 60% of exepcted 100 records, is considered an anomaly)


The `window` parameter is a tunning parameter;  <br>
gaps might occur because of a transient network issue of the input source to the table, so you want to consider a bigger window and tune the percent threshold <br>

```
let endtime           = ago(4h);
let tableName         = 'Table';
let timeColumnName    = 'TimeColumn';
let uniqueColumnName  = 'uniqueColumn';
let window            = 4h;
let threshold         = 90.0;
detect_anomalies_details(table = tableName, dt = 10m, anomaly_type = "Missing", time_col = timeColumnName, unique_id_col = uniqueColumnName, endtime = endtime, window = window, window_records_pct = threshold)
```

<!-- RANGE ANOMALY DETECTION IN KQL -->
## Range Anomaly Detection In KQL
In case we want to check if ranges are below or above certain value.

```
// You can keep range validation it as is, if no validation is required (and no redundant calculation will occur)
| extend RangeAnomalyDetails_pColumn1 = in_range_anomaly_reals('Column1', s_pColumn1, s_pTimestampColumn)
// Or pass lower and upper parameters (or either pass just one of them) if validation is required
| extend RangeAnomalyDetails_pColumn1 = in_range_anomaly_reals('Column1', s_pColumn1, s_pTimestampColumn, lower = 0, upper = 100)
```

<!-- CONTACT -->
## Contact

Aviv Yaniv <br>
[Email](avivyaniv@microsoft.com)
[Site](https://www.tau.ac.il/~avivyaniv/)
[Blog](https://avivyaniv.medium.com/)
[StackOverflow](https://stackoverflow.com/users/14148864/aviv-yaniv?tab=profile)
[GitHub](https://github.com/AvivYaniv)
[Project Euler](https://projecteuler.net/profile/Aviv_Yaniv.png)

<p align="right">(<a href="#top">back to top</a>)</p>
