using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// The WASM client hosts the docs' one interactive island — the /playground component,
// which compiles utility classes to CSS entirely in the browser via the pure-C#
// MonorailCss engine. The server project supplies every other (static SSR) page; here
// we only build the host so the island's render mode can activate in the browser.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();
