using System;
using System.Collections.Generic;
using System.IO;
using BundlesLoader.Bundles.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.Bundles.Parsers
{
    public class AssetBundlesVersionsCreator
    {
        public static string CreateVersions(string[] files, List<Container> selectedObj, string minVersion, string maxVersion)
        {
            Dictionary<string, BundleVersion> tokens = new Dictionary<string, BundleVersion>();
            for (int i = 0; i < files.Length; ++i)
            {
                var name = Path.GetFileName(files[i]);
                byte[] bytes;

                try
                {
                    bytes = File.ReadAllBytes(files[i]);
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                    return string.Empty;
                }

                var bundle = selectedObj.Find(x => x.BundleName.Equals(name));
                if (bundle == null)
                    continue;

                if (!tokens.ContainsKey(name))
                {
                    tokens.Add(name,
                        new BundleVersion(Md5Sum(bytes),
                        DateTime.Now,
                        minVersion,
                        maxVersion));
                }
                else
                {
                    Debug.LogWarning($"{files[i]} already in dictionary");
                }
            }

            string output = string.Empty;

            try
            {
                output = JsonConvert.SerializeObject(tokens, Formatting.Indented);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return output;
        }

        private static string Md5Sum(byte[] bytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(32, '0');
        }
    }
}