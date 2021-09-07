using System.Collections.Generic;
using UnityEngine;

namespace Bundles.Utils
{
    public static class AssetTreeNames
    {
        public static string[] Initialize(RootObject names)
        {
            if (names != null)
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

        private static void HandleTree(List<string> lst, string rootName, List<Child> childs)
        {
            foreach (var chld in childs)
            {
                TraverseNames(lst, $"{rootName}/{chld.Name}", chld);
            }
        }

        private static void TraverseNames(List<string> lst, string name, Child chld)
        {
            if (chld.Children == null)
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
