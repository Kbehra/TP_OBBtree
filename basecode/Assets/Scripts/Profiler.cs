using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Profiling
{
    static public class Profiler
    {
        static private Dictionary<string, List<double>> m_durations;

        static private Dictionary<string, Stopwatch> m_chronos;

        static public void Start(string label)
        {
            if (m_durations == null)
            {
                m_durations = new Dictionary<string, List<double>>();
            }

            if (!m_durations.ContainsKey(label))
            {
                m_durations.Add(label, new List<double>());
            }

            if (m_chronos == null)
            {
                m_chronos = new Dictionary<string, Stopwatch>();
            }

            if (!m_chronos.ContainsKey(label))
            {
                m_chronos.Add(label, new Stopwatch());
            }

            m_chronos[label].Reset();

            m_chronos[label].Start();
        }

        static public double Stop(string label)
        {
            if (m_chronos.ContainsKey(label) && m_durations.ContainsKey(label))
            {
                m_chronos[label].Stop();

                long elapsed_ticks = m_chronos[label].ElapsedTicks;

                double elapsed_s = (double)elapsed_ticks / (double)System.Diagnostics.Stopwatch.Frequency;

                double elapsed_ms = elapsed_s * 1000.0;

                m_durations[label].Add(elapsed_ms);

                return elapsed_ms;
            }
            else
            {
                UnityEngine.Debug.LogError("Please call StartProfiling before StopProfiling");

                return 0.0;
            }
        }

        static public void DisplayAllAverages()
        {
            if (m_durations != null)
            {
                foreach (string label in m_durations.Keys)
                {
                    DisplayAverage(label);
                }
            }
        }

        static public void DisplayAverage(string label)
        {
            if (m_durations != null)
            {
                if (m_durations.ContainsKey(label))
                {
                    double sum = 0.0;

                    foreach (double duration in m_durations[label])
                    {
                        sum += duration;
                    }

                    UnityEngine.Debug.Log("Average duration for " + label + ": " + sum / (double)m_durations[label].Count + "ms");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Profiler not found (" + label + ")");
                }
            }
        }
    }
}