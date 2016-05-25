using CQRSCore.Messages;

namespace CQRSCore.Commands
{
    public interface ICommand : IMessage
    {
        int ExpectedVersion { get; set; }
    }
}