/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    public abstract class PropertyDefinition : DependencyObject
    {
        private IList targetProperties;

        internal PropertyDefinition()
        {
            targetProperties = new List<object>();
        }

        [TypeConverter(typeof(ListConverter))]
        public IList TargetProperties
        {
            get { return targetProperties; }
            set
            {
                if (IsLocked)
                    throw new InvalidOperationException(@"Cannot modify TargetProperties once the definition has beed added to a collection.");

                targetProperties = value;
            }
        }

        internal bool IsLocked { get; private set; }

        internal virtual void Lock()
        {
            if (IsLocked)
                return;

            // Just create a new copy of the properties target to ensure 
            // that the list doesn't ever get modified.

            var newList = new List<object>();
            if (targetProperties != null)
            {
                newList.AddRange(targetProperties.Cast<object>());
            }

            targetProperties = new ReadOnlyCollection<object>(newList);
            IsLocked = true;
        }
    }
}
