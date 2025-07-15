using System;

namespace Analyzers;

[Flags]
public enum DiagnosticFieldReportOptions
{
    None = 0x0,
    ReportOnReturnType = 0x1,
}
