using BundlesLoader.Service;

namespace BundlesLoader
{
    public static class AssetsServiceLoader
    {
        private static IAssetsService service;

        public static IAssetsService AssetsService { get => service; }

        public static void SetAssetsSerivce(IAssetsService assetsService) => service = assetsService;
    }
}
