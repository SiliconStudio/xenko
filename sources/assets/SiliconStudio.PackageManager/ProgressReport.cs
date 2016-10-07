using System;
using System.Linq;

using NuGet;

using SiliconStudio.PackageManager;

namespace SiliconStudio.PackageManager
{
    public class ProgressReport : IDisposable
    {
        private readonly NugetStore store;
        private readonly string version;
        private int progress;

        public ProgressReport(NugetStore store, NugetPackage package)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            this.store = store;
            version = package.Version.ToString();

            foreach (var progressProvider in store.SourceRepository.Repositories.OfType<IProgressProvider>())
            {
                progressProvider.ProgressAvailable += OnProgressAvailable;
            }
        }

        public event Action<int> ProgressChanged;

        public void Dispose()
        {
            foreach (var progressProvider in ((AggregateRepository)store.Manager.SourceRepository).Repositories.OfType<IProgressProvider>())
            {
                progressProvider.ProgressAvailable -= OnProgressAvailable;
            }
        }

        private void OnProgressAvailable(object sender, ProgressEventArgs e)
        {
            if (version == null)
                return;

            if (e.Operation.Contains(version))
            {
                var percentComplete = e.PercentComplete;
                if (progress != percentComplete)
                {
                    progress = percentComplete;
                    ProgressChanged?.Invoke(progress);
                }
            }         
        }
    }
}