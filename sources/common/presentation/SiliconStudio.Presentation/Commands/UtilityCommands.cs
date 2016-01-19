using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    public static class UtilityCommands
    {
        private static readonly Lazy<ICommandBase> LazyOpenHyperlinkCommand = new Lazy<ICommandBase>(OpenHyperlinkCommandFactory);

        public static ICommandBase OpenHyperlinkCommand => LazyOpenHyperlinkCommand.Value;

        private static ICommandBase OpenHyperlinkCommandFactory()
        {
            // TODO: have a proper way to initialize the services (maybe at application startup)
            var serviceProvider = new ViewModelServiceProvider(new[] { new DispatcherService(Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher) });
            return new AnonymousCommand<string>(serviceProvider, OpenHyperlink, CanOpenHyperlink);
        }

        private static bool CanOpenHyperlink(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        private static void OpenHyperlink(string url)
        {
            Process.Start(url);
        }
    }
}
