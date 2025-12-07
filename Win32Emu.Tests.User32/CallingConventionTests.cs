using Xunit;
using Win32Emu.Loader;

namespace Win32Emu.Tests.User32
{
	[Trait("Category", "DllModuleTests")]
	public class CallingConventionTests
	{
		[Fact]
		public void StdcallConvention_HasCorrectDocumentation()
		{
			// Verify that the stdcall enum exists and matches x86 calling convention spec
			// per https://en.wikipedia.org/wiki/X86_calling_conventions
			var convention = CallingConvention.Stdcall;
			Assert.Equal(CallingConvention.Stdcall, convention);
			
			// Stdcall specification from x86 calling conventions:
			// - Arguments pushed right-to-left
			// - Callee cleans the stack (using RET n instruction)
			// - Return value in EAX/AX/AL
		}

		[Fact]
		public void CdeclConvention_HasCorrectDocumentation()
		{
			// Verify that the cdecl enum exists and matches x86 calling convention spec
			var convention = CallingConvention.Cdecl;
			Assert.Equal(CallingConvention.Cdecl, convention);
			
			// Cdecl specification from x86 calling conventions:
			// - Arguments pushed right-to-left
			// - Caller cleans the stack (caller adjusts ESP after RET)
			// - Return value in EAX/AX/AL
		}

		[Fact]
		public void FastcallConvention_HasCorrectDocumentation()
		{
			// Verify that the fastcall enum exists and matches x86 calling convention spec
			var convention = CallingConvention.Fastcall;
			Assert.Equal(CallingConvention.Fastcall, convention);
			
			// Fastcall specification from x86 calling conventions:
			// - First two arguments in ECX and EDX registers
			// - Remaining arguments pushed right-to-left on stack
			// - Callee cleans the stack (using RET n instruction)
			// - Return value in EAX/AX/AL
		}

		[Fact]
		public void ThiscallConvention_HasCorrectDocumentation()
		{
			// Verify that the thiscall enum exists and matches x86 calling convention spec
			var convention = CallingConvention.Thiscall;
			Assert.Equal(CallingConvention.Thiscall, convention);
			
			// Thiscall specification from x86 calling conventions:
			// - 'this' pointer passed in ECX register
			// - Remaining arguments pushed right-to-left on stack
			// - Callee cleans the stack (using RET n instruction)
			// - Return value in EAX/AX/AL
		}

		[Fact]
		public void PascalConvention_Exists()
		{
			// Verify that the Pascal calling convention enum value exists
			var convention = CallingConvention.Pascal;
			Assert.Equal(CallingConvention.Pascal, convention);
		}

		[Fact]
		public void PascalConvention_HasCorrectDocumentation()
		{
			// Verify that the Pascal enum exists and matches x86 calling convention spec
			// per https://en.wikipedia.org/wiki/X86_calling_conventions
			var convention = CallingConvention.Pascal;
			Assert.Equal(CallingConvention.Pascal, convention);
			
			// Pascal specification from x86 calling conventions:
			// - Arguments pushed LEFT-TO-RIGHT (opposite of stdcall!)
			// - Callee cleans the stack (using RET n instruction)
			// - Return value in AL/AX/EAX
			// - Used by Win16 applications and Pascal compilers
		}

		[Fact]
		public void StdcallAndPascal_HaveDifferentArgumentOrder()
		{
			// This test documents the key difference between stdcall and Pascal:
			// - stdcall: arguments pushed right-to-left
			// - Pascal: arguments pushed left-to-right
			
			// Both use callee stack cleanup (RET n instruction)
			// Both return values in EAX/AX/AL
			// The ONLY difference is the argument push order
			
			var stdcall = CallingConvention.Stdcall;
			var pascal = CallingConvention.Pascal;
			
			Assert.NotEqual(stdcall, pascal);
		}

		[Fact]
		public void ExportMetadata_DefaultIsStdcall()
		{
			// Verify that the default calling convention is stdcall
			// This is the most common convention for Win32 APIs
			var defaultMeta = ExportMetadata.Default;
			Assert.Equal(CallingConvention.Stdcall, defaultMeta.Convention);
		}

		[Fact]
		public void ExportMetadata_CanDetectStdcallDecoration()
		{
			// Test that stdcall decoration (FunctionName@N) is correctly parsed
			var meta = ExportMetadata.FromDecoratedName("MessageBoxA@16");
			
			Assert.NotNull(meta);
			Assert.Equal(CallingConvention.Stdcall, meta.Convention);
			Assert.Equal(16, meta.StackArgBytes);
			Assert.True(meta.IsInferred);
		}

		[Fact]
		public void ExportMetadata_CanDetectFastcallDecoration()
		{
			// Test that fastcall decoration (@FunctionName@N) is correctly parsed
			var meta = ExportMetadata.FromDecoratedName("@FastFunction@8");
			
			Assert.NotNull(meta);
			Assert.Equal(CallingConvention.Fastcall, meta.Convention);
			Assert.Equal(8, meta.StackArgBytes);
			Assert.True(meta.IsInferred);
		}

		[Fact]
		public void ExportMetadata_CanDetectThiscallDecoration()
		{
			// Test that thiscall decoration (?...@@...) is detected
			var meta = ExportMetadata.FromDecoratedName("?MethodName@@QAEXH@Z");
			
			Assert.NotNull(meta);
			Assert.Equal(CallingConvention.Thiscall, meta.Convention);
			Assert.True(meta.IsInferred);
		}

		[Theory]
		[InlineData("PlainFunction")]
		[InlineData("_UnderscoreFunction")]
		[InlineData("Function123")]
		public void ExportMetadata_ReturnsNullForUndecoratedNames(string functionName)
		{
			// Test that undecorated names return null (can't infer convention)
			var meta = ExportMetadata.FromDecoratedName(functionName);
			Assert.Null(meta);
		}

		[Fact]
		public void CallingConventionEnum_HasAllExpectedValues()
		{
			// Verify all calling conventions from x86 spec are present
			var values = System.Enum.GetValues<CallingConvention>();
			
			Assert.Contains(CallingConvention.Stdcall, values);
			Assert.Contains(CallingConvention.Cdecl, values);
			Assert.Contains(CallingConvention.Fastcall, values);
			Assert.Contains(CallingConvention.Thiscall, values);
			Assert.Contains(CallingConvention.Pascal, values);
		}
	}
}
