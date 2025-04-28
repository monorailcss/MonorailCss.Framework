namespace TryMonorail;

public class DebounceHelper
{
    private CancellationTokenSource? _debounceToken;

    public async Task<T?> DebounceAsync<T>(Func<CancellationToken, Task<T>> func, int milliseconds = 1000)
    {
        try
        {
            // Cancel previous task
            _debounceToken?.Cancel();

            // Assign new token
            _debounceToken = new CancellationTokenSource();

            // Debounce delay
            await Task.Delay(milliseconds, _debounceToken.Token);

            // Throw if canceled
            _debounceToken.Token.ThrowIfCancellationRequested();

            // Run function
            return await func(_debounceToken.Token);
        }
        catch (TaskCanceledException) { }

        return default;
    }
}