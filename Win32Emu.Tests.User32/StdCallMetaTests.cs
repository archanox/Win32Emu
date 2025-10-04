using Xunit;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32
{
    public class StdCallMetaTests
    {
        [Theory]
        [InlineData("USER32.DLL", "RegisterClassA", 4)]  // 1 param * 4 bytes
        [InlineData("USER32.DLL", "CreateWindowExA", 48)] // 12 params * 4 bytes
        [InlineData("USER32.DLL", "ShowWindow", 8)]      // 2 params * 4 bytes
        [InlineData("USER32.DLL", "GetMessageA", 16)]    // 4 params * 4 bytes
        [InlineData("USER32.DLL", "TranslateMessage", 4)] // 1 param * 4 bytes
        [InlineData("USER32.DLL", "DispatchMessageA", 4)] // 1 param * 4 bytes
        public void User32_ShouldHaveCorrectArgBytes(string dll, string export, int expectedBytes)
        {
            // Act
            var argBytes = StdCallMeta.GetArgBytes(dll, export);

            // Assert
            Assert.Equal(expectedBytes, argBytes);
        }
    }
}
