namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    internal interface IQueryMath
    {
        double CalculateExactQueryLength(
            IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration);

        List<ResultEntry> GetBestCandidates(
            IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites,
            int numberOfCandidatesToReturn,
            IModelService modelService,
            FingerprintConfiguration fingerprintConfiguration,
            double queryLength);
    }
}