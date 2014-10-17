// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SiliconStudio.Paradox.VisualStudio
{
    public class SolutionEventsListener : IVsSolutionEvents, IVsSolutionLoadEvents, IDisposable
    {
        private IVsSolution solution;
        private uint solutionEventsCookie;

        public event Action AfterSolutionLoaded;
        public event Action AfterSolutionBackgroundLoadComplete;
        public event Action BeforeSolutionClosed;

        public event Action<IVsHierarchy> AfterProjectOpened;
        public event Action<IVsHierarchy> BeforeProjectClosed;

        public SolutionEventsListener(IServiceProvider serviceProvider)
        {
            solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            var afterSolutionBackgroundLoadComplete = AfterSolutionBackgroundLoadComplete;
            if (afterSolutionBackgroundLoadComplete != null)
                afterSolutionBackgroundLoadComplete();
            return VSConstants.S_OK;
        }

        #region IVsSolutionEvents Members

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var afterProjectOpened = AfterProjectOpened;
            if (afterProjectOpened != null)
                afterProjectOpened(pHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            var afterSolutionLoaded = AfterSolutionLoaded;
            if (afterSolutionLoaded != null)
                afterSolutionLoaded();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var beforeProjectClosed = BeforeProjectClosed;
            if (beforeProjectClosed != null)
                beforeProjectClosed(pHierarchy);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            var beforeSolutionClosed = BeforeSolutionClosed;
            if (beforeSolutionClosed != null)
                beforeSolutionClosed();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (solution != null && solutionEventsCookie != 0)
            {
                GC.SuppressFinalize(this);
                solution.UnadviseSolutionEvents(solutionEventsCookie);
                AfterSolutionLoaded = null;
                BeforeSolutionClosed = null;
                solutionEventsCookie = 0;
                solution = null;
            }
        }

        #endregion
    }
}