using System;
using System.Collections.Generic;

namespace LayeredPlayables
{
    /// <summary>A custom animation event at a normalized clip time in [0, 1].</summary>
    public readonly struct TimelineMarker
    {
        public readonly double NormalizedTime;
        public readonly string Id;
        public readonly string Payload;

        public TimelineMarker(double normalizedTime, string id, string payload = null)
        {
            NormalizedTime = normalizedTime;
            Id = id;
            Payload = payload;
        }
    }

    /// <summary>A marker crossing on an unwrapped, forward-moving clip timeline.</summary>
    public readonly struct TimelineOccurrence
    {
        public readonly TimelineMarker Marker;
        public readonly long Cycle;
        public readonly double AbsoluteTime;

        public TimelineOccurrence(TimelineMarker marker, long cycle, double absoluteTime)
        {
            Marker = marker;
            Cycle = cycle;
            AbsoluteTime = absoluteTime;
        }
    }

    /// <summary>Collects custom event crossings without depending on Unity APIs.</summary>
    public static class AnimationEventTimeline
    {
        // A deliberate failure is safer than hanging a frame after an enormous time jump.
        public const int MaxOccurrencesPerCollect = 10000;

        // Beyond this range a double cannot reliably distinguish neighboring cycle times.
        public const long MaxSupportedCycle = 4503599627370494L;

        /// <summary>
        /// Appends events in (previousTime, currentTime], ordered by time and then marker
        /// definition order. includeStart additionally includes time zero only when
        /// previousTime is zero. Times are unwrapped seconds, not wrapped clip positions.
        /// For looping clips, normalized time 1 is canonicalized to time 0 of each cycle;
        /// separate marker definitions at 0 and 1 still each emit their own occurrence.
        /// Non-looping clips keep 0 and 1 as separate endpoints and never repeat.
        /// The output is unchanged if validation, precision, or the event budget fails.
        /// </summary>
        /// <exception cref="ArgumentException">Times move backward or a marker is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Time, duration, or cycle range is invalid.</exception>
        /// <exception cref="InvalidOperationException">Too many crossings or indistinguishable cycle times.</exception>
        public static void Collect(
            IReadOnlyList<TimelineMarker> markers,
            double previousTime,
            double currentTime,
            double duration,
            bool loop,
            bool includeStart,
            List<TimelineOccurrence> output)
        {
            if (markers == null) throw new ArgumentNullException(nameof(markers));
            if (output == null) throw new ArgumentNullException(nameof(output));
            ValidateTime(previousTime, nameof(previousTime));
            ValidateTime(currentTime, nameof(currentTime));
            if (!IsFinite(duration) || duration <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be finite and greater than zero.");
            if (currentTime < previousTime)
                throw new ArgumentException("Only forward playback is supported: currentTime must be at least previousTime.", nameof(currentTime));

            // Validate the full input before collecting, including when the interval is empty.
            for (int i = 0; i < markers.Count; ++i)
            {
                double normalized = markers[i].NormalizedTime;
                if (!IsFinite(normalized) || normalized < 0.0 || normalized > 1.0)
                    throw new ArgumentException("Marker " + i + " must have a finite normalized time in [0, 1].", nameof(markers));
            }

            bool collectStart = includeStart && previousTime == 0.0;
            if (markers.Count == 0 || (currentTime == previousTime && !collectStart)) return;

            long firstCandidateCycle = 0;
            if (loop)
            {
                double currentCycle = currentTime / duration;
                if (!IsFinite(currentCycle) || currentCycle > MaxSupportedCycle)
                    throw new ArgumentOutOfRangeException(nameof(currentTime), "Looping time exceeds the supported double-precision cycle range.");

                // Start one cycle earlier to tolerate division rounding at an exact seam.
                double previousCycle = Math.Floor(previousTime / duration);
                firstCandidateCycle = previousCycle > 0.0 ? (long)previousCycle - 1L : 0L;
            }

            var collected = new List<IndexedOccurrence>();
            for (int markerIndex = 0; markerIndex < markers.Count; ++markerIndex)
            {
                TimelineMarker marker = markers[markerIndex];
                double normalized = loop && marker.NormalizedTime == 1.0 ? 0.0 : marker.NormalizedTime;
                double phase = normalized * duration;

                if (!loop)
                {
                    if (IsCrossing(phase, previousTime, currentTime, collectStart))
                        Add(collected, marker, markerIndex, 0L, phase);
                    continue;
                }

                long cycle = firstCandidateCycle;
                double lastCandidateTime = -1.0;
                while (true)
                {
                    double absoluteTime = cycle * duration + phase;
                    if (absoluteTime > currentTime) break;
                    if (absoluteTime <= lastCandidateTime)
                        throw new InvalidOperationException("Double precision cannot distinguish adjacent event cycles at this time and duration.");
                    lastCandidateTime = absoluteTime;

                    if (IsCrossing(absoluteTime, previousTime, currentTime, collectStart))
                        Add(collected, marker, markerIndex, cycle, absoluteTime);

                    // Integer advancement is bounded, unlike repeatedly adding duration.
                    if (cycle >= MaxSupportedCycle) break;
                    ++cycle;
                }
            }

            collected.Sort(Compare);
            for (int i = 0; i < collected.Count; ++i) output.Add(collected[i].Occurrence);
        }

        private static bool IsCrossing(double time, double previousTime, double currentTime, bool collectStart)
        {
            return time <= currentTime && (time > previousTime || (collectStart && time == 0.0));
        }

        private static void Add(List<IndexedOccurrence> collected, TimelineMarker marker, int markerIndex, long cycle, double time)
        {
            if (collected.Count >= MaxOccurrencesPerCollect)
                throw new InvalidOperationException("More than " + MaxOccurrencesPerCollect + " animation event occurrences in one Collect call. Split the time interval into smaller steps.");
            collected.Add(new IndexedOccurrence(new TimelineOccurrence(marker, cycle, time), markerIndex));
        }

        private static int Compare(IndexedOccurrence left, IndexedOccurrence right)
        {
            int byTime = left.Occurrence.AbsoluteTime.CompareTo(right.Occurrence.AbsoluteTime);
            if (byTime != 0) return byTime;
            int byMarker = left.MarkerIndex.CompareTo(right.MarkerIndex);
            return byMarker != 0 ? byMarker : left.Occurrence.Cycle.CompareTo(right.Occurrence.Cycle);
        }

        private static void ValidateTime(double time, string parameterName)
        {
            if (!IsFinite(time) || time < 0.0)
                throw new ArgumentOutOfRangeException(parameterName, "Time must be finite and nonnegative.");
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private readonly struct IndexedOccurrence
        {
            public readonly TimelineOccurrence Occurrence;
            public readonly int MarkerIndex;

            public IndexedOccurrence(TimelineOccurrence occurrence, int markerIndex)
            {
                Occurrence = occurrence;
                MarkerIndex = markerIndex;
            }
        }
    }
}
