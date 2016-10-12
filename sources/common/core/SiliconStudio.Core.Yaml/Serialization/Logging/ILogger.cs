using System;

namespace SiliconStudio.Core.Yaml.Serialization.Logging
{
    /// <summary>
    /// Logger interface.
    /// </summary>
    public interface ILogger
    {
        void Log(LogLevel level, Exception ex, string message);
    }
}