using System;

namespace SimpleSQLEditor.Services.State
{
    public class OperationSourceService : IOperationSourceService
    {
        #region Fields

        private readonly AsyncLocal<string?> _currentSource = new();

        #endregion

        #region Properties

        public string CurrentSource => _currentSource.Value ?? "N/A";

        #endregion

        #region Methods & Events

        public IDisposable Begin(string source)
        {
            var previous = _currentSource.Value;
            _currentSource.Value = source;

            return new Scope(() => _currentSource.Value = previous);
        }

        private sealed class Scope : IDisposable
        {
            private readonly Action _onDispose;

            public Scope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }

        #endregion
    }
}