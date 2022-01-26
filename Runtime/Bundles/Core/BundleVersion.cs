using System;

namespace BundlesLoader.Bundles.Core
{
    public class BundleVersion
    {
        public BundleVersion(string hash, DateTime createdAt, string minVersion, string maxVersion)
        {
            Hash = hash;
            CreatedAt = createdAt;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        public string Hash { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string MinVersion { get; private set; }
        public string MaxVersion { get; private set; }
    }
}