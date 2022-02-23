using System;
using Newtonsoft.Json;

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

        private Version min;
        private Version max;

        [JsonIgnore] public Version Min
        {
            get
            {
                if (min != null)
                    return min;

                if (Version.TryParse(MinVersion, out var ver))
                {
                    min = ver;
                    return min;
                }
                else
                    return null;
            }
        }

        [JsonIgnore] public Version Max
        {
            get
            {
                if (max != null)
                    return max;

                if (Version.TryParse(MaxVersion, out var ver))
                {
                    max = ver;
                    return max;
                }
                else
                    return null;
            }
        }
    }
}