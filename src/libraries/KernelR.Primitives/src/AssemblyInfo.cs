using System.Runtime.CompilerServices;

// Allow other KernelR packages to access internal members
// (KernelRJsonContext is used by KernelR.Domain, KernelR.Application,
// and KernelR.AspNetCore for consistent error serialization).
[assembly: InternalsVisibleTo("KernelR.Domain")]
[assembly: InternalsVisibleTo("KernelR.Application")]
[assembly: InternalsVisibleTo("KernelR.AspNetCore")]
