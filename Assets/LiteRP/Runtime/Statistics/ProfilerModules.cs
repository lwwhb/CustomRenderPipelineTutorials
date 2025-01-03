using Unity.Profiling.Editor;

namespace LiteRP
{
    [ProfilerModuleMetadata("VisibleLights")] 
    public class LightsProfilerModule : ProfilerModule
    {
        private static readonly ProfilerCounterDescriptor[] kChartCounters = 
        {
            new ProfilerCounterDescriptor(MetricsStatistics.kTotalVisibleLightsCountName, MetricsStatistics.kVisibleLightsCategory),
        };
        public LightsProfilerModule() : base(kChartCounters)
        {
        }
    }
}
