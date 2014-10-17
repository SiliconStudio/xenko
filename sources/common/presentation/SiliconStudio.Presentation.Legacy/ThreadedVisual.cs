// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Represents a <c>Visual</c> rendered in a different <c>Thread</c> than the one in which it has been created.
    /// <remarks>More specifically, the <c>ThreadedVisual</c> is a kind of <c>Visual</c> gateway that
    /// renders a <c>Visual</c> in a certain <c>Thread</c> and present it in another <c>Visual</c> in another <c>Thread</c>.
    /// Caution: There is absolutely no layout neither input processing in the new rendering <c>Thread</c>.
    /// Everything must be managed manually from the calling <c>Thread</c>.</remarks>
    /// </summary>
    public abstract class ThreadedVisual : HostVisual
    {
        /// <summary>
        /// Gets the <c>Dispatcher</c> of the newly created <c>Thread</c> in which the <c>Visual</c> is rendered.
        /// <remarks>Becareful, this <c>Dispatcher</c> is not the one in which the <c>ThreadedVisual</c> has been instanced.</remarks>
        /// </summary>
        public new Dispatcher Dispatcher { get; private set; }
        /// <summary>
        /// Gets the <c>SynchronizationContext</c> of the newly created <c>Thread</c> in which the <c>Visual</c> is rendered.
        /// <remarks>Becareful, this <c>SynchronizationContext</c> is not the one in which the <c>ThreadedVisual</c> has been instanced.</remarks>
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// Initializes the <c>ThreadedVisual</c>.
        /// <remarks>The new thread is created in the constructor,
        /// and thus the abstract ProduceRootVisual method is called from within the constructor too.</remarks>
        /// </summary>
        protected ThreadedVisual()
        {
            AutoResetEvent initNotifier = new AutoResetEvent(false);

            Thread th = new Thread(() =>
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext = new DispatcherSynchronizationContext();

                VisualTargetPresentationSource presentationSource = new VisualTargetPresentationSource(this);
                presentationSource.RootVisual = ProduceRootVisual();

                initNotifier.Set();

                Dispatcher.Run();
            });

            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();

            initNotifier.WaitOne();
        }

        /// <summary>
        /// When overridden, produces the top-level <c>Visual</c> element to be rendered in the new thread (a secondary UI thread).
        /// </summary>
        /// <returns>Returns the <c>Visual</c> to be rendered by the new thread.</returns>
        protected abstract Visual ProduceRootVisual();
    }

    /// <summary>
    /// Represents a <c>ThreadedVisual</c> where top-level <c>Visual</c> element generation can be delegated to an anonymous method.
    /// </summary>
    public class DelegateThreadedVisual : ThreadedVisual
    {
        private Func<Visual> produceRootVisual;

        /// <summary>
        /// Initializes the <c>DelegateThreadedVisual</c>.
        /// <remarks>The new thread is created in the constructor,
        /// and thus the produceRootVisual anonymous method is called from within the constructor too.</remarks>
        /// </summary>
        public DelegateThreadedVisual(Func<Visual> produceRootVisual)
        {
            if (produceRootVisual == null)
                throw new ArgumentNullException("produceRootVisual");

            this.produceRootVisual = produceRootVisual;
        }

        /// <summary>
        /// Produces the top-level <c>Visual</c> element to be rendered in the new thread (a secondary UI thread).
        /// </summary>
        /// <returns>Returns the <c>Visual</c> to be rendered by the new thread.</returns>
        protected override Visual ProduceRootVisual()
        {
            Visual result = produceRootVisual();
            if (result == null)
                throw new InvalidOperationException("produceRootVisual anonymous method must return a valid instance of Visual.");
            return result;
        }
    }
}
