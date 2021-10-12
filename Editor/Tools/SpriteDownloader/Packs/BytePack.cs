namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
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
