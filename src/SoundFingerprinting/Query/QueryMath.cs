﻿namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    internal class QueryMath : IQueryMath
    {
        private readonly IQueryResultCoverageCalculator queryResultCoverageCalculator;
        private readonly IConfidenceCalculator confidenceCalculator;

        internal QueryMath(IQueryResultCoverageCalculator queryResultCoverageCalculator, IConfidenceCalculator confidenceCalculator)
        {
            this.queryResultCoverageCalculator = queryResultCoverageCalculator;
            this.confidenceCalculator = confidenceCalculator;
        }

        public List<ResultEntry> GetBestCandidates(
            List<HashedFingerprint> hashedFingerprints,
            IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites,
            int maxNumberOfMatchesToReturn,
            IModelService modelService,
            FingerprintConfiguration fingerprintConfiguration)
        {
            double queryLength = CalculateExactQueryLength(hashedFingerprints, fingerprintConfiguration);
            var trackIds = hammingSimilarites.OrderByDescending(e => e.Value.HammingSimilaritySum)
                                     .Take(maxNumberOfMatchesToReturn)
                                     .Select(p => p.Key)
                                     .ToList();

            var tracks = modelService.ReadTracksByReferences(trackIds);
            return tracks
                .Select(track => GetResultEntry(fingerprintConfiguration, track, hammingSimilarites[track.TrackReference], queryLength))
                .ToList();
        }

        public bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes)
        {
            int[] query = queryFingerprint.HashBins;
            int[] result = candidate.Hashes;
            int count = 0;
            for (int i = 0; i < query.Length; ++i)
            {
                if (query[i] == result[i])
                {
                    count++;
                }

                if (count >= thresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }

        public double CalculateExactQueryLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration)
        {
            double startsAt = double.MaxValue, endsAt = double.MinValue;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                startsAt = System.Math.Min(startsAt, hashedFingerprint.StartsAt);
                endsAt = System.Math.Max(endsAt, hashedFingerprint.StartsAt);
            }

            return SubFingerprintsToSeconds.AdjustLengthToSeconds(endsAt, startsAt, fingerprintConfiguration);
        }

        private ResultEntry GetResultEntry(FingerprintConfiguration configuration, TrackData track, ResultEntryAccumulator acc, double queryLength)
        {
            var coverage = queryResultCoverageCalculator.GetCoverage(
                acc.Matches,
                queryLength,
                configuration);

            double confidence = confidenceCalculator.CalculateConfidence(
                coverage.SourceMatchStartsAt,
                coverage.SourceMatchLength,
                queryLength,
                coverage.OriginMatchStartsAt,
                track.Length);

            return new ResultEntry(
                track,
                coverage.SourceMatchStartsAt,
                coverage.SourceMatchLength,
                coverage.OriginMatchStartsAt,
                GetTrackStartsAt(acc.BestMatch),
                confidence,
                acc.HammingSimilaritySum,
                queryLength,
                acc.BestMatch);
        }

        private double GetTrackStartsAt(MatchedPair bestMatch)
        {
            return bestMatch.HashedFingerprint.StartsAt - bestMatch.SubFingerprint.SequenceAt;
        }
    }
}
