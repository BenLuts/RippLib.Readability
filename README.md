# RippLib.Readability
Small library with usefull methods that help display intent in code & keep it more readable and clear.

To install use NuGet package manager or 
```
dotnet add package RippLib.Readability
```

All extentions reside in the RippLib.Readability namespace.
If you are using Global Usings simply add 
```
RippLib.Readability
```

## Current available methods:

```
.Empty()
```
Checks if a collection is null or contains no elements. Uses optimized checks if the collection is of type List or Array.
Instead of writing if (myCollection == null || !myCollection.Any()) use if (myCollection.Empty())

```
.Has()
```
Direct replacement for LINQ .Any method, accepts a Lambda expression, has a build in NULL check.

```
.NotEmpty()
```
Checks if a list is not null and has at least one element. Uses optimized checks if the collection is of type List or Array.
Instead of writing if (myCollection != null || myCollection.Any()) use if (myCollection.NotEmpty())

```
.HasNo()
```
Direct replacement for LINQ !.Any method, accepts a Lambda expression, has a build in NULL check.

```
.HasBeenCanceled()
```
More readable method that wraps checks for Task Cancelation.

[![Release to NuGet](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml/badge.svg)](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml)
