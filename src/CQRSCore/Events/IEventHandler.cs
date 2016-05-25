using CQRSCore.Messages;

namespace CQRSCore.Events
{
	public interface IEventHandler<T> : IHandler<T> where T : IEvent
	{
	}
}