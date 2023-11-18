using NetShared.NetObject;
using System;
using System.Threading.Tasks;

namespace NetShared.NetParser
{
    public interface IParser
    {
        TypeHandlerGeneric<THandler> AddHandler<THandler, TReturn>(Func<ParserArgs, THandler, TReturn> handler);
        TypeHandlerGeneric<THandler> GetHandler<THandler>();
        Task<TReturn> CallHandlerChild<THandler, TReturn, TLookup>(TLookup lookUp, params object[] args);
        Task<TResponse> ParseType<TResponse>(ParserArgs args, object obj) where TResponse : ResponseBase;


    }
}