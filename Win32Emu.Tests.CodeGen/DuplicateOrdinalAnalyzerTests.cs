using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Win32Emu.Generators;

namespace Win32Emu.Tests.CodeGen;

public class DuplicateOrdinalAnalyzerTests
{
	[Fact]
	public async Task NoDuplicates_ShouldNotProduceDiagnostics()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Win32.Modules
{
	using Win32Emu.Win32;

	public class TestModule
	{
		[DllModuleExport(1)]
		private uint Function1() => 0;

		[DllModuleExport(2)]
		private uint Function2() => 0;

		[DllModuleExport(3)]
		private uint Function3() => 0;
	}
}";

		await VerifyAnalyzerAsync(testCode);
	}

	[Fact]
	public async Task SameOrdinalDifferentVersions_ShouldNotProduceDiagnostics()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Win32.Modules
{
	using Win32Emu.Win32;

	public class TestModule
	{
		[DllModuleExport(1, Version = ""4.90.0.3000"")]
		[DllModuleExport(2, Version = ""5.1.2600.6532"")]
		private uint Function1() => 0;

		[DllModuleExport(1, Version = ""6.0.0.0"")]
		private uint Function2() => 0;
	}
}";

		await VerifyAnalyzerAsync(testCode);
	}

	[Fact]
	public async Task DuplicateOrdinalSameVersion_ShouldProduceDiagnostics()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Win32.Modules
{
	using Win32Emu.Win32;

	public class TestModule
	{
		[DllModuleExport(1, Version = ""4.90.0.3000"")]
		private uint Function1() => 0;

		[DllModuleExport(1, Version = ""4.90.0.3000"")]
		private uint Function2() => 0;
	}
}";

		await VerifyAnalyzerAsync(testCode,
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(21, 4, 21, 47)
				.WithArguments("1", "TestModule", "4.90.0.3000"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(24, 4, 24, 47)
				.WithArguments("1", "TestModule", "4.90.0.3000"));
	}

	[Fact]
	public async Task DuplicateOrdinalNoVersion_ShouldProduceDiagnostics()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Win32.Modules
{
	using Win32Emu.Win32;

	public class TestModule
	{
		[DllModuleExport(37)]
		private uint Function1() => 0;

		[DllModuleExport(37)]
		private uint Function2() => 0;
	}
}";

		await VerifyAnalyzerAsync(testCode,
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(21, 4, 21, 23)
				.WithArguments("37", "TestModule", "(no version specified)"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(24, 4, 24, 23)
				.WithArguments("37", "TestModule", "(no version specified)"));
	}

	[Fact]
	public async Task MultipleDuplicateOrdinals_ShouldProduceDiagnosticsForAll()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Win32.Modules
{
	using Win32Emu.Win32;

	public class TestModule
	{
		[DllModuleExport(4)]
		private uint Function1() => 0;

		[DllModuleExport(4)]
		private uint Function2() => 0;

		[DllModuleExport(4)]
		private uint Function3() => 0;

		[DllModuleExport(16)]
		private uint Function4() => 0;

		[DllModuleExport(16)]
		private uint Function5() => 0;
	}
}";

		await VerifyAnalyzerAsync(testCode,
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(21, 4, 21, 22)
				.WithArguments("4", "TestModule", "(no version specified)"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(24, 4, 24, 22)
				.WithArguments("4", "TestModule", "(no version specified)"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(27, 4, 27, 22)
				.WithArguments("4", "TestModule", "(no version specified)"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(30, 4, 30, 23)
				.WithArguments("16", "TestModule", "(no version specified)"),
			DiagnosticResult.CompilerWarning(DuplicateOrdinalAnalyzer.DiagnosticId)
				.WithSpan(33, 4, 33, 23)
				.WithArguments("16", "TestModule", "(no version specified)"));
	}

	[Fact]
	public async Task NonModuleClass_ShouldNotBeAnalyzed()
	{
		var testCode = @"
using System;

namespace Win32Emu.Win32
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DllModuleExportAttribute : Attribute
	{
		public uint Ordinal { get; }
		public string? Version { get; init; }
		public DllModuleExportAttribute(uint ordinal) => Ordinal = ordinal;
	}
}

namespace Win32Emu.Tests
{
	using Win32Emu.Win32;

	public class TestClass
	{
		[DllModuleExport(1)]
		private uint Function1() => 0;

		[DllModuleExport(1)]
		private uint Function2() => 0;
	}
}";

		// Should not produce diagnostics because class name doesn't end with "Module"
		await VerifyAnalyzerAsync(testCode);
	}

	private static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<DuplicateOrdinalAnalyzer, DefaultVerifier>
		{
			TestCode = source,
			ReferenceAssemblies = ReferenceAssemblies.Net.Net90
		};

		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync();
	}
}
