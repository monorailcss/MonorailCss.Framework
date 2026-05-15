using MonorailCss.Discovery;

// The framework assembly ships utility-class-shaped template strings (e.g. "bg-{color}-500")
// in its IL #US heap. Opt it out of discovery scanning so apps don't pick up the templates.
[assembly: MonorailCssNoScan]
