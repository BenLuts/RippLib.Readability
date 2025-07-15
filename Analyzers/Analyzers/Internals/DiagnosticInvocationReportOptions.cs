using System;

namespace Analyzers;

[Flags]
public enum DiagnosticInvocationReportOptions
{
    None = 0x0,
    ReportOnMember = 0x1,
}
