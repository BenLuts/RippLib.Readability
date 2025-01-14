using System;

namespace Analyzers;

[Flags]
public enum DiagnosticPropertyReportOptions
{
    None = 0x0,
    ReportOnReturnType = 0x1,
}
