# Sorrend

Generates and injects [SemVer](https://semver.org/) or [CalVer](https://calver.org/) versions based on Git commits during the build of .NET projects.

## Install

1.  Install the **Sorrend.MsBuild** NuGet package in the project that needs to be versioned.

2.  If you are using a [non-SDK project](https://learn.microsoft.com/en-us/nuget/resources/check-project-format#check-the-project-format), remove the assembly version attributes from the source code. Typically, you need to delete lines from the `Properties\AssemblyInfo.cs` file that look like the following:

    ```csharp
    [assembly: AssemblyVersion("1.0.0.0")]
    [assembly: AssemblyFileVersion("1.0.0.0")]
    [assembly: AssemblyInformationalVersion("1.0.0")]
    ```

3.  If you want to switch to the Calendar Versioning or change the default values, create the [configuration file](#Configuration).

The version number will be generated automatically at the next build of the project.

### Dependencies

*   **.NET SDK** that is still supported according to the Microsoft Lifecycle Policy. For .NET Framework, version 4.7.2 is the minimum required.
*   **Git** installed and added to the system's PATH environment variable.
*   A full clone of your project repository with a working tree. Shallow clones, bare repositories, and sparse checkouts are not supported.

## Usage

**Semantic Versioning** is used by default:

*   Each regular commit produces a pre-release.
*   A project built from a commit with a version tag will have the version number from that tag.
*   A commit made after a release tag will produce a pre-release of the next patch release.

For example:

| Commit | Tag | Generated version | Comment |
| --- | --- | --- | --- |
| `356a192` | | `0.1.0-dev.1.r356a192b7913` | First commit in the repository |
| `da4b923` | | `0.1.0-dev.2.rda4b9237bacc` | |
| `77de68d` | | `0.1.0-dev.3.r77de68daecd8` | |
| `1b64538` | `v1.0.0` | `1.0.0+1b6453892473a467d07372d45eb05abc2031647a` | Explicit release |
| `ac3478d` | | `1.0.1-dev.1.rac3478d69a3c` | Pre-release of the assumed next release |
| `c1dfd96` | | `1.0.1-dev.2.rc1dfd96eea8c` | |
| `902ba3c` | `v1.1.0-dev.1` | `1.1.0-dev.1.r902ba3cda188` | Explicit start of development for release 1.1.0 |
| `fe5dbbc` | | `1.1.0-dev.2.rfe5dbbcea5ce` | |
| `0ade7c2` | `v1.1.0` | `1.1.0+0ade7c2cf97f75d009975f4d720d1fa6c19f4897` | |

Instead, you can use **Calendar Versioning**, which generates versions using commit dates. For example, the commit made on `Thu, 02 Feb 2020 02:22:20 +0000` will produce the version `2020.4622-r152470fe8b6c`. As before, version tags take precedence over dates.

## Configuration

The configuration is stored in the `sorrend.config.xml` file. It can be located in the repository root, in the solution directory, or in the project directory.

Sample configuration file:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Configuration>
  <VersioningScheme>CalendarVersioning</VersioningScheme>
</Configuration>
```

## Contributing

Thank you for your interest in this project. This is my personal sandbox and codebase, which I really enjoy working on. I'm sharing it in the hope that it might be useful to someone, but I'm primarily developing it for myself.

I work on this project in my spare time, and I don't have many opportunities for collaboration. Because of that, I am unable to review or manage Issues and Pull Requests. If you need quick feedback or guaranteed support, this project may not be the right fit.

If you'd like to make changes or even continue developing the project, feel free to create a fork and publish your own version. I will be happy if the project has independent branches with different directions and futures.

## License

MIT
