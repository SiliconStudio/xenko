// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if !SILICONSTUDIO_PLATFORM_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.Plugin
{
    partial class CommsInterface
    {
        /// <summary>
        /// UnicastIPAddressInformation.IPv4Mask is not implemented in Xamarin. This method sits in a partial class definition
        /// on each native platform and retrieves the netmask in whatever way it can be done for each platform. 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            return ip.IPv4Mask;
        }
    }
}
#endif
