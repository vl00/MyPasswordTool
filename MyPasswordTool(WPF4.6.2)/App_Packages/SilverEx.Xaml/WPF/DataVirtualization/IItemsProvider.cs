using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilverEx.DataVirtualization
{
    /// <summary>
    /// Represents a provider of collection details.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface IItemsProvider<T>
    {
        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        Task<int> FetchCount();

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The number of items to fetch.</param>
        /// <returns></returns>
        Task<IList<T>> FetchRange(int startIndex, int length);//, out int overallCount);
    }

    public class FuncItemsProvider<T> : IItemsProvider<T>
    {
        public FuncItemsProvider(Func<int, int, Task<IEnumerable<T>>> fetchRange, Func<Task<int>> fetchCount)
        {
            _func_FetchRange = fetchRange;
            _func_FetchCount = fetchCount;
        }

        private readonly Func<Task<int>> _func_FetchCount;
        private readonly Func<int, int, Task<IEnumerable<T>>> _func_FetchRange;

        public Task<int> FetchCount()
        {
            return _func_FetchCount();
        }

        public async Task<IList<T>> FetchRange(int startIndex, int length)
        {
            var ls = await _func_FetchRange(startIndex, length);
            return ls is IList<T> l ? l :
                ls == null ? new List<T>() :
                ls.ToList();
        }
    }
}
