[assembly: CollectionBehavior(DisableTestParallelization = false)]

// Configure xUnit to run tests in parallel but with limited concurrency for integration tests
[assembly: System.Reflection.AssemblyMetadata("ParallelTestExecution", "true")]