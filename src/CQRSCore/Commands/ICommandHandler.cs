using CQRSCore.Messages;

namespace CQRSCore.Commands
{
	public interface ICommandHandler<in T> : IHandler<T> where T : ICommand
	{
	}
}