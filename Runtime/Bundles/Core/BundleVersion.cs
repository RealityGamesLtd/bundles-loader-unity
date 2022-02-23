using System;

namespace BundlesLoader.Bundles.Core
{
    public class BundleVersion
    {
        public BundleVersion(string hash, DateTime createdAt, string minVersion, string maxVersion)
        {
            Hash = hash;
            CreatedAt = createdAt;
            if(Version.TryParse(minVersion, out var min)) {
                MinVersion = min;
            }
            if(Version.TryParse(maxVersion, out var max))
            {
                MaxVersion = max;
            }
        }

        public string Hash { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Version MinVersion { get; private set; }
        public Version MaxVersion { get; private set; }
    }
}