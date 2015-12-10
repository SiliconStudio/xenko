// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Games.Testing.Requests
{
    [DataContract]
    internal class LogRequest : TestRequestBase
    {
        public string Message;
    }
}