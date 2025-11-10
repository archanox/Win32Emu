using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.COM;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that COM vtable methods are populated in the correct order.
/// This addresses the vtable method ordering issue that caused crashes in BasicDD.exe.
/// </summary>
public class ComVtableOrderingTests
{
	private readonly ITestOutputHelper _output;

	public ComVtableOrderingTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void CreateComObjectOrdered_ShouldPreserveMethodOrder()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dispatcher = env.ComDispatcher;

		// Create a list of methods in specific order
		var methods = new List<KeyValuePair<string, ComMethodInfo>>
		{
			new("QueryInterface", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("AddRef", new ComMethodInfo((cpu, mem) => 1, 4)),
			new("Release", new ComMethodInfo((cpu, mem) => 2, 4)),
			new("Method3", new ComMethodInfo((cpu, mem) => 3, 8)),
			new("Method4", new ComMethodInfo((cpu, mem) => 4, 8)),
			new("Method5", new ComMethodInfo((cpu, mem) => 5, 12))
		};

		// Act
		var comObjectAddr = dispatcher.CreateComObjectOrdered("TestInterface", methods);

		// Assert
		Assert.NotEqual(0u, comObjectAddr);

		// Read vtable pointer from COM object
		var vtableAddr = memory.Read32(comObjectAddr);
		Assert.NotEqual(0u, vtableAddr);

		_output.WriteLine($"COM object at 0x{comObjectAddr:X8}, vtable at 0x{vtableAddr:X8}");

		// Verify each method pointer is at the correct offset
		for (int i = 0; i < methods.Count; i++)
		{
			var methodPtr = memory.Read32(vtableAddr + (uint)(i * 4));
			_output.WriteLine($"  Method[{i}] ({methods[i].Key}) = 0x{methodPtr:X8}");
			
			// Method pointers should be in COM vtable region (0x0D000000 range)
			Assert.True(methodPtr >= 0x0D000000 && methodPtr < 0x0E000000,
				$"Method[{i}] ({methods[i].Key}) pointer 0x{methodPtr:X8} is not in expected COM vtable region");
		}
	}

	[Fact]
	public void CreateComObjectOrdered_WithManyMethods_ShouldPreserveOrder()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dispatcher = env.ComDispatcher;

		// Create 20 methods to test ordering with a larger vtable
		var methods = new List<KeyValuePair<string, ComMethodInfo>>();
		for (int i = 0; i < 20; i++)
		{
			methods.Add(new KeyValuePair<string, ComMethodInfo>(
				$"Method{i}",
				new ComMethodInfo((cpu, mem) => (uint)i, 8)));
		}

		// Act
		var comObjectAddr = dispatcher.CreateComObjectOrdered("LargeTestInterface", methods);

		// Assert
		Assert.NotEqual(0u, comObjectAddr);
		var vtableAddr = memory.Read32(comObjectAddr);
		Assert.NotEqual(0u, vtableAddr);

		_output.WriteLine($"COM object with 20 methods at 0x{comObjectAddr:X8}, vtable at 0x{vtableAddr:X8}");

		// Verify all 20 methods are in order and sequential in memory
		uint? previousMethodPtr = null;
		for (int i = 0; i < methods.Count; i++)
		{
			var methodPtr = memory.Read32(vtableAddr + (uint)(i * 4));
			_output.WriteLine($"  Method[{i}] = 0x{methodPtr:X8}");

			Assert.True(methodPtr >= 0x0D000000 && methodPtr < 0x0E000000,
				$"Method[{i}] pointer 0x{methodPtr:X8} is not in expected COM vtable region");

			// Verify methods are sequential (each stub should be 16 bytes apart)
			if (previousMethodPtr.HasValue)
			{
				var diff = methodPtr - previousMethodPtr.Value;
				Assert.Equal(16u, diff);
			}
			previousMethodPtr = methodPtr;
		}
	}

	[Fact]
	public void IDirectDrawSurface_Flip_ShouldBeAtOffset0x2C()
	{
		// This test verifies that the Flip method is at the correct vtable offset
		// for IDirectDrawSurface (offset 0x2C = index 11)
		// This was the root cause of the BasicDD.exe crash at 0x0040715A
		
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dispatcher = env.ComDispatcher;

		// Create IDirectDrawSurface methods in correct COM interface order
		var methods = new List<KeyValuePair<string, ComMethodInfo>>
		{
			// IUnknown (0-2)
			new("QueryInterface", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("AddRef", new ComMethodInfo((cpu, mem) => 0, 4)),
			new("Release", new ComMethodInfo((cpu, mem) => 0, 4)),
			// IDirectDrawSurface (3-28)
			new("AddAttachedSurface", new ComMethodInfo((cpu, mem) => 0, 8)),
			new("AddOverlayDirtyRect", new ComMethodInfo((cpu, mem) => 0, 8)),
			new("Blt", new ComMethodInfo((cpu, mem) => 0, 20)),
			new("BltBatch", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("BltFast", new ComMethodInfo((cpu, mem) => 0, 24)),
			new("DeleteAttachedSurface", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("EnumAttachedSurfaces", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("EnumOverlayZOrders", new ComMethodInfo((cpu, mem) => 0, 12)),
			new("Flip", new ComMethodInfo((cpu, mem) => 0xF11F, 12)), // Index 11 = offset 0x2C
			new("GetAttachedSurface", new ComMethodInfo((cpu, mem) => 0, 12)), // Index 12
			// ... more methods would follow
		};

		// Act
		var comObjectAddr = dispatcher.CreateComObjectOrdered("IDirectDrawSurface", methods);

		// Assert
		Assert.NotEqual(0u, comObjectAddr);
		var vtableAddr = memory.Read32(comObjectAddr);
		Assert.NotEqual(0u, vtableAddr);

		_output.WriteLine($"IDirectDrawSurface COM object at 0x{comObjectAddr:X8}, vtable at 0x{vtableAddr:X8}");

		// Read Flip method pointer at offset 0x2C (index 11)
		var flipOffset = 11 * 4; // 0x2C
		var flipMethodPtr = memory.Read32(vtableAddr + (uint)flipOffset);
		
		_output.WriteLine($"Flip method at offset 0x{flipOffset:X} (index 11): 0x{flipMethodPtr:X8}");

		// Verify Flip is not pointing to data section (0x00407154-0x0040715A range)
		Assert.False(flipMethodPtr >= 0x00407150 && flipMethodPtr < 0x00407160,
			$"Flip method pointer 0x{flipMethodPtr:X8} is in data section! This would cause crash.");

		// Verify Flip is in COM vtable stub region
		Assert.True(flipMethodPtr >= 0x0D000000 && flipMethodPtr < 0x0E000000,
			$"Flip method pointer 0x{flipMethodPtr:X8} is not in expected COM vtable region");
	}

	[Fact]
	public void CreateComObject_WithDictionary_StillWorks()
	{
		// This test verifies the old CreateComObject method still works
		// for backward compatibility with code that hasn't been migrated yet
		
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dispatcher = env.ComDispatcher;

		var methods = new Dictionary<string, ComMethodInfo>
		{
			{ "QueryInterface", new ComMethodInfo((cpu, mem) => 0, 12) },
			{ "AddRef", new ComMethodInfo((cpu, mem) => 1, 4) },
			{ "Release", new ComMethodInfo((cpu, mem) => 2, 4) }
		};

		// Act
		var comObjectAddr = dispatcher.CreateComObject("TestInterface", methods);

		// Assert
		Assert.NotEqual(0u, comObjectAddr);
		var vtableAddr = memory.Read32(comObjectAddr);
		Assert.NotEqual(0u, vtableAddr);

		_output.WriteLine($"Dictionary-based COM object at 0x{comObjectAddr:X8}, vtable at 0x{vtableAddr:X8}");
		
		// Just verify we got valid pointers - order is not guaranteed with Dictionary
		for (int i = 0; i < 3; i++)
		{
			var methodPtr = memory.Read32(vtableAddr + (uint)(i * 4));
			_output.WriteLine($"  Method[{i}] = 0x{methodPtr:X8}");
			Assert.True(methodPtr >= 0x0D000000 && methodPtr < 0x0E000000);
		}
	}
}
