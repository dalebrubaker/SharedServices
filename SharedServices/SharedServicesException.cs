using System;

namespace BruSoftware.SharedServices;

[Serializable]
public class SharedServicesException : Exception
{
    public SharedServicesException()
    {
    }

    public SharedServicesException(string message)
        : base(message)
    {
    }

    public SharedServicesException(string message, Exception inner)
        : base(message, inner)
    {
    }
}