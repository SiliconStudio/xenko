// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for detail

using System;
using System.Linq;
using NuGet;

namespace SiliconStudio.Packages
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
            version = package.Version.ToSemanticVersion().ToNormalizedString();

            store.DownloadProgressChanged += UpdateProgress;

            foreach (var progressProvider in store.SourceRepository.Repositories.OfType<IProgressProvider>())
            {
                progressProvider.ProgressAvailable += OnProgressAvailable;
            }
        }

        public event Action<int> ProgressChanged;

        public void Dispose()
        {
            foreach (var progressProvider in store.SourceRepository.Repositories.OfType<IProgressProvider>())
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

        private void UpdateProgress(int percentage)
        {
            // Only update when changed.
            if (progress != percentage)
            {
                progress = percentage;
                ProgressChanged?.Invoke(percentage);
            }
        }

    }
}
