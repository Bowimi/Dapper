using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ITypeHandler = Dapper.SqlMapper.ITypeHandler;

namespace Dapper
{
    /// <summary>
    /// Manages additional on-demand Dapper type handlers.
    /// </summary>
    public static class DynamicTypeHandler
    {
        private class HandlerConfig(IEnumerable<Func<Type, ITypeHandler?>> chain)
        {
            public ConcurrentDictionary<Type, ITypeHandler?> TypeHandlers { get; } = new();

            public IReadOnlyList<Func<Type, ITypeHandler?>> Chain { get; } = chain.ToArray();


            public bool TryGetHandler(Type type, out ITypeHandler? handler)
            {
                foreach (var callback in Chain)
                {
                    handler = callback(type);
                    if (handler is not null) return true;
                }

                handler = null;
                return false;
            }
        }


        private volatile static HandlerConfig? config = null;

        /// <summary>
        /// Configure dynamic type handling.
        /// </summary>
        /// <param name="chain">Dynamic type handlers, invoked in sequence when a new type is discovered.</param>
        public static void Configure(params Func<Type, ITypeHandler?>[] chain)
        {
            config = new(chain);
        }

        /// <summary>
        /// Tries to get the type handler, creating one if necessary
        /// </summary>
        /// <param name="type">Type to retrieve the handler for</param>
        /// <param name="handler">Type handler</param>
        /// <returns></returns>
        public static bool TryGetHandler(Type type, out ITypeHandler? handler)
        {
            handler = null;
            return config?.TryGetHandler(type, out handler) ?? false;
        }
    }
}
