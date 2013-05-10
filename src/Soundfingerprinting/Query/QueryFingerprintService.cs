﻿namespace Soundfingerprinting.Query
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.Utils;
    using Soundfingerprinting.Query.Configuration;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ICombinedHashingAlgoritm hashingAlgorithm;
        private readonly IModelService modelService;

        public QueryFingerprintService(ICombinedHashingAlgoritm hashingAlgorithm, IModelService modelService)
        {
            this.hashingAlgorithm = hashingAlgorithm;
            this.modelService = modelService;
        }

        public Task<QueryResult> Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        int bestMatch = 0;
                        int minDistance = int.MaxValue;
                        foreach (var fingerprint in fingerprints)
                        {
                            var tuple = hashingAlgorithm.Hash(fingerprint, queryConfiguration.NumberOfLSHTables, queryConfiguration.NumberOfMinHashesPerTable);
                            var subFingerprints = modelService.ReadSubFingerprintsByHashBucketsHavingThreshold(tuple.Item2, queryConfiguration.ThresholdVotes);
                            foreach (var subFingerprint in subFingerprints)
                            {
                                int distance = HashingUtils.CalculateHammingDistance(tuple.Item1, subFingerprint.Item1.Signature);
                                if (minDistance > distance)
                                {
                                    bestMatch = subFingerprint.Item1.TrackId;
                                    minDistance = distance;
                                }
                            }
                        }

                        if (bestMatch != 0)
                        {
                            return new QueryResult { BestMatch = modelService.ReadTrackById(bestMatch), IsSuccessful = true };
                        }

                        return new QueryResult();
                    });
        }
    }
}
