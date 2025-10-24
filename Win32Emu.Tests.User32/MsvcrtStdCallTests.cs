using Xunit;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32
{
    public class MsvcrtStdCallTests
    {
        [Theory]
        [InlineData("MSVCRT.DLL", "malloc", 4)]
        [InlineData("MSVCRT.DLL", "calloc", 8)]
        [InlineData("MSVCRT.DLL", "free", 4)]
        [InlineData("MSVCRT.DLL", "memcpy", 12)]
        [InlineData("MSVCRT.DLL", "__set_app_type", 4)]
        [InlineData("MSVCRT.DLL", "__p__fmode", 0)]
        [InlineData("MSVCRT.DLL", "__p__commode", 0)]
        [InlineData("MSVCRT.DLL", "__p___initenv", 0)]
        [InlineData("MSVCRT.DLL", "_initterm", 8)]
        [InlineData("MSVCRT.DLL", "atexit", 4)]
        public void Msvcrt_ShouldHaveCorrectArgBytes(string dll, string export, int expectedBytes)
        {
            // Act
            var argBytes = StdCallMeta.GetArgBytes(dll, export);

            // Assert
            Assert.Equal(expectedBytes, argBytes);
        }

        [Theory]
        [InlineData("KERNEL32.DLL", "GetCommandLineW", 0)]
        [InlineData("KERNEL32.DLL", "SetConsoleOutputCP", 4)]
        [InlineData("KERNEL32.DLL", "CreateFileW", 28)]
        public void Kernel32NewFunctions_ShouldHaveCorrectArgBytes(string dll, string export, int expectedBytes)
        {
            // Act
            var argBytes = StdCallMeta.GetArgBytes(dll, export);

            // Assert
            Assert.Equal(expectedBytes, argBytes);
        }
    }
}
