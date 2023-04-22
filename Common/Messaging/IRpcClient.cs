namespace Common.Messaging;
public interface IRpcClient<TResponse>
{
	Task<TResponse> SendRequest(RpcMessage message,
		Func<RpcMessage, TResponse> responseProcessor,
		Func<Exception, TResponse> errorHandler,
		CancellationToken cancellationToken);
}
