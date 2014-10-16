// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Quantum;
#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace SiliconStudio.Presentation.SampleApp
{
    public class AsyncTestObject : INotifyPropertyChanged
    {
        private ViewModelContentState loadState = ViewModelContentState.NotLoaded;
        public ViewModelContentState LoadState
        {
            get { return loadState; }
            set
            {
                if (loadState != value)
                {
                    loadState = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("LoadState"));
                }
            }
        }

        private object value;
        public object Value
        {
            get { return value; }
            set
            {
                if (Equals(this.value, value) == false)
                {
                    this.value = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        private ICommand loadContentCommand;
        public ICommand LoadContentCommand
        {
            get
            {
                if (loadContentCommand == null)
                    loadContentCommand = new AnonymousCommand(null, LoadContent);
                return loadContentCommand;
            }
        }

        private ICommand cancelContentLoadingCommand;
        public ICommand CancelContentLoadingCommand
        {
            get
            {
                if (cancelContentLoadingCommand == null)
                    cancelContentLoadingCommand = new AnonymousCommand(null, CancelContentLoading);
                return cancelContentLoadingCommand;
            }
        }

        private CancellationTokenSource cts;

        private async void LoadContent()
        {
            cts = new CancellationTokenSource();
            try
            {
                LoadState = ViewModelContentState.NotLoaded;
                await TaskEx.Delay(1000, cts.Token);
                LoadState = ViewModelContentState.Loading;
                await TaskEx.Delay(2000, cts.Token);
                LoadState = ViewModelContentState.Loaded;
                Value = "Data Loaded!";
            }
            catch (TaskCanceledException)
            {
                LoadState = ViewModelContentState.NotLoaded;
            }
        }

        private void CancelContentLoading()
        {
            if (cts != null)
                cts.Cancel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
