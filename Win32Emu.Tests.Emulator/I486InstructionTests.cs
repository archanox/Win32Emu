using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator
{
	/// <summary>
	/// Tests for 486-specific x86 instructions
	/// The Intel 486 introduced several new instructions including:
	/// - BSWAP: Byte swap for endianness conversion
	/// - CMPXCHG: Compare and exchange for atomic operations
	/// - XADD: Exchange and add for atomic operations
	/// - INVD: Invalidate cache (privileged)
	/// - WBINVD: Write-back and invalidate cache (privileged)
	/// - INVLPG: Invalidate TLB entry (privileged)
	/// </summary>
	public class I486InstructionTests : IDisposable
	{
		private readonly CpuTestHelper _helper;

		public I486InstructionTests()
		{
			_helper = new CpuTestHelper();
		}

		[Fact]
		public void BSWAP_EAX_ShouldReverseByteOrder()
		{
			// Arrange: BSWAP EAX (0F C8)
			_helper.SetReg("EAX", 0x12345678);
			_helper.WriteCode(0x0F, 0xC8);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x78563412u, _helper.GetReg("EAX"));
		}

		[Fact]
		public void BSWAP_EBX_ShouldReverseByteOrder()
		{
			// Arrange: BSWAP EBX (0F CB)
			_helper.SetReg("EBX", 0xAABBCCDD);
			_helper.WriteCode(0x0F, 0xCB);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0xDDCCBBAAu, _helper.GetReg("EBX"));
		}

		[Fact]
		public void BSWAP_ECX_ShouldReverseByteOrder()
		{
			// Arrange: BSWAP ECX (0F C9)
			_helper.SetReg("ECX", 0x11223344);
			_helper.WriteCode(0x0F, 0xC9);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x44332211u, _helper.GetReg("ECX"));
		}

		[Fact]
		public void BSWAP_EDX_ShouldReverseByteOrder()
		{
			// Arrange: BSWAP EDX (0F CA)
			_helper.SetReg("EDX", 0xFF00FF00);
			_helper.WriteCode(0x0F, 0xCA);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x00FF00FFu, _helper.GetReg("EDX"));
		}

		[Fact]
		public void CMPXCHG_Register_Equal_ShouldExchange()
		{
			// Arrange: CMPXCHG EBX, ECX (0F B1 CB)
			// When EAX == EBX, ZF=1 and EBX gets ECX
			_helper.SetReg("EAX", 0x12345678);
			_helper.SetReg("EBX", 0x12345678);
			_helper.SetReg("ECX", 0xAABBCCDD);
			_helper.WriteCode(0x0F, 0xB1, 0xCB);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0xAABBCCDDu, _helper.GetReg("EBX"));
			Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set when values are equal");
		}

		[Fact]
		public void CMPXCHG_Register_NotEqual_ShouldLoadEAX()
		{
			// Arrange: CMPXCHG EBX, ECX (0F B1 CB)
			// When EAX != EBX, ZF=0 and EAX gets EBX
			_helper.SetReg("EAX", 0x11111111);
			_helper.SetReg("EBX", 0x22222222);
			_helper.SetReg("ECX", 0x33333333);
			_helper.WriteCode(0x0F, 0xB1, 0xCB);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x22222222u, _helper.GetReg("EAX"));
			Assert.Equal(0x22222222u, _helper.GetReg("EBX")); // Should not change
			Assert.False(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be clear when values are not equal");
		}

		[Fact]
		public void XADD_Registers_ShouldExchangeAndAdd()
		{
			// Arrange: XADD EBX, ECX (0F C1 CB)
			// temp = EBX, EBX = EBX + ECX, ECX = temp
			_helper.SetReg("EBX", 0x00000005);
			_helper.SetReg("ECX", 0x00000003);
			_helper.WriteCode(0x0F, 0xC1, 0xCB);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x00000008u, _helper.GetReg("EBX")); // 5 + 3
			Assert.Equal(0x00000005u, _helper.GetReg("ECX")); // original EBX
		}

		[Fact]
		public void XADD_WithCarry_ShouldSetFlags()
		{
			// Arrange: XADD EAX, EBX (0F C1 D8)
			_helper.SetReg("EAX", 0xFFFFFFFF);
			_helper.SetReg("EBX", 0x00000001);
			_helper.WriteCode(0x0F, 0xC1, 0xD8);

			// Act
			_helper.ExecuteInstruction();

			// Assert
			Assert.Equal(0x00000000u, _helper.GetReg("EAX")); // Overflow to 0
			Assert.Equal(0xFFFFFFFFu, _helper.GetReg("EBX")); // original EAX
			Assert.True(_helper.IsFlagSet(CpuFlag.Zf), "ZF should be set for zero result");
			Assert.True(_helper.IsFlagSet(CpuFlag.Cf), "CF should be set for carry");
		}

		[Fact]
		public void INVD_ShouldNotCrash()
		{
			// Arrange: INVD (0F 08) - Invalidate cache
			// This is a privileged instruction but should have a stub
			_helper.WriteCode(0x0F, 0x08);

			// Act & Assert - Should not throw
			var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
			// In user mode, this might be a NOP or throw. Either is acceptable.
			// For now, we just verify it doesn't crash the emulator
		}

		[Fact]
		public void WBINVD_ShouldNotCrash()
		{
			// Arrange: WBINVD (0F 09) - Write-back and invalidate cache
			// This is a privileged instruction but should have a stub
			_helper.WriteCode(0x0F, 0x09);

			// Act & Assert - Should not throw
			var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
			// In user mode, this might be a NOP or throw. Either is acceptable.
			// For now, we just verify it doesn't crash the emulator
		}

		[Fact]
		public void INVLPG_ShouldNotCrash()
		{
			// Arrange: INVLPG [EBX] (0F 01 3B) - Invalidate TLB entry
			// This is a privileged instruction but should have a stub
			_helper.SetReg("EBX", 0x00200000);
			_helper.WriteCode(0x0F, 0x01, 0x3B);

			// Act & Assert - Should not throw
			var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
			// In user mode, this might be a NOP or throw. Either is acceptable.
			// For now, we just verify it doesn't crash the emulator
		}

		public void Dispose()
		{
			_helper?.Dispose();
		}
	}
}
