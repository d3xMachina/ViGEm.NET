using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Nefarius.ViGEm.Client.Exceptions;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class VigemUnknownException : Exception
{
    public VigemUnknownException()
    {
    }

    public VigemUnknownException(string message)
        : base(message)
    {
    }

    public VigemUnknownException(string format, params object[] args)
        : base(string.Format(format, args))
    {
    }

    public VigemUnknownException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public VigemUnknownException(string format, Exception innerException, params object[] args)
        : base(string.Format(format, args), innerException)
    {
    }

    protected VigemUnknownException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}