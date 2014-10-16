// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    public struct AppDomainUnloadWrapper : IDisposable
    {
        private AppDomain domain;

        public AppDomainUnloadWrapper(AppDomain domain)
        {
            this.domain = domain;
        }

        public static implicit operator AppDomain(AppDomainUnloadWrapper domain)
        {
            return domain.domain;
        }

        public void Dispose()
        {
            AppDomain.Unload(domain);
        }
    }
}