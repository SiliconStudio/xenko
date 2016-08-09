namespace SiliconStudio.Core.Threading
{
    internal interface IPooledClosure
    {
        void AddReference();

        void Release();
    }
}