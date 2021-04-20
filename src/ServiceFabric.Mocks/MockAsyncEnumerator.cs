//using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Returns an <cref name="AsyncEnumerator" /> that asynchronously iterates through the collection.
    /// </summary>
    /// <returns>An <cref name="AsyncEnumerator" /> that can be used to asynchronously iterate through the collection.</returns>
    internal class MockAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly System.Collections.Generic.IEnumerator<T> _source;

        public MockAsyncEnumerator(System.Collections.Generic.IEnumerator<T> enumerator)
        {
            _source = enumerator;
        }

        public MockAsyncEnumerator(System.Collections.Generic.IEnumerable<T> enumerable)
        {
            _source = enumerable.GetEnumerator();
        }

        /// <inheritdoc />
        public T Current => _source.Current;

        /// <inheritdoc />
        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_source.MoveNext());
        }

        /// <inheritdoc />
        public void Reset()
        {
            _source.Reset();
        }
        /// <inheritdoc />
        public void Dispose()
        {
            _source.Dispose();
        }
    }
}