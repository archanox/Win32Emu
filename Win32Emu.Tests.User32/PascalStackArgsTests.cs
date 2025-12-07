using Xunit;
using Win32Emu.Cpu;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32
{
	[Trait("Category", "DllModuleTests")]
	public class PascalStackArgsTests
	{
		[Fact]
		public void PascalStackArgs_ReversesParameterOrder()
		{
			// Arrange - Create a mock CPU and memory with test data
			var cpu = new MockCpu();
			var memory = new VirtualMemory(1024 * 1024); // 1MB
			
			// Set up stack with ESP at 0x1000
			var esp = 0x1000u;
			cpu.SetRegister("ESP", esp);
			
			// Write test parameters to stack as if Pascal convention was used
			// Pascal pushes left-to-right, so for function(param1=0xAAAA, param2=0xBBBB, param3=0xCCCC):
			// ESP+12 = 0xAAAA (param1, pushed first)
			// ESP+8  = 0xBBBB (param2, pushed second)
			// ESP+4  = 0xCCCC (param3, pushed last)
			memory.Write32(esp + 4, 0xCCCCCCCC);  // param3 at lowest offset
			memory.Write32(esp + 8, 0xBBBBBBBB);  // param2 in middle
			memory.Write32(esp + 12, 0xAAAAAAAA); // param1 at highest offset
			
			// Act - Read parameters using PascalStackArgs with 3 parameters
			var args = new PascalStackArgs(cpu, memory, 3);
			var param1 = args.UInt32(0);
			var param2 = args.UInt32(1);
			var param3 = args.UInt32(2);
			
			// Assert - Verify parameters are read in correct logical order
			Assert.Equal(0xAAAAAAAAu, param1); // Index 0 should read from highest offset (ESP+12)
			Assert.Equal(0xBBBBBBBBu, param2); // Index 1 should read from middle (ESP+8)
			Assert.Equal(0xCCCCCCCCu, param3); // Index 2 should read from lowest offset (ESP+4)
		}

		[Fact]
		public void PascalStackArgs_WorksWithDifferentParameterCounts()
		{
			// Arrange
			var cpu = new MockCpu();
			var memory = new VirtualMemory(1024 * 1024);
			var esp = 0x2000u;
			cpu.SetRegister("ESP", esp);
			
			// Test with 5 parameters
			memory.Write32(esp + 4, 0x11111111);   // param5
			memory.Write32(esp + 8, 0x22222222);   // param4
			memory.Write32(esp + 12, 0x33333333);  // param3
			memory.Write32(esp + 16, 0x44444444);  // param2
			memory.Write32(esp + 20, 0x55555555);  // param1
			
			// Act
			var args = new PascalStackArgs(cpu, memory, 5);
			
			// Assert
			Assert.Equal(0x55555555u, args.UInt32(0)); // param1 at ESP+20
			Assert.Equal(0x44444444u, args.UInt32(1)); // param2 at ESP+16
			Assert.Equal(0x33333333u, args.UInt32(2)); // param3 at ESP+12
			Assert.Equal(0x22222222u, args.UInt32(3)); // param4 at ESP+8
			Assert.Equal(0x11111111u, args.UInt32(4)); // param5 at ESP+4
		}

		[Fact]
		public void PascalStackArgs_Int32_HandlesSignedValues()
		{
			// Arrange
			var cpu = new MockCpu();
			var memory = new VirtualMemory(1024 * 1024);
			var esp = 0x3000u;
			cpu.SetRegister("ESP", esp);
			
			// Write signed values (negative numbers in two's complement)
			memory.Write32(esp + 4, 0xFFFFFFFF);  // -1
			memory.Write32(esp + 8, 0x7FFFFFFF);  // Max positive int32
			
			// Act
			var args = new PascalStackArgs(cpu, memory, 2);
			var param1 = args.Int32(0);
			var param2 = args.Int32(1);
			
			// Assert
			Assert.Equal(int.MaxValue, param1); // Should read 0x7FFFFFFF from ESP+8
			Assert.Equal(-1, param2);           // Should read 0xFFFFFFFF from ESP+4
		}

		[Fact]
		public void PascalStackArgs_SupportsAllHelperMethods()
		{
			// Arrange
			var cpu = new MockCpu();
			var memory = new VirtualMemory(1024 * 1024);
			var esp = 0x4000u;
			cpu.SetRegister("ESP", esp);
			
			// Write pointer values
			memory.Write32(esp + 4, 0x9000);   // param2 pointer
			memory.Write32(esp + 8, 0x8000);   // param1 pointer
			
			// Act
			var args = new PascalStackArgs(cpu, memory, 2);
			
			// Assert - All pointer methods should work
			Assert.Equal(0x8000u, args.Ptr(0));
			Assert.Equal(0x8000u, args.Lpstr(0));
			Assert.Equal(0x8000u, args.Lpcstr(0));
			Assert.Equal(0x9000u, args.Ptr(1));
		}

		[Fact]
		public void StdcallStackArgs_ForComparison_UsesForwardOrder()
		{
			// This test documents the difference between stdcall and Pascal
			// Arrange
			var cpu = new MockCpu();
			var memory = new VirtualMemory(1024 * 1024);
			var esp = 0x5000u;
			cpu.SetRegister("ESP", esp);
			
			// Same stack layout as PascalStackArgs test
			memory.Write32(esp + 4, 0xCCCCCCCC);  // At ESP+4
			memory.Write32(esp + 8, 0xBBBBBBBB);  // At ESP+8
			memory.Write32(esp + 12, 0xAAAAAAAA); // At ESP+12
			
			// Act - Read using stdcall StackArgs
			var stdcallArgs = new StackArgs(cpu, memory);
			var param1_stdcall = stdcallArgs.UInt32(0);
			var param2_stdcall = stdcallArgs.UInt32(1);
			var param3_stdcall = stdcallArgs.UInt32(2);
			
			// Assert - Stdcall reads in forward order (lowest offset first)
			Assert.Equal(0xCCCCCCCCu, param1_stdcall); // Index 0 reads from ESP+4
			Assert.Equal(0xBBBBBBBBu, param2_stdcall); // Index 1 reads from ESP+8
			Assert.Equal(0xAAAAAAAAu, param3_stdcall); // Index 2 reads from ESP+12
			
			// This is OPPOSITE of Pascal which would read:
			// Index 0 from ESP+12, Index 1 from ESP+8, Index 2 from ESP+4
		}
	}
}
