using System;

public sealed class RequestCommand
{
	public Action<IResponse> callbackEvent;

	public IRequest serverRequest;

	public Type responseType;

	public int autoRetryCount;

	public bool errorReturnTitle;
}
