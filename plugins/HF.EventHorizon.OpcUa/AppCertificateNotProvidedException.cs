using System.Runtime.Serialization;

namespace HF.EventHorizon.OpcUa;

public class AppCertificateNotProvidedException : Exception, ISerializable
{

    protected AppCertificateNotProvidedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
    }

    public AppCertificateNotProvidedException()
    {
    }

    public AppCertificateNotProvidedException(string message)
        : base(message)
    {
    }

    public AppCertificateNotProvidedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
