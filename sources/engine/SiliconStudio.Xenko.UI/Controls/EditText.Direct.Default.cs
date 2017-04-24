// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX

namespace SiliconStudio.Xenko.UI.Controls
{
    public partial class EditText
    {
        private static void InitializeStaticImpl()
        {
        }

        private void InitializeImpl()
        {
        }

        private int GetLineCountImpl()
        {
            if (Font == null)
                return 1;

            return text.Split('\n').Length;
        }

        private void OnMaxLinesChangedImpl()
        {
        }

        private void OnMinLinesChangedImpl()
        {
        }

        private void ActivateEditTextImpl()
        {
        }

        private void DeactivateEditTextImpl()
        {
            FocusedElement = null;
        }

        private void UpdateTextToEditImpl()
        {
        }

        private void UpdateInputTypeImpl()
        {
        }

        private void UpdateSelectionFromEditImpl()
        {
        }

        private void UpdateSelectionToEditImpl()
        {
        }
    }
}

#endif
