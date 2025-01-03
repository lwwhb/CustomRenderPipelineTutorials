using Unity.Profiling;

namespace LiteRP
{
    public class MetricsStatistics
    {
        public static readonly ProfilerCounterOptions kCounterOptions = 
            ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush;
        
        //VisibleLights
        public static readonly ProfilerCategory kVisibleLightsCategory = new ProfilerCategory("VisibleLights");
        public const string kTotalVisibleLightsCountName = "Total Visible Lights Count";
        public static readonly ProfilerCounterValue<int> TotalLightsCount =
            new ProfilerCounterValue<int>(kVisibleLightsCategory, kTotalVisibleLightsCountName, ProfilerMarkerDataUnit.Count, kCounterOptions);
    }
}
