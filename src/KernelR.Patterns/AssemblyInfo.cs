using System.Runtime.CompilerServices;

// Allow other KernelR packages to access internal members
// (ErrorJsonConverter and ErrorSerialization are used by
// KernelR.Domain and KernelR.AspNetCore for consistent error serialization).

[assembly: InternalsVisibleTo("KernelR.Domain")]
[assembly: InternalsVisibleTo("KernelR.AspNetCore")]
