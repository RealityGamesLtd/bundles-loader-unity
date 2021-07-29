namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class Pack
    {
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
    }

    public class BytePack : Pack
    {
        public BytePack(byte[] bytes, string name, string parent)
        {
            Bytes = bytes;
            Name = name;
            Parent = parent;
        }
    }
}
