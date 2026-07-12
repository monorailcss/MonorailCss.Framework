namespace MonorailCss.Docs.Client;

/// <summary>
/// Debounces the live re-compile: typing fires a change event per keystroke, but a full
/// parse + CSS generation pass shouldn't run mid-word. Ported from tests/TryMonorail.
/// </summary>
internal sealed class DebounceHelper
{
    private CancellationTokenSource? _debounceToken;

    public async Task<T?> DebounceAsync<T>(Func<CancellationToken, Task<T>> func, int milliseconds = 1000)
    {
        try
        {
            // Cancel the previous pending run.
            _debounceToken?.Cancel();

            // Assign a new token for this run.
            _debounceToken = new CancellationTokenSource();

            // Wait out the quiet window.
            await Task.Delay(milliseconds, _debounceToken.Token);

            // Throw if a newer keystroke cancelled us.
            _debounceToken.Token.ThrowIfCancellationRequested();

            return await func(_debounceToken.Token);
        }
        catch (TaskCanceledException) { }

        return default;
    }
}
