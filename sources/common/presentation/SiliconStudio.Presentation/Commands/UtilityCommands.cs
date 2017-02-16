using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    public static class UtilityCommands
    {
        private static readonly Lazy<ICommandBase> LazyOpenHyperlinkCommand = new Lazy<ICommandBase>(OpenHyperlinkCommandFactory);

        public static ICommandBase OpenHyperlinkCommand => LazyOpenHyperlinkCommand.Value;

        [NotNull]
        private static ICommandBase OpenHyperlinkCommandFactory()
        {
            // TODO: have a proper way to initialize the services (maybe at application startup)
            var serviceProvider = new ViewModelServiceProvider(new[] { new DispatcherService(Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher) });
            return new AnonymousCommand<string>(serviceProvider, OpenHyperlink, CanOpenHyperlink);
        }

        private static bool CanOpenHyperlink([CanBeNull] string url)
        {
            return !string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        private static void OpenHyperlink([NotNull] string url)
        {
            // see https://support.microsoft.com/en-us/kb/305703
            try
            {
                Process.Start(url);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (e.ErrorCode == -2147467259)
                    MessageBox.Show(e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
