// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Games.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Games.ViewModel;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Serialization.Packages;
using SiliconStudio.Xenko.Games.IO;

namespace ScriptTest
{
    class AssetBrowser
    {
        public AssetBrowser(EngineContext engineContext)
        {
            //engineContext.VirtualFileSystem.CreateWatcher("/");
        }
    }

    class FileTracker
    {
        public SortedSet<string> Files = new SortedSet<string>();

        public void Setup(string baseUrl)
        {
            VirtualFileSystem.ListFiles(baseUrl, "*.*", VirtualSearchOption.AllDirectories)
                .ContinueWith(task =>
                {
                    foreach (var url in task.Result)
                    {
                        if (IsExtensionSupported(url))
                        {
                            lock (Files)
                            {
                                Files.Add(url);
                            }
                        }
                    }

                    var watcher = VirtualFileSystem.CreateWatcher(baseUrl, "*.*");
                    watcher.Changed += watcher_Changed;
                    watcher.Enable = true;
                });
        }

        void watcher_Changed(VirtualWatcherChangeTypes watcherChangeTypes, string url)
        {
            if (!IsExtensionSupported(url))
                return;

            if ((watcherChangeTypes & VirtualWatcherChangeTypes.Created) != 0)
                Files.Add(url);

            if ((watcherChangeTypes & VirtualWatcherChangeTypes.Deleted) != 0)
                Files.Remove(url);

            //if ((watcherChangeTypes & WatcherChangeTypes.Renamed) != 0)
            //    Files.Remove(url);
        }

        public static bool IsExtensionSupported(string url)
        {
            var urlAsLowerCase = url.ToLowerInvariant();
            return (urlAsLowerCase.EndsWith(".png")
                || urlAsLowerCase.EndsWith(".jpg")
                //|| urlAsLowerCase.EndsWith(".fbx")
                || urlAsLowerCase.EndsWith(".xksl"));
        }
    }

    class AssetBrowserEnumerator : IChildrenPropertyEnumerator
    {
        private EngineContext engineContext;
        private FileTracker fileTracker;
        private string searchFilter = string.Empty;
        private string searchRoot = string.Empty;
        private Task<string[]> searchResults;

        public AssetBrowserEnumerator(EngineContext engineContext)
        {
            this.engineContext = engineContext;
        }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if ((viewModelNode.NodeValue as string) == "Root")
            {
                viewModelNode.Children.Add(new ViewModelNode("SearchResults", new EnumerableViewModelContent<ViewModelReference>(() =>
                    searchResults != null && searchResults.IsCompleted ? searchResults.Result.Select(searchResult => new ViewModelReference(KeyValuePair.Create(UrlType.SearchResult, searchResult), true))
                                                                       : new ViewModelReference[] { })));
                viewModelNode.Children.Add(new ViewModelNode("SearchFilter",
                    new LambdaViewModelContent<string>(
                        () => searchFilter,
                        (newFilter) =>
                        {
                            searchFilter = newFilter;
                            StartSearch();
                        })));
                viewModelNode.Children.Add(new ViewModelNode("Packages",
                    new EnumerableViewModelContent<ViewModelReference>(() => engineContext.PackageManager.Packages.Select(package => new ViewModelReference(package, true)))));

                fileTracker = new FileTracker();
                fileTracker.Setup("/global_data");
                fileTracker.Setup("/global_data2");
                viewModelNode.Children.Add(new ViewModelNode("FileTracker", new RootViewModelContent(fileTracker)).GenerateChildren(context));
            }
            if (viewModelNode.Type == typeof(FileTracker))
            {
                viewModelNode.Content.SerializeFlags = ViewModelContentSerializeFlags.None;
                viewModelNode.Children.Add(new ViewModelNode("RootFolder", KeyValuePair.Create(UrlType.FileTracker, "/")).GenerateChildren(context));
                handled = true;
            }
            if (viewModelNode.NodeValue is Package)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(Package).GetProperty("Name"))));
                handled = true;
            }

            if (viewModelNode.NodeValue is Tuple<UrlType, string>)
            {
                var nodeValue = (Tuple<UrlType, string>)viewModelNode.NodeValue;
                var url = nodeValue.Item2;

                if (nodeValue.Item1 == UrlType.SearchResult)
                {
                    // Load thumbnail (not cached yet)
                    if (url.EndsWith(".png") || url.EndsWith(".jpg"))
                    {
                        //var textureData = engineContext.ContentManager.LoadAsync<Image>(url);
                        var thumbnail = new ViewModelNode("Thumbnail",
                            new AsyncViewModelContent<Image>(new NullViewModelContent(), operand => engineContext.AssetManager.Load<Image>(url)));
                        viewModelNode.Children.Add(thumbnail);
                        /*textureData.ContinueWith(task =>
                            {
                                thumbnail.Value = task.Result;
                                thumbnail.Content.Flags |= ViewModelFlags.Static;
                            });*/
                    }
                    /*else
                    {
                        throw new NotImplementedException();
                    }*/

                    viewModelNode.Content = new RootViewModelContent(url);
                    viewModelNode.Content.SerializeFlags = ViewModelContentSerializeFlags.Serialize;
                }
                else if (nodeValue.Item1 == UrlType.FileTracker)
                {
                    viewModelNode.Content = new RootViewModelContent(url);
                    viewModelNode.Content.SerializeFlags = ViewModelContentSerializeFlags.Serialize;

                    viewModelNode.Children.Add(new ViewModelNode("SetAsSearchFilter", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                        {
                            searchRoot = url;
                            StartSearch();
                        }))));

                    if (url.EndsWith("/"))
                    {
                        viewModelNode.Children.Add(new ViewModelNode("Folders", new EnumerableViewModelContent<ViewModelReference>(() =>
                                fileTracker.Files
                                    .Where(file => file.StartsWith(url))
                                    .GroupBy(file =>
                                        {
                                            var separatorIndex = file.IndexOf('/', url.Length + 1);
                                            return file.Substring(url.Length, separatorIndex != -1 ? separatorIndex - url.Length + 1 : file.Length - url.Length);
                                        })
                                    .Where(x => x.Key.EndsWith("/") || x.Key.EndsWith(".dat") || x.Key.EndsWith(".xksl"))
                                    .Select(x => new ViewModelReference(KeyValuePair.Create(UrlType.FileTracker, url + x.Key), this))
                            )));
                    }
                }

                handled = true;
            }
        }

        public IEnumerable<string> EnumeratePackageObjects(Package package, string filter)
        {
            var packageUrl = package.Source.Url;
            return package.ObjectHeaders
                .Select(packageObject => packageUrl + "/" + packageObject.Name);
        }

        public void StartSearch()
        {
            if (searchRoot.EndsWith("/"))
            {
                searchResults = VirtualFileSystem
                    .ListFiles(searchRoot, searchFilter + "*.*", VirtualSearchOption.AllDirectories)
                    .ContinueWith(task =>
                        {
                            return task.Result.Where(FileTracker.IsExtensionSupported)
                                .Concat(engineContext.PackageManager.Packages
                                    .Where(x => x.Source.Url.StartsWith(searchRoot))
                                    .SelectMany(x => EnumeratePackageObjects(x, searchFilter)))
                                .ToArray();
                        });
            }
            else
            {
                // User selected a file (must be a package since we list only directory and packages)
                var package = engineContext.PackageManager.Packages.FirstOrDefault(x => x.Source.Url == searchRoot);
                if (package != null)
                {
                    searchResults = Task<string[]>.Factory.StartNew(() => EnumeratePackageObjects(package, searchFilter).ToArray());
                }
            }
        }

        private enum UrlType
        {
            SearchResult = 0,
            FileTracker = 1,
        }
    }
}
