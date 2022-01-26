using System.Collections.Generic;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public abstract class Versionable : ScriptableObject
    {
        public abstract List<string> Versions { get; }
    }
}