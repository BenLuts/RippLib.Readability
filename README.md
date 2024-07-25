# RippLib.Readibility
Small library with usefull methods that help display intent in code & keep it more readable and clear.

To install use NuGet package manager or 
```
dotnet add package RippLib.Readibility
```

All extentions reside in the RippLib.Readibility namespace.
If you are using Global Usings simply add 
```
RippLib.Readibility
```

## Current available methods:

```
.Empty()
```
Same usage as Linq .Any(), also excepts a lambda expression. Has build in null check.
Instead of writing if (myCollection == null || !myCollection.Any()) use if (myCollection.Empty())

```
.NotEmpty()
```
Same usage as Linq .Any(), also excepts a lambda expression. Has build in null check.
Instead of writing if (myCollection != null || myCollection.Any()) use if (myCollection.NotEmpty())

[![Release to NuGet](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml/badge.svg)](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml)
