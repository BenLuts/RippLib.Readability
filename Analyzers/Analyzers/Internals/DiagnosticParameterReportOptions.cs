using System;

namespace Analyzers;

[Flags]
public enum DiagnosticParameterReportOptions
{
    None = 0x0,
    ReportOnType = 0x1,
}
