using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CQRSCore.Infrastructure
{
    internal class PrivateReflectionDynamicObject : DynamicObject
    {
        public object RealObject { get; set; }
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static object WrapObjectIfNeeded(object o)
        {
            // Don't wrap primitive types, which don't have many interesting internal APIs
            if (o == null || o.GetType().GetTypeInfo().IsPrimitive || o is string)
                return o;

            return new PrivateReflectionDynamicObject { RealObject = o };
        }

        // Called when a method is called
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = WrapObjectIfNeeded(result);

            return true;
        }

        private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
        {
            var argtypes = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
                argtypes[i] = args[i].GetType();
            while (true)
            {
                var member = (from m in type.GetRuntimeMethods()
                             where m.Name == name 
                             && !m.IsStatic 
                             && (m.IsPrivate || m.IsPublic) 
                             let parameters = m.GetParameters()
                             where parameters.Length == argtypes.Length
                             && parameters.Select(p => p.ParameterType.Name).SequenceEqual(argtypes.Select(t => t.Name))
                             select m).FirstOrDefault();
                //var member = type.GetRuntimeMethod(name, argtypes);
                //var member = type.GetMethod(name, bindingFlags, null, argtypes, null);
                if (member != null) return member.Invoke(target, args);
                if (type.GetTypeInfo().BaseType == null) return null;
                type = type.GetTypeInfo().BaseType;
            }
        }
    }
}