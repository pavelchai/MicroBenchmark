# MicroBenchmark

Small solution for the performance checking

## How to use

1) Simple (default, 100 measurements, first 5 measurements will be skipped, outliers filtering is on)
```
var result = Benchmark.Run("your action that should be tested");
``` 

2) Specified (see in code/comments)
```
var result = Benchmark.Run("your action that should be tested", "specified params");
``` 

## Output results
* Mean
* Standard deviation
* Q1 (first quartile, 25th percentile)
* Q2 (second quartile, 50th percentile)
* Q3 (thitd quartile, 75th percentile)
* Resolution of the measuring

## Сapabilities
* Filtering the outliers (Tukey's fences outlier detection method is used)
* Small and easy to understand

## Limitations
* Measures performance with including calls the action, that will be added to the results. You may use this solution if calling time << code execution time in the action.
* Not propertly works for the actions that execution time less than or around of resolution of the timer (see in output result)
