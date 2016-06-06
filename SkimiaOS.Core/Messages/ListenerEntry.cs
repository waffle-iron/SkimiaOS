using System;

namespace SkimiaOS.Core.Messages
{
    [Flags]
    public enum ListenerEntry
    {
        Undefined = 0,
        Local = 1,
        Client = 2,
        Server = 4,
    }
}
