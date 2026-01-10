using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Win32Emu.Generators;

namespace Win32Emu.Tests.CodeGen;

public class StringPointerToStringAnalyzerTests
{
	[Fact]
	public async Task LpcStrToString_ShouldProduceDiagnostic()
	{
		var testCode = @"
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public readonly struct LpcStr
	{
		public readonly uint Address;
		public LpcStr(uint address) => Address = address;
		public string? Read(VirtualMemory? mem = null) => null;
	}
}

namespace Win32Emu.Memory
{
	public class VirtualMemory { }
}

namespace Test
{
	using Win32Emu.Win32;
	using Win32Emu.Memory;

	public class TestClass
	{
		public void TestMethod(LpcStr str, VirtualMemory memory)
		{
			var s = {|#0:str.ToString()|};
		}
	}
}";

		var expected = DiagnosticResult.CompilerError(StringPointerToStringAnalyzer.DiagnosticId)
			.WithLocation(0)
			.WithArguments("LpcStr");

		await VerifyAnalyzerAsync(testCode, expected);
	}

	[Fact]
	public async Task LpcWStrToString_ShouldProduceDiagnostic()
	{
		var testCode = @"
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public readonly struct LpcWStr
	{
		public readonly uint Address;
		public LpcWStr(uint address) => Address = address;
		public string? Read(VirtualMemory? mem = null) => null;
	}
}

namespace Win32Emu.Memory
{
	public class VirtualMemory { }
}

namespace Test
{
	using Win32Emu.Win32;
	using Win32Emu.Memory;

	public class TestClass
	{
		public void TestMethod(LpcWStr str, VirtualMemory memory)
		{
			var s = {|#0:str.ToString()|};
		}
	}
}";

		var expected = DiagnosticResult.CompilerError(StringPointerToStringAnalyzer.DiagnosticId)
			.WithLocation(0)
			.WithArguments("LpcWStr");

		await VerifyAnalyzerAsync(testCode, expected);
	}

	[Fact]
	public async Task LpWStrToString_ShouldProduceDiagnostic()
	{
		var testCode = @"
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public readonly struct LpWStr
	{
		public readonly uint Address;
		public LpWStr(uint address) => Address = address;
		public string Read(VirtualMemory mem) => string.Empty;
	}
}

namespace Win32Emu.Memory
{
	public class VirtualMemory { }
}

namespace Test
{
	using Win32Emu.Win32;
	using Win32Emu.Memory;

	public class TestClass
	{
		public void TestMethod(LpWStr str, VirtualMemory memory)
		{
			var s = {|#0:str.ToString()|};
		}
	}
}";

		var expected = DiagnosticResult.CompilerError(StringPointerToStringAnalyzer.DiagnosticId)
			.WithLocation(0)
			.WithArguments("LpWStr");

		await VerifyAnalyzerAsync(testCode, expected);
	}

	[Fact]
	public async Task LpcStrRead_ShouldNotProduceDiagnostic()
	{
		var testCode = @"
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public readonly struct LpcStr
	{
		public readonly uint Address;
		public LpcStr(uint address) => Address = address;
		public string? Read(VirtualMemory? mem = null) => null;
	}
}

namespace Win32Emu.Memory
{
	public class VirtualMemory { }
}

namespace Test
{
	using Win32Emu.Win32;
	using Win32Emu.Memory;

	public class TestClass
	{
		public void TestMethod(LpcStr str, VirtualMemory memory)
		{
			var s = str.Read(memory);
		}
	}
}";

		await VerifyAnalyzerAsync(testCode);
	}

	[Fact]
	public async Task RegularStringToString_ShouldNotProduceDiagnostic()
	{
		var testCode = @"
namespace Test
{
	public class TestClass
	{
		public void TestMethod(string str)
		{
			var s = str.ToString();
		}
	}
}";

		await VerifyAnalyzerAsync(testCode);
	}

	[Fact]
	public async Task OtherTypeToString_ShouldNotProduceDiagnostic()
	{
		var testCode = @"
namespace Test
{
	public class TestClass
	{
		public void TestMethod(int value)
		{
			var s = value.ToString();
		}
	}
}";

		await VerifyAnalyzerAsync(testCode);
	}

	[Fact]
	public async Task MultipleViolations_ShouldProduceMultipleDiagnostics()
	{
		var testCode = @"
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public readonly struct LpcStr
	{
		public readonly uint Address;
		public LpcStr(uint address) => Address = address;
		public string? Read(VirtualMemory? mem = null) => null;
	}

	public readonly struct LpcWStr
	{
		public readonly uint Address;
		public LpcWStr(uint address) => Address = address;
		public string? Read(VirtualMemory? mem = null) => null;
	}
}

namespace Win32Emu.Memory
{
	public class VirtualMemory { }
}

namespace Test
{
	using Win32Emu.Win32;
	using Win32Emu.Memory;

	public class TestClass
	{
		public void TestMethod(LpcStr str1, LpcWStr str2, VirtualMemory memory)
		{
			var s1 = {|#0:str1.ToString()|};
			var s2 = {|#1:str2.ToString()|};
		}
	}
}";

		var expected1 = DiagnosticResult.CompilerError(StringPointerToStringAnalyzer.DiagnosticId)
			.WithLocation(0)
			.WithArguments("LpcStr");

		var expected2 = DiagnosticResult.CompilerError(StringPointerToStringAnalyzer.DiagnosticId)
			.WithLocation(1)
			.WithArguments("LpcWStr");

		await VerifyAnalyzerAsync(testCode, expected1, expected2);
	}

	private static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<StringPointerToStringAnalyzer, DefaultVerifier>
		{
			TestCode = source,
			ReferenceAssemblies = ReferenceAssemblies.Net.Net90
		};

		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync();
	}
}
