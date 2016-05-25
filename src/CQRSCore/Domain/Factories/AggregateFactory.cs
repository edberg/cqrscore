using System;
using CQRSCore.Domain.Exception;
using System.Reflection;
using System.Linq;

namespace CQRSCore.Domain.Factories
{
    internal static class AggregateFactory
    {
        public static T CreateAggregate<T>()
        {
            var typeInfo = typeof(T).GetTypeInfo();
            var ctor = typeInfo.DeclaredConstructors.SingleOrDefault(c => c.GetParameters().Length == 0);
            if (ctor == null) throw new MissingParameterLessConstructorException(typeof(T));
            return (T)ctor.Invoke(null);
        }
    }
}