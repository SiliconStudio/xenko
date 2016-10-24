using System;

namespace FirstPersonShooter.Trigger
{
    public class TriggerGroupException : Exception
    {
        public TriggerGroupException(string ex) : base(ex) { }
    }
}
