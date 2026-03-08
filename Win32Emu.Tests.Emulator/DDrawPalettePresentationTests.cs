using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class DDrawPalettePresentationTests
{
	private const uint FakeReturnAddress = 0x12345678;
	private const byte OpaqueAlpha = 0xFF;

	[Fact]
	public void SurfaceSetPalette_ShouldRefreshPrimarySurfacePresentation()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000000;
		const uint surfaceHandle = 0x71000000;
		const uint paletteHandle = 0x72000000;
		const uint paletteComAddress = 0x00500000;

		AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		AddPalette(ddrawModule, paletteHandle, paletteComAddress, CreatePaletteEntries(0x000000FFu));
		var surface = AddPrimarySurface(ddrawModule, surfaceHandle, ddrawHandle, paletteHandle: 0);

		SetupStackArgs(cpu, memory, surfaceHandle, paletteComAddress);
		var method = GetPrivateMethod("Surface_SetPalette");
		var result = (uint)method.Invoke(ddrawModule, [cpu, memory, surfaceHandle])!;

		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, result);
		Assert.Equal(paletteHandle, (uint)GetPropertyValue(surface, "PaletteHandle")!);
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.Equal(255, backend.LastFrameData![0]);
		Assert.Equal(0, backend.LastFrameData[1]);
		Assert.Equal(0, backend.LastFrameData[2]);
		Assert.Equal(255, backend.LastFrameData[3]);
	}

	[Fact]
	public void PaletteSetEntries_ShouldRefreshPrimarySurfacePresentation()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000001;
		const uint surfaceHandle = 0x71000001;
		const uint paletteHandle = 0x72000001;

		AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		AddPalette(ddrawModule, paletteHandle, 0x00500004, CreatePaletteEntries(0));
		AddPrimarySurface(ddrawModule, surfaceHandle, ddrawHandle, paletteHandle);

		const uint paletteEntriesAddress = 0x00600000;
		memory.Write32(paletteEntriesAddress, 0x000000FFu);
		SetupStackArgs(cpu, memory, 0x00500004, 0, 1, 1, paletteEntriesAddress);
		var method = GetPrivateMethod("Palette_SetEntries");
		var result = (uint)method.Invoke(ddrawModule, [cpu, memory, paletteHandle])!;

		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, result);
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.Equal(255, backend.LastFrameData![0]);
		Assert.Equal(0, backend.LastFrameData[1]);
		Assert.Equal(0, backend.LastFrameData[2]);
		Assert.Equal(255, backend.LastFrameData[3]);
	}

	private static uint[] CreatePaletteEntries(uint entryColor)
	{
		var entries = new uint[256];
		entries[1] = entryColor;
		return entries;
	}

	private static void SetupStackArgs(JitCpu cpu, VirtualMemory memory, params uint[] args)
	{
		var esp = cpu.GetRegister("ESP");
		for (var i = args.Length - 1; i >= 0; i--)
		{
			esp -= 4;
			memory.Write32(esp, args[i]);
		}

		esp -= 4;
		memory.Write32(esp, FakeReturnAddress);
		cpu.SetRegister("ESP", esp);
	}

	private static object AddDirectDrawObject(DDrawModule ddrawModule, uint handle, TestRenderingBackend backend)
	{
		var ddrawObject = CreateNestedInstance("DirectDrawObject");
		SetPropertyValue(ddrawObject, "Handle", handle);
		SetPropertyValue(ddrawObject, "BitsPerPixel", 8);
		SetPropertyValue(ddrawObject, "RenderingBackend", backend);
		GetDictionaryField(ddrawModule, "_ddrawObjects")[handle] = ddrawObject;
		return ddrawObject;
	}

	private static object AddPrimarySurface(DDrawModule ddrawModule, uint handle, uint ddrawHandle, uint paletteHandle)
	{
		var surface = CreateNestedInstance("DirectDrawSurface");
		SetPropertyValue(surface, "Handle", handle);
		SetPropertyValue(surface, "Width", 1);
		SetPropertyValue(surface, "Height", 1);
		SetPropertyValue(surface, "Pitch", 1);
		SetPropertyValue(surface, "Bits", new byte[] { 1 });
		SetPropertyValue(surface, "IsPrimary", true);
		SetPropertyValue(surface, "DirectDrawHandle", ddrawHandle);
		SetPropertyValue(surface, "PaletteHandle", paletteHandle);
		GetDictionaryField(ddrawModule, "_surfaces")[handle] = surface;
		return surface;
	}

	private static void AddPalette(DDrawModule ddrawModule, uint handle, uint comObjectAddress, uint[] entries)
	{
		var palette = CreateNestedInstance("DirectDrawPalette");
		SetPropertyValue(palette, "Handle", handle);
		SetPropertyValue(palette, "ComObjectAddress", comObjectAddress);
		SetPropertyValue(palette, "Entries", entries);
		GetDictionaryField(ddrawModule, "_palettes")[handle] = palette;
	}

	private static IDictionary GetDictionaryField(DDrawModule ddrawModule, string fieldName)
	{
		var field = typeof(DDrawModule).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);
		return (IDictionary)field!.GetValue(ddrawModule)!;
	}

	private static MethodInfo GetPrivateMethod(string methodName)
	{
		var method = typeof(DDrawModule).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(method);
		return method!;
	}

	private static object CreateNestedInstance(string typeName)
	{
		var nestedType = typeof(DDrawModule).GetNestedType(typeName, BindingFlags.NonPublic);
		Assert.NotNull(nestedType);
		return Activator.CreateInstance(nestedType!, nonPublic: true)!;
	}

	private static void SetPropertyValue(object instance, string propertyName, object value)
	{
		var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
		Assert.NotNull(property);
		property!.SetValue(instance, value);
	}

	private static object? GetPropertyValue(object instance, string propertyName)
	{
		var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
		Assert.NotNull(property);
		return property!.GetValue(instance);
	}

	private sealed class TestRenderingBackend : IRenderingBackend
	{
		public event EventHandler<UIEventArgs>? UIEvent;

		public bool IsInitialized => true;
		public int Width => 1;
		public int Height => 1;
		public int UpdateCallCount { get; private set; }
		public byte[]? LastFrameData { get; private set; }

		public Task<bool> InitializeAsync(int width, int height, string title = "Win32Emu Display") => Task.FromResult(true);

		public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
		{
			var rgba = new byte[width * height * 4];
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var index = indexedData[y * pitch + x];
					var color = palette[index];
					var dstOffset = (y * width + x) * 4;
					rgba[dstOffset] = (byte)(color & 0xFF);
					rgba[dstOffset + 1] = (byte)((color >> 8) & 0xFF);
					rgba[dstOffset + 2] = (byte)((color >> 16) & 0xFF);
					rgba[dstOffset + 3] = OpaqueAlpha;
				}
			}

			return rgba;
		}

		public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch) => throw new NotSupportedException();
		public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch) => throw new NotSupportedException();

		public bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default)
		{
			UpdateCallCount++;
			LastFrameData = new byte[data.Length];
			Array.Copy(data, LastFrameData, data.Length);
			return true;
		}

		public void Clear(byte r, byte g, byte b, byte a = 255) { }
		public void ProcessEvents() { }
		public void BeginFrame() { }
		public void EndFrame() { }
		public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices) { }
		public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format) { }
		public void BindTexture(uint textureId) { }
		public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull) { }
		public void DeleteTexture(uint textureId) { }
		public void Dispose() { }
	}
}
