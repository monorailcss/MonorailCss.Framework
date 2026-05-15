using MonorailCss.Discovery;

// The discovery assembly carries class-shaped strings (templates, canonical fixtures) in its
// IL #US heap. Opt it out of discovery scanning so apps don't pick up the noise.
[assembly: MonorailCssNoScan]
