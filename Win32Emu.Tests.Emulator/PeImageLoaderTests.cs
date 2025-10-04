using Win32Emu.Loader;

namespace Win32Emu.Tests.Emulator
{
	/// <summary>
	/// Tests for PE image loader validation
	/// </summary>
	public class PeImageLoaderTests
	{
		[Fact]
		public void IsPE32_WithNonExistentFile_ReturnsFalse()
		{
			// Arrange
			var nonExistentPath = "/tmp/nonexistent.exe";

			// Act
			var result = PeImageLoader.IsPE32(nonExistentPath);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsPE32_WithTextFile_ReturnsFalse()
		{
			// Arrange - Create a temporary text file
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile, "This is not a PE file");

				// Act
				var result = PeImageLoader.IsPE32(tempFile);

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
		public void IsPE32_WithInvalidPEHeader_ReturnsFalse()
		{
			// Arrange - Create a file with an invalid PE header
			var tempFile = Path.GetTempFileName();
			try
			{
				// Write "MZ" header but invalid PE data
				var invalidPeData = new byte[1024];
				invalidPeData[0] = 0x4D; // 'M'
				invalidPeData[1] = 0x5A; // 'Z'
				// Fill rest with zeros (invalid PE structure)
				File.WriteAllBytes(tempFile, invalidPeData);

				// Act
				var result = PeImageLoader.IsPE32(tempFile);

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
	}
}
