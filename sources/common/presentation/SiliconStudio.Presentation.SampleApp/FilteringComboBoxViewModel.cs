// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.SampleApp
{
    public class FilteringComboBoxViewModel : ViewModelBase
    {
        public class FilteringComboBoxItemViewModel : ViewModelBase
        {
            private string text;
            private int num;
            private static IEnumerator<string> strings = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Select(x => x.Name).GetEnumerator();
            public FilteringComboBoxItemViewModel()
            {
                if (!strings.MoveNext())
                {
                    strings = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Select(x => x.Name).GetEnumerator();
                    strings.MoveNext();
                }
                Text = strings.Current;
                Num = new Random().Next();
            }

            public override string ToString()
            {
                return string.Format("{0}_{1}", text, num);
            }

            public string Text { get { return text; } set { SetValue(ref text, value); } }

            public int Num { get { return num; } set { SetValue(ref num, value); } }
        }

        private string text;
        private NonGenericObservableListWrapper<FilteringComboBoxItemViewModel> names = new NonGenericObservableListWrapper<FilteringComboBoxItemViewModel>(new ObservableList<FilteringComboBoxItemViewModel>());

        public FilteringComboBoxViewModel()
        {
            for (int i = 0; i < 500; ++i)
            {
                names.Add(new FilteringComboBoxItemViewModel());
            }
        }

        public string Text { get { return text; } set { SetValue(ref text, value); } }

        public NonGenericObservableListWrapper<FilteringComboBoxItemViewModel> Names { get { return names; } set { SetValue(ref names, value); } }
    }
}