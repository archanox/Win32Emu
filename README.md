# Win32Emu

A Win32 PE emulator written in C# .NET 9.0.

## CI/CD

This project uses GitHub Actions for continuous integration and deployment:

### Continuous Integration (CI)
- **Triggers**: Pull requests and pushes to `main` and `develop` branches
- **Actions**:
  - Builds the solution in Release configuration
  - Runs code quality analysis
  - Executes tests (if any exist)
  - Reports build status and warnings

### Continuous Deployment (CD)
- **Triggers**: Version tags pushed to the repository (format: `v*`)
- **Actions**:
  - Builds release artifacts for Linux x64 and Windows x64
  - Creates self-contained executables (no .NET runtime dependency required)
  - Packages binaries into downloadable archives
  - Creates GitHub releases with attached binaries
  - Supports pre-release detection for alpha/beta/rc tags

### Creating a Release
To create a new release:
1. Tag your commit with a version: `git tag v1.0.0`
2. Push the tag: `git push origin v1.0.0`
3. The CD workflow will automatically create a GitHub release with downloadable binaries