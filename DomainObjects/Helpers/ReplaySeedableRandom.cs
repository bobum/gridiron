using System;

namespace DomainObjects.Helpers
{
    public class ReplaySeedableRandom : ISeedableRandom
    {
        private readonly SeedableRandom _baseRng;
        private readonly ReplayLog _log;
        private readonly bool _isReplay;
        private int _doubleIndex = 0;
        private int _intIndex = 0;
        private int _intRangeIndex = 0;

        public ReplaySeedableRandom(int seed, ReplayLog log = null!)
        {
            _baseRng = new SeedableRandom(seed);
            _log = log ?? new ReplayLog { Seed = seed };
            _isReplay = log != null;
        }

        public double NextDouble()
        {
            if (_isReplay)
            {
                if (_doubleIndex >= _log.Doubles.Count)
                    throw new InvalidOperationException("Replay log exhausted: Doubles");
                return _log.Doubles[_doubleIndex++];
            }
            var v = _baseRng.NextDouble();
            _log.Doubles.Add(v);
            return v;
        }

        public int Next()
        {
            if (_isReplay)
            {
                if (_intIndex >= _log.Ints.Count)
                    throw new InvalidOperationException("Replay log exhausted: Ints");
                return _log.Ints[_intIndex++];
            }
            var v = _baseRng.Next();
            _log.Ints.Add(v);
            return v;
        }

        public int Next(int maxValue)
        {
            if (_isReplay)
            {
                if (_intIndex >= _log.Ints.Count)
                    throw new InvalidOperationException("Replay log exhausted: Ints");
                return _log.Ints[_intIndex++];
            }
            var v = _baseRng.Next(maxValue);
            _log.Ints.Add(v);
            return v;
        }

        public int Next(int minValue, int maxValue)
        {
            if (_isReplay)
            {
                if (_intRangeIndex >= _log.IntRanges.Count)
                    throw new InvalidOperationException("Replay log exhausted: IntRanges");
                var entry = _log.IntRanges[_intRangeIndex++];
                if (entry.Min != minValue || entry.Max != maxValue)
                    throw new InvalidOperationException($"Replay log mismatch: expected ({minValue},{maxValue}), got ({entry.Min},{entry.Max})");
                return entry.Value;
            }
            var v = _baseRng.Next(minValue, maxValue);
            _log.IntRanges.Add(new RandomIntRangeEntry { Min = minValue, Max = maxValue, Value = v });
            return v;
        }

        public void GetBytes(byte[] buffer) => _baseRng.GetBytes(buffer);

        public void GetNonZeroBytes(byte[] data) => _baseRng.GetNonZeroBytes(data);

        public void Dispose() { }

        public ReplayLog GetReplayLog() => _log;
    }
}