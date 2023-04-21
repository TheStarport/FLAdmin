namespace Common.Messaging;
public class RpcMessage
{
    public RpcMessage(IDictionary<string, object> data)
    {
        Data = data;
    }

    public IDictionary<string, object> Data { get; }
}
