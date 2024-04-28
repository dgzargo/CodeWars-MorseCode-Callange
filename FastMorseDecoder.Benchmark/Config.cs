using System.Globalization;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.EventProcessors;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace FastMorseDecoder.Benchmark;

internal class Config: IConfig
{
	public IOrderer? Orderer { get; }
	public ICategoryDiscoverer? CategoryDiscoverer { get; }
	public SummaryStyle SummaryStyle { get; }
	public ConfigUnionRule UnionRule { get; }
	public string ArtifactsPath { get; }
	public CultureInfo? CultureInfo { get; }
	public ConfigOptions Options { get; }
	public TimeSpan BuildTimeout { get; }
	public IReadOnlyList<Conclusion> ConfigAnalysisConclusion { get; }

	public Config()
	{
		this.WithOptions(ConfigOptions.JoinSummary)
			.WithOptions(ConfigOptions.StopOnFirstError);
	}
        
	public IEnumerable<IColumnProvider> GetColumnProviders()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IExporter> GetExporters()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<ILogger> GetLoggers()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IDiagnoser> GetDiagnosers()
	{
		yield return MemoryDiagnoser.Default;
		yield return EventPipeProfiler.Default;
	}

	public IEnumerable<IAnalyser> GetAnalysers()
	{
		yield return EnvironmentAnalyser.Default;
		yield return ZeroMeasurementAnalyser.Default;
		yield return MinIterationTimeAnalyser.Default;
	}

	public IEnumerable<Job> GetJobs()
	{
		var defaultJob = Job.Dry.WithPlatform(Platform.AnyCpu)
							// .WithRuntime()
							.WithJit(Jit.RyuJit)
							// .WithAffinity()
							// .WithGcMode(GcMode.Default)
							// .WithLargeAddressAware()
		// end of environment configuration
							.WithEvaluateOverhead(true)
							.WithWarmupCount(5)
							.WithGcAllowVeryLargeObjects(false)
							.Freeze();

		yield return defaultJob.WithStrategy(RunStrategy.Throughput);
		yield return defaultJob.WithStrategy(RunStrategy.ColdStart);
	}

	public IEnumerable<IValidator> GetValidators()
	{
		//ReturnValueValidator.FailOnError.Validate(new ValidationParameters(new BenchmarkCase[]{Benchmarkca}))
		throw new NotImplementedException();
	}

	public IEnumerable<HardwareCounter> GetHardwareCounters()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IFilter> GetFilters()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<BenchmarkLogicalGroupRule> GetLogicalGroupRules()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<EventProcessor> GetEventProcessors()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IColumnHidingRule> GetColumnHidingRules()
	{
		throw new NotImplementedException();
	}
}