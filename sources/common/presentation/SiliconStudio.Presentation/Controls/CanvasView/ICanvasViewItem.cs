// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Controls
{
    public interface ICanvasViewItem
    {
        void Attach(CanvasView view);

        void Detach(CanvasView view);

        void Render(CanvasRenderer renderer);
    }
}
