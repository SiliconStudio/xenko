namespace SiliconStudio.Assets
{
    public class PackageSaveParameters
    {
        private static readonly PackageSaveParameters DefaultParameters = new PackageSaveParameters();

        public static PackageSaveParameters Default()
        {
            return DefaultParameters.Clone();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PackageLoadParameters.</returns>
        public PackageSaveParameters Clone()
        {
            return (PackageSaveParameters)MemberwiseClone();
        }
    }
}
