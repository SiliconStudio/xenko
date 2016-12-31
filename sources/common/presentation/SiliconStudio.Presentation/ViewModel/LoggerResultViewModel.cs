// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// A view model based on the <see cref="LoggerViewModel"/> that monitors <see cref="LoggerResult"/> objects. The main difference with the base <see cref="LoggerViewModel"/>
    /// is that all mesages already contained <see cref="LoggerResult"/> are added to the <see cref="LoggerViewModel.Messages"/> collection in the constructor.
    /// </summary>
    public class LoggerResultViewModel : LoggerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerResultViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> to use for this view model.</param>
        public LoggerResultViewModel([NotNull] IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerResultViewModel"/> class with a single <see cref="LoggerResult"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> to use for this view model.</param>
        /// <param name="loggerResult">The <see cref="LoggerResult"/> to monitor.</param>
        public LoggerResultViewModel([NotNull] IViewModelServiceProvider serviceProvider, [NotNull] LoggerResult loggerResult)
            : base(serviceProvider, loggerResult)
        {
            var messages = (ObservableList<ILogMessage>)Messages;
            messages.AddRange(loggerResult.Messages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerResultViewModel"/> class multiple instances of <see cref="LoggerResult"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> to use for this view model.</param>
        /// <param name="loggerResults"></param>
        public LoggerResultViewModel([NotNull] IViewModelServiceProvider serviceProvider, [NotNull] IEnumerable<LoggerResult> loggerResults)
            : base(serviceProvider, loggerResults)
        {
            var messages = (ObservableList<ILogMessage>)Messages;
            foreach (var logger in Loggers)
            {
                var loggerResult = (LoggerResult)logger.Key;
                logger.Value.AddRange(loggerResult.Messages);
                messages.AddRange(loggerResult.Messages);
            }
        }

        /// <inheritdoc/>
        public override void AddLogger(Logger logger)
        {
            if (!(logger is LoggerResult)) throw new ArgumentException("logger");
            base.AddLogger(logger);
            var messages = (ObservableList<ILogMessage>)Messages;
            Loggers[logger].AddRange(((LoggerResult)logger).Messages);
            messages.AddRange(((LoggerResult)logger).Messages);
        }
    }
}