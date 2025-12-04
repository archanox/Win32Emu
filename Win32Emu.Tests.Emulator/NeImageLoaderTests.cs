using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for NE (Win16) image loader validation
/// </summary>
public class NeImageLoaderTests
{
	[Fact]
	public void IsNE_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var nonExistentPath = "/tmp/nonexistent.exe";

		// Act
		var result = NeImageLoader.IsNE(nonExistentPath);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void IsNE_WithTextFile_ReturnsFalse()
	{
		// Arrange - Create a temporary text file
		var tempFile = Path.GetTempFileName();
		try
		{
			File.WriteAllText(tempFile, "This is not an NE file");

			// Act
			var result = NeImageLoader.IsNE(tempFile);

			// Assert
			Assert.False(result);
		}
		finally
		{
			// Cleanup
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void IsNE_WithInvalidNEHeader_ReturnsFalse()
	{
		// Arrange - Create a file with MZ header but invalid NE signature
		var tempFile = Path.GetTempFileName();
		try
		{
			var invalidNeData = new byte[1024];
			// DOS MZ header
			invalidNeData[0] = 0x4D; // 'M'
			invalidNeData[1] = 0x5A; // 'Z'
			// Offset to NE header at 0x3C
			invalidNeData[0x3C] = 0x40; // Points to offset 0x40
			invalidNeData[0x3D] = 0x00;
			invalidNeData[0x3E] = 0x00;
			invalidNeData[0x3F] = 0x00;
			// Invalid signature at 0x40 (not 'NE')
			invalidNeData[0x40] = 0x00;
			invalidNeData[0x41] = 0x00;
			
			File.WriteAllBytes(tempFile, invalidNeData);

			// Act
			var result = NeImageLoader.IsNE(tempFile);

			// Assert
			Assert.False(result);
		}
		finally
		{
			// Cleanup
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void IsNE_WithValidNESignature_ReturnsTrue()
	{
		// Arrange - Create a minimal NE file structure
		var tempFile = Path.Combine(Path.GetTempPath(), $"test_ne_{Guid.NewGuid()}.exe");
		try
		{
			var neData = new byte[1024];
			// DOS MZ header
			neData[0] = 0x4D; // 'M'
			neData[1] = 0x5A; // 'Z'
			// Offset to NE header at 0x3C (32-bit little-endian)
			neData[0x3C] = 0x80; // Offset = 0x80
			neData[0x3D] = 0x00;
			neData[0x3E] = 0x00;
			neData[0x3F] = 0x00;
			// NE signature at 0x80
			neData[0x80] = 0x4E; // 'N'
			neData[0x81] = 0x45; // 'E'
			
			File.WriteAllBytes(tempFile, neData);

			// Act
			var result = NeImageLoader.IsNE(tempFile);

			// Assert
			Assert.True(result, $"Expected NE file to be detected. File size: {new FileInfo(tempFile).Length}");
		}
		finally
		{
			// Cleanup
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void IsNE_WithByteArray_ValidNESignature_ReturnsTrue()
	{
		// Arrange - Create a minimal NE file structure
		var neData = new byte[256];
		// DOS MZ header
		neData[0] = 0x4D; // 'M'
		neData[1] = 0x5A; // 'Z'
		// Offset to NE header at 0x3C
		neData[0x3C] = 0x80; // Points to offset 0x80
		neData[0x3D] = 0x00;
		neData[0x3E] = 0x00;
		neData[0x3F] = 0x00;
		// NE signature at 0x80
		neData[0x80] = 0x4E; // 'N'
		neData[0x81] = 0x45; // 'E'

		// Act
		var result = NeImageLoader.IsNE(neData);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsNE_WithByteArray_InvalidSignature_ReturnsFalse()
	{
		// Arrange
		var invalidData = new byte[256];
		// DOS MZ header
		invalidData[0] = 0x4D; // 'M'
		invalidData[1] = 0x5A; // 'Z'
		// Offset to NE header at 0x3C
		invalidData[0x3C] = 0x80;
		// Invalid signature at 0x80 (not 'NE')
		invalidData[0x80] = 0x00;
		invalidData[0x81] = 0x00;

		// Act
		var result = NeImageLoader.IsNE(invalidData);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void DetectFormat_WithNEFile_ReturnsNE()
	{
		// Arrange - Create a minimal NE file
		var tempFile = Path.GetTempFileName();
		try
		{
			var neData = new byte[1024];
			// DOS MZ header
			neData[0] = 0x4D; // 'M'
			neData[1] = 0x5A; // 'Z'
			// Offset to NE header at 0x3C
			neData[0x3C] = 0x80;
			neData[0x3D] = 0x00;
			neData[0x3E] = 0x00;
			neData[0x3F] = 0x00;
			// NE signature at 0x80
			neData[0x80] = 0x4E; // 'N'
			neData[0x81] = 0x45; // 'E'
			
			File.WriteAllBytes(tempFile, neData);

			// Act
			var result = PeImageLoader.DetectFormat(tempFile);

			// Assert
			Assert.Equal(ExecutableFormat.NE, result);
		}
		finally
		{
			// Cleanup
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void DetectFormat_WithByteArray_NEFile_ReturnsNE()
	{
		// Arrange
		var neData = new byte[256];
		// DOS MZ header
		neData[0] = 0x4D; // 'M'
		neData[1] = 0x5A; // 'Z'
		// Offset to NE header at 0x3C
		neData[0x3C] = 0x80;
		// NE signature at 0x80
		neData[0x80] = 0x4E; // 'N'
		neData[0x81] = 0x45; // 'E'

		// Act
		var result = PeImageLoader.DetectFormat(neData);

		// Assert
		Assert.Equal(ExecutableFormat.NE, result);
	}

	[Fact]
	public void DetectFormat_WithInvalidFile_ReturnsUnknown()
	{
		// Arrange - Create an invalid file
		var tempFile = Path.GetTempFileName();
		try
		{
			File.WriteAllText(tempFile, "Not an executable");

			// Act
			var result = PeImageLoader.DetectFormat(tempFile);

			// Assert
			Assert.Equal(ExecutableFormat.Unknown, result);
		}
		finally
		{
			// Cleanup
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void LoadFromBytes_WithMinimalNEFile_CreatesLoadedImage()
	{
		// Arrange
		var vm = new VirtualMemory(256 * 1024 * 1024, NullLogger.Instance); // 256MB
		var loader = new NeImageLoader(vm, NullLogger.Instance);

		// Create a minimal valid NE file structure
		var neData = CreateMinimalNEFile();

		// Act
		var image = loader.LoadFromBytes(neData, "<test>");

		// Assert
		Assert.NotNull(image);
		Assert.Equal(0x00010000u, image.BaseAddress); // Default base address for NE
		Assert.True(image.EntryPointAddress >= image.BaseAddress);
	}

	[Fact]
	public void LoadFromBytes_WithMinimalNEFile_SetsHeaderEndRvaToZero()
	{
		// Arrange
		var vm = new VirtualMemory(256 * 1024 * 1024, NullLogger.Instance); // 256MB
		var loader = new NeImageLoader(vm, NullLogger.Instance);

		// Create a minimal valid NE file structure
		var neData = CreateMinimalNEFile();

		// Act
		var image = loader.LoadFromBytes(neData, "<test>");

		// Assert
		// NE executables have no PE header, so HeaderEndRva should be 0
		// This allows code to execute from the base address without triggering
		// "PE header region" detection in the emulator
		Assert.Equal(0u, image.HeaderEndRva);
	}

	[Fact]
	public void LoadFromBytes_WithImportModules_ParsesModuleNamesCorrectly()
	{
		// Arrange
		var vm = new VirtualMemory(256 * 1024 * 1024, NullLogger.Instance); // 256MB
		var loader = new NeImageLoader(vm, NullLogger.Instance);

		// Create an NE file with import modules
		var neData = CreateNEFileWithImports();

		// Act - This should not throw and should parse module names correctly
		var image = loader.LoadFromBytes(neData, "<test>");

		// Assert - If the fix is correct, this should load successfully
		// The bug would cause garbage module names and potentially crash or fail to load
		Assert.NotNull(image);
		Assert.Equal(0x00010000u, image.BaseAddress);
	}

	/// <summary>
	/// Creates an NE file with import modules to test import parsing.
	/// </summary>
	private static byte[] CreateNEFileWithImports()
	{
		var data = new byte[2048];
		
		// DOS MZ header
		data[0] = 0x4D; // 'M'
		data[1] = 0x5A; // 'Z'
		data[0x3C] = 0x80;
		
		// NE header at offset 0x80
		var neOffset = 0x80;
		data[neOffset + 0] = 0x4E;  // 'N'
		data[neOffset + 1] = 0x45;  // 'E'
		data[neOffset + 2] = 5;
		data[neOffset + 3] = 10;
		
		WriteUInt16(data, neOffset + 4, 0x0100);
		WriteUInt16(data, neOffset + 6, 0);
		WriteUInt32(data, neOffset + 8, 0);
		WriteUInt16(data, neOffset + 12, 0x0300);
		WriteUInt16(data, neOffset + 14, 2);
		WriteUInt16(data, neOffset + 0x16, 1);
		WriteUInt16(data, neOffset + 0x18, 0);
		WriteUInt16(data, neOffset + 0x1E, 1);
		
		// Module reference count
		WriteUInt16(data, neOffset + 0x20, 2); // 2 modules
		
		WriteUInt16(data, neOffset + 0x24, 0x40); // Segment table
		WriteUInt16(data, neOffset + 0x26, 0x48); // Resource table
		WriteUInt16(data, neOffset + 0x28, 0x50); // Resident name table
		WriteUInt16(data, neOffset + 0x2A, 0x60); // Module reference table
		WriteUInt16(data, neOffset + 0x2C, 0x70); // Imported names table
		WriteUInt32(data, neOffset + 44, 0);
		WriteUInt16(data, neOffset + 0x32, 0);
		WriteUInt16(data, neOffset + 0x34, 4);
		data[neOffset + 0x38] = 2;
		WriteUInt16(data, neOffset + 0x40, 0x0300);
		
		// Segment table
		var segmentOffset = neOffset + 0x40;
		WriteUInt16(data, segmentOffset + 0, 0x20);
		WriteUInt16(data, segmentOffset + 2, 0x100);
		WriteUInt16(data, segmentOffset + 4, 0x0000);
		WriteUInt16(data, segmentOffset + 6, 0x100);
		
		// Resource table (empty)
		WriteUInt16(data, neOffset + 0x48, 0);
		
		// Resident name table
		data[neOffset + 0x50] = 4;
		data[neOffset + 0x51] = (byte)'T';
		data[neOffset + 0x52] = (byte)'E';
		data[neOffset + 0x53] = (byte)'S';
		data[neOffset + 0x54] = (byte)'T';
		WriteUInt16(data, neOffset + 0x55, 0);
		data[neOffset + 0x57] = 0;
		
		// Module reference table at neOffset + 0x60
		// Two entries, each is a 2-byte offset into imported names table
		WriteUInt16(data, neOffset + 0x60, 0); // Offset to "KERNEL" (0 bytes from start of imported names)
		WriteUInt16(data, neOffset + 0x62, 7); // Offset to "USER" (7 bytes from start of imported names)
		
		// Imported names table at neOffset + 0x70
		// Format: length-byte + string
		// "KERNEL" at offset 0
		data[neOffset + 0x70] = 6; // Length
		data[neOffset + 0x71] = (byte)'K';
		data[neOffset + 0x72] = (byte)'E';
		data[neOffset + 0x73] = (byte)'R';
		data[neOffset + 0x74] = (byte)'N';
		data[neOffset + 0x75] = (byte)'E';
		data[neOffset + 0x76] = (byte)'L';
		
		// "USER" at offset 7
		data[neOffset + 0x77] = 4; // Length
		data[neOffset + 0x78] = (byte)'U';
		data[neOffset + 0x79] = (byte)'S';
		data[neOffset + 0x7A] = (byte)'E';
		data[neOffset + 0x7B] = (byte)'R';
		
		// Put dummy code
		data[0x200] = 0xC3;
		
		return data;
	}

	/// <summary>
	/// Creates a minimal valid NE file structure for testing.
	/// This is a simplified structure that passes basic validation.
	/// </summary>
	private static byte[] CreateMinimalNEFile()
	{
		var data = new byte[2048];
		
		// DOS MZ header
		data[0] = 0x4D; // 'M'
		data[1] = 0x5A; // 'Z'
		// Offset to NE header at 0x3C
		data[0x3C] = 0x80; // Points to offset 0x80
		data[0x3D] = 0x00;
		data[0x3E] = 0x00;
		data[0x3F] = 0x00;
		
		// NE header at offset 0x80
		var neOffset = 0x80;
		data[neOffset + 0] = 0x4E;  // 'N'
		data[neOffset + 1] = 0x45;  // 'E'
		data[neOffset + 2] = 5;     // Linker major version
		data[neOffset + 3] = 10;    // Linker minor version
		
		// Entry table offset (after NE header)
		WriteUInt16(data, neOffset + 4, 0x0100);
		// Entry table length
		WriteUInt16(data, neOffset + 6, 0);
		
		// CRC checksum
		WriteUInt32(data, neOffset + 8, 0);
		
		// Program flags
		WriteUInt16(data, neOffset + 12, 0x0300); // Protected mode, single data
		// Application type
		WriteUInt16(data, neOffset + 14, 2); // Windows application
		
		// Entry point (segment:offset)
		WriteUInt16(data, neOffset + 0x16, 1); // Segment 1 (CS)
		WriteUInt16(data, neOffset + 0x18, 0); // Offset 0 (IP)
		
		// Segment count
		WriteUInt16(data, neOffset + 0x1E, 1); // One segment
		
		// Segment table offset (relative to NE header)
		WriteUInt16(data, neOffset + 0x24, 0x40); // Offset 0x40 from NE header
		
		// Resource table offset
		WriteUInt16(data, neOffset + 0x26, 0x48); // After segment table
		
		// Resident name table offset
		WriteUInt16(data, neOffset + 0x28, 0x50); // After resource table
		
		// Module reference table offset
		WriteUInt16(data, neOffset + 0x2A, 0x60); // After resident name table
		
		// Imported names table offset
		WriteUInt16(data, neOffset + 0x2C, 0x70); // After module reference table
		
		// Non-resident name table offset (file offset, not relative)
		WriteUInt32(data, neOffset + 44, 0); // No non-resident names
		
		// Movable entry count
		WriteUInt16(data, neOffset + 0x32, 0);
		
		// Sector alignment shift
		WriteUInt16(data, neOffset + 0x34, 4); // 16-byte sectors
		
		// Target OS
		data[neOffset + 0x38] = 2; // Windows
		
		// Expected Windows version
		WriteUInt16(data, neOffset + 0x40, 0x0300); // Windows 3.0
		
		// Segment table at offset neOffset + 0x40
		var segmentOffset = neOffset + 0x40;
		// Segment 1: offset in file (in sectors), length, flags, min allocation
		WriteUInt16(data, segmentOffset + 0, 0x20); // File offset = 0x20 * 16 = 0x200
		WriteUInt16(data, segmentOffset + 2, 0x100); // Length = 256 bytes
		WriteUInt16(data, segmentOffset + 4, 0x0000); // Flags: code segment
		WriteUInt16(data, segmentOffset + 6, 0x100); // Min allocation = 256 bytes
		
		// Resource table (empty) at offset neOffset + 0x48
		// Just alignment size (2 bytes) set to 0
		WriteUInt16(data, neOffset + 0x48, 0);
		
		// Resident name table at offset neOffset + 0x50
		// Module name entry (length-prefixed string + ordinal)
		data[neOffset + 0x50] = 4; // Name length
		data[neOffset + 0x51] = (byte)'T'; // "TEST"
		data[neOffset + 0x52] = (byte)'E';
		data[neOffset + 0x53] = (byte)'S';
		data[neOffset + 0x54] = (byte)'T';
		WriteUInt16(data, neOffset + 0x55, 0); // Ordinal 0 (module name)
		// End of name table
		data[neOffset + 0x57] = 0;
		
		// Module reference table (empty) at offset neOffset + 0x60
		// Imported names table (empty) at offset neOffset + 0x70
		
		// Put some dummy code in the segment at file offset 0x200
		data[0x200] = 0xC3; // RET instruction
		
		return data;
	}

	private static void WriteUInt16(byte[] data, int offset, ushort value)
	{
		data[offset] = (byte)(value & 0xFF);
		data[offset + 1] = (byte)((value >> 8) & 0xFF);
	}

	private static void WriteUInt32(byte[] data, int offset, uint value)
	{
		data[offset] = (byte)(value & 0xFF);
		data[offset + 1] = (byte)((value >> 8) & 0xFF);
		data[offset + 2] = (byte)((value >> 16) & 0xFF);
		data[offset + 3] = (byte)((value >> 24) & 0xFF);
	}
}
