namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class ConfigPack : Pack
    {
        public ConfigPack(byte[] bytes, string name, string parent)
        {
            Bytes = bytes;
            Name = name;
            Parent = parent;

            if(bytes != null)
            {
                var str = System.Text.Encoding.UTF8.GetString(bytes);
                if (str != null)
                {
                    ParseConfig(str);
                }
            }
        }

        public virtual void ParseConfig(string conf) {}
    }
}
