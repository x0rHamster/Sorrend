using System;
using System.Threading.Tasks;

namespace Sorrend.IntegrationTests.Utilities
{
    public abstract class AsyncInitializingProvider<TService> : IDisposable, IAsyncDisposable
    {
        private readonly Lazy<Task<TService>> _lazyInstantiationTask;

        protected AsyncInitializingProvider()
        {
            _lazyInstantiationTask = new Lazy<Task<TService>>(CreateInitializedAsync);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeCoreAsync();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (
                disposing
                && _lazyInstantiationTask.IsValueCreated
                && _lazyInstantiationTask.Value.IsCompleted
                && _lazyInstantiationTask.Value.Result is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected virtual async ValueTask DisposeCoreAsync()
        {
            if (!_lazyInstantiationTask.IsValueCreated)
            {
                return;
            }

            switch (await _lazyInstantiationTask.Value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;

                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        public async Task<TService> GetAsync()
            => await _lazyInstantiationTask.Value;

        protected abstract Task<TService> CreateInitializedAsync();
    }
}
