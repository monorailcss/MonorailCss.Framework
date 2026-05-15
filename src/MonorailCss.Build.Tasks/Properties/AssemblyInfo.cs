using MonorailCss.Discovery;

// The build-task assembly carries class-shaped strings in its IL #US heap. Opt it out of
// discovery scanning for consistency with the other framework assemblies.
[assembly: MonorailCssNoScan]
