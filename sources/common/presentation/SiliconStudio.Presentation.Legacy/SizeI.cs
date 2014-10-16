// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.Legacy
{
    public struct SizeI
    {
        private static SizeI empty;
        public static SizeI Empty
        {
            get { return empty; }
        }

        static SizeI()
        {
            empty = new SizeI { isEmpty = true };
        }

        public SizeI(int x, int y)
        {
            this.isEmpty = false;
            this.x = x;
            this.y = y;
        }

        private int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Width
        {
            get { return x; }
            set { x = value; }
        }

        private int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public int Height
        {
            get { return y; }
            set { y = value; }
        }

        private bool isEmpty;
        public bool IsEmpty
        {
            get { return isEmpty; }
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", x, y);
        }
    }
}
