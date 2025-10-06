using UnityEngine;

public class PerformanceTracker : MonoBehaviour
{
    public string trackedName = "";

    System.Diagnostics.Stopwatch stopwatch;

    public void StartTracking()
    {
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
    }

    public void EndTracking()
    {
        stopwatch.Stop();
        Debug.Log(trackedName + ": " + stopwatch.ElapsedMilliseconds + " milliseconds");
    }
}
