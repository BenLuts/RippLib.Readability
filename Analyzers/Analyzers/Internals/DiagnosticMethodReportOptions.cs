using System;

namespace Analyzers;

[Flags]
public enum DiagnosticMethodReportOptions
{
    None = 0x0,
    ReportOnMethodName = 0x1,
    ReportOnReturnType = 0x2,
}
