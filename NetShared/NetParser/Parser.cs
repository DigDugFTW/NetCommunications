using NetShared.NetObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.NetParser
{
    // TODO add remove method for dictionary
    public class GenericDictionary
    {
        public Dictionary<object, object> Dictionary { get; private set; } = new Dictionary<object, object>();

        public void Add<TKey, TValue>(TKey key, TValue value) where TValue : class
        {
            Dictionary.Add(key, value);
        }

      
        // Handle cast from tValve as TValue?
        public bool TryGetValue<TKey, TValue>(TKey key, out TValue t) where TValue : class
        {
            bool keyPresent = Dictionary.TryGetValue(key, out object tValue);
            t = tValue as TValue;
            return keyPresent;

        }

    }

    public class TypeHandler
    {
        public Delegate HandlerDelegate { get; set; }

        public GenericDictionary Children = new GenericDictionary();
    }

    public class TypeHandlerGeneric<THandler> : TypeHandler
    {
        public TypeHandlerGeneric<THandler> AddChild<TLookUp, TReturn>(TLookUp lookUp, Func<ParserArgs, THandler, TReturn> child)
        {
            Children.Add(lookUp, child);
            return this;
        }

    }




    public class Parser : IParser
    {

        public readonly Dictionary<Type, TypeHandler> TypeHandlerDictionary = new Dictionary<Type, TypeHandler>();

        public TypeHandlerGeneric<THandler> AddHandler<THandler, TReturn>(Func<ParserArgs, THandler, TReturn> handler)
        {
            var typeHandler = new TypeHandlerGeneric<THandler>() { HandlerDelegate = handler };
            TypeHandlerDictionary.Add(typeof(THandler), typeHandler);
            return typeHandler;
        }


        public TypeHandlerGeneric<THandler> GetHandler<THandler>()
        {
            bool valPresent = TypeHandlerDictionary.TryGetValue(typeof(THandler), out TypeHandler val);
            if (valPresent)
            {
                return val as TypeHandlerGeneric<THandler>;
            }
            throw new Exception($"GetHandler couldn't find handler of type \"{typeof(THandler)}\"");
        }

        public async Task<TReturn> CallHandlerChild<THandler, TReturn, TLookup>(TLookup lookUp, params object[] args)
        {
            bool typePresent = TypeHandlerDictionary.TryGetValue(typeof(THandler), out TypeHandler typeHandler);
            
            bool childPresent = typeHandler.Children.TryGetValue(lookUp, out Func<ParserArgs, THandler, TReturn> func);

            
            if(typePresent && childPresent)
                return await Task.FromResult((TReturn)func.DynamicInvoke(args));

            throw new Exception("CallHandlerChild failed to find child parent type or child type.");
        }



        
        public async Task<TResponse> ParseType<TResponse>(ParserArgs args, object obj) where TResponse : ResponseBase
        {
            try
            {
                bool typeValid = TypeHandlerDictionary.TryGetValue(obj.GetType(), out TypeHandler typeHandler);
                if (typeValid)
                {
                    Delegate handlerDelegate = typeHandler.HandlerDelegate;

                    var ret = await Task.FromResult(handlerDelegate.DynamicInvoke(args, obj));
                    if (ret.GetType() == typeof(Task<TResponse>))
                    {
                        return (ret as Task<TResponse>).Result;
                    }
               
                    return ret as TResponse;

                }
                throw new Exception($"Deserialized type unknown to parser. {obj.GetType()}");
            }
            catch (Exception e)
            {
                return new ResponseBase() { HasError = true, Exception = e, StatusCode = StatusCode.ERROR } as TResponse;
            }

       
        }


    }
}
