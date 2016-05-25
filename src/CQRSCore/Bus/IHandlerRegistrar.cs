using System;
using CQRSCore.Messages;

namespace CQRSCore.Bus
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T>(Action<T> handler) where T : IMessage;
    }
}