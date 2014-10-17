// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException()
        {
        }

        public ServiceNotFoundException(Type serviceType)
            : base(FormatServiceNotFoundMessage(serviceType))
        {
            ServiceType = serviceType;
        }

        public ServiceNotFoundException(Type serviceType, Exception innerException)
            : base(FormatServiceNotFoundMessage(serviceType), innerException)
        {
            ServiceType = serviceType;
        }


        public Type ServiceType { get; private set; }


        private static string FormatServiceNotFoundMessage(Type serviceType)
        {
            return string.Format("Service [{0}] not found", serviceType.Name);
        }
    }
}