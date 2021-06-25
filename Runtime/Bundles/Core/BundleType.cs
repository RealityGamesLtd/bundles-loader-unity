using System.Collections.Generic;
using Bundles.Utils;
using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class BundleType
    {
        public string[] Names => Initialze(AssetBundlesChecker.GetBundlesNames());

        public string FullName;

        private string[] Initialze(RootObject names)
        {
            if(names != null)
            {
                var listNames = new List<string>();
                HandleTree(listNames, names.RootName, names.Children);
                return listNames.ToArray();
            }
            else
            {
                Debug.LogError("Names are null!");
                return new string[] { string.Empty };
            }
        }

        private void HandleTree(List<string> lst, string rootName, List<Child> childs)
        {
            foreach (var chld in childs)
            {
                TraverseNames(lst, $"{rootName}/{chld.Name}", chld);
            }
        }

        private void TraverseNames(List<string> lst, string name, Child chld)
        {
            if(chld.Children == null)
            {
                lst.Add(name);
            }
            else
            {
                foreach (var child in chld.Children)
                {
                    TraverseNames(lst, $"{name}/{child.Name}", child);
                }
            }
        }
    }
}
