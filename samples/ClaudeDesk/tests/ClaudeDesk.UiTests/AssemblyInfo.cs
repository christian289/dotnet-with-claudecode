// UI automation tests drive a real app window with real UIA traffic —
// parallel test classes would launch overlapping app instances and interfere.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
