namespace SiliconStudio.Assets.Quantum
{
    public class AssetPropertyNodeGraphContainer
    {
        private readonly PackageSession session;
        private readonly AssetNodeContainer nodeContainer;

        public AssetPropertyNodeGraphContainer(PackageSession session, AssetNodeContainer nodeContainer)
        {
            this.session = session;
            this.nodeContainer = nodeContainer;
        }

        public void InitializeAssets()
        {

        }
    }
}
