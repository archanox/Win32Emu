using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;
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
	public void UpdateRenderingBackend_ShouldSkipPrimarySurfacePresentation_WhenPaletteIsMissing()
	{
		var memory = new VirtualMemory();
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000010;
		const uint surfaceHandle = 0x71000010;

		var ddrawObject = AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		AddPrimarySurface(ddrawModule, surfaceHandle, ddrawHandle, paletteHandle: 0);

		var method = GetPrivateMethod("UpdateRenderingBackend");
		method.Invoke(ddrawModule, [GetDictionaryField(ddrawModule, "_surfaces")[surfaceHandle]!, ddrawObject]);

		Assert.Equal(0, backend.UpdateCallCount);
		Assert.Null(backend.LastFrameData);
	}

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

	[Fact]
	public void SurfaceWithAttachedBackbuffers_ShouldBeRendered_WhenUnlocked()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000002;
		const uint primarySurfaceHandle = 0x71000002;
		const uint backBufferHandle = 0x71000003;
		const uint paletteHandle = 0x72000002;

		AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		AddPalette(ddrawModule, paletteHandle, 0x00500008, CreatePaletteEntries(0x0000FF00u));

		// Create a primary surface with an attached backbuffer
		_ = AddSurfaceWithBackbuffer(ddrawModule, primarySurfaceHandle, ddrawHandle, paletteHandle, backBufferHandle);
		// Create backbuffer that also has attached surfaces (simulating a flip chain)
		var backBuffer = AddBackbufferSurface(ddrawModule, backBufferHandle, ddrawHandle, paletteHandle);
		// Add the primary surface as an "attached" surface to the backbuffer to create a flip chain
		var backbufferAttachedSurfaces = (System.Collections.Generic.List<uint>)GetPropertyValue(backBuffer, "AttachedSurfaces")!;
		backbufferAttachedSurfaces.Add(primarySurfaceHandle);

		// Lock and unlock the backbuffer (which should trigger rendering because it has attached surfaces)
		SetupStackArgs(cpu, memory, backBufferHandle, 0, 0);
		var lockMethod = GetPrivateMethod("Surface_Lock");
		var lockResult = (uint)lockMethod.Invoke(ddrawModule, [cpu, memory, backBufferHandle])!;
		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, lockResult);

		// Write some test data to the locked memory pointer (simulating what a game does)
		var lockedMemoryPtr = (uint)GetPropertyValue(backBuffer, "LockedMemoryPtr")!;
		memory.Write8(lockedMemoryPtr, 1); // Index into palette

		// Unlock the backbuffer
		SetupStackArgs(cpu, memory, backBufferHandle, 0);
		var unlockMethod = GetPrivateMethod("Surface_Unlock");
		var unlockResult = (uint)unlockMethod.Invoke(ddrawModule, [cpu, memory, backBufferHandle])!;
		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, unlockResult);

		// Verify the rendering backend was updated
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.NotNull(backend.LastFrameData);
		Assert.Equal(0, backend.LastFrameData[0]); // Red channel (color is 0x0000FF00)
		Assert.Equal(255, backend.LastFrameData[1]); // Green channel
		Assert.Equal(0, backend.LastFrameData[2]); // Blue channel
		Assert.Equal(255, backend.LastFrameData[3]); // Alpha channel
	}

	[Fact]
	public void SurfaceWithAttachedBackbuffers_ShouldBeRendered_WhenBltFastCalled()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000003;
		const uint surfaceHandle = 0x71000004;
		const uint backBufferHandle = 0x71000005;
		const uint sourceSurfaceHandle = 0x71000006;
		const uint paletteHandle = 0x72000003;

		AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		AddPalette(ddrawModule, paletteHandle, 0x00500010, CreatePaletteEntries(0x00FF0000u));

		// Create a primary surface with an attached backbuffer
		AddSurfaceWithBackbuffer(ddrawModule, surfaceHandle, ddrawHandle, paletteHandle, backBufferHandle);
		var backBuffer = AddBackbufferSurface(ddrawModule, backBufferHandle, ddrawHandle, paletteHandle);
		// Add the primary surface as an "attached" surface to the backbuffer to create a flip chain
		var backbufferAttachedSurfaces = (System.Collections.Generic.List<uint>)GetPropertyValue(backBuffer, "AttachedSurfaces")!;
		backbufferAttachedSurfaces.Add(surfaceHandle);

		var sourceSurface = AddOffscreenSurface(ddrawModule, sourceSurfaceHandle, ddrawHandle, paletteHandle);

		// Lock the source surface
		SetupStackArgs(cpu, memory, sourceSurfaceHandle, 0, 0);
		var lockMethod = GetPrivateMethod("Surface_Lock");
		var lockResult = (uint)lockMethod.Invoke(ddrawModule, [cpu, memory, sourceSurfaceHandle])!;
		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, lockResult);

		// Set up source surface data
		var sourceLockedMemoryPtr = (uint)GetPropertyValue(sourceSurface, "LockedMemoryPtr")!;
		memory.Write8(sourceLockedMemoryPtr, 1); // Index into palette for blue color

		// Unlock the source surface (this copies LockedMemoryPtr back to Bits)
		SetupStackArgs(cpu, memory, sourceSurfaceHandle, 0);
		var unlockMethod = GetPrivateMethod("Surface_Unlock");
		var unlockResult = (uint)unlockMethod.Invoke(ddrawModule, [cpu, memory, sourceSurfaceHandle])!;
		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, unlockResult);

		// Call BltFast to blit to the backbuffer
		const uint backBufferComAddr = 0x00600000;
		const uint sourceSurfaceComAddr = 0x00600010;
		SetPropertyValue(backBuffer, "ComObjectAddress", backBufferComAddr);
		SetPropertyValue(sourceSurface, "ComObjectAddress", sourceSurfaceComAddr);
		const uint srcRectPtr = 0;
		SetupStackArgs(cpu, memory, backBufferComAddr, 0, 0, sourceSurfaceComAddr, srcRectPtr, 0);
		var bltFastMethod = GetPrivateMethod("Surface_BltFast");
		var result = (uint)bltFastMethod.Invoke(ddrawModule, [cpu, memory])!;

		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, result);
		// Verify the rendering backend was updated
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.NotNull(backend.LastFrameData);
		Assert.Equal(0, backend.LastFrameData[0]); // Red channel (color is 0x00FF0000)
		Assert.Equal(0, backend.LastFrameData[1]); // Green channel
		Assert.Equal(255, backend.LastFrameData[2]); // Blue channel
		Assert.Equal(255, backend.LastFrameData[3]); // Alpha channel
	}

	[Fact]
	public void UpdateRenderingBackend_ShouldUsePaletteFromAttachedSurface_WhenPrimaryHasNoPalette()
	{
		var memory = new VirtualMemory();
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000020;
		const uint primaryHandle = 0x71000020;
		const uint backBufferHandle = 0x71000021;
		const uint paletteHandle = 0x72000020;

		var ddrawObject = AddDirectDrawObject(ddrawModule, ddrawHandle, backend);

		// Palette is only on the backbuffer, not the primary surface
		AddPalette(ddrawModule, paletteHandle, 0x00500020, CreatePaletteEntries(0x000000FFu));
		AddSurfaceWithBackbuffer(ddrawModule, primaryHandle, ddrawHandle, 0, backBufferHandle);
		AddBackbufferSurface(ddrawModule, backBufferHandle, ddrawHandle, paletteHandle);

		var method = GetPrivateMethod("UpdateRenderingBackend");
		method.Invoke(ddrawModule, [GetDictionaryField(ddrawModule, "_surfaces")[primaryHandle]!, ddrawObject]);

		// Primary has no palette but backbuffer does – rendering should succeed via flip-chain lookup
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.NotNull(backend.LastFrameData);
		// Pixel at index 1 -> palette entry 0x000000FF -> R=255, G=0, B=0
		Assert.Equal(255, backend.LastFrameData![0]);
		Assert.Equal(0, backend.LastFrameData[1]);
		Assert.Equal(0, backend.LastFrameData[2]);
		Assert.Equal(255, backend.LastFrameData[3]);
	}

	[Fact]
	public void PaletteSetEntries_ShouldRefreshPrimaryViaFlipChain_WhenPaletteIsOnBackbuffer()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		var backend = new TestRenderingBackend();
		const uint ddrawHandle = 0x70000021;
		const uint primaryHandle = 0x71000022;
		const uint backBufferHandle = 0x71000023;
		const uint paletteHandle = 0x72000021;

		AddDirectDrawObject(ddrawModule, ddrawHandle, backend);
		// Palette starts with all-zero entries; only the backbuffer references it
		AddPalette(ddrawModule, paletteHandle, 0x00500024, CreatePaletteEntries(0));
		AddSurfaceWithBackbuffer(ddrawModule, primaryHandle, ddrawHandle, 0, backBufferHandle);
		AddBackbufferSurface(ddrawModule, backBufferHandle, ddrawHandle, paletteHandle);

		// Update palette entries on the backbuffer's palette; should trigger a primary surface refresh
		const uint paletteEntriesAddress = 0x00700000;
		memory.Write32(paletteEntriesAddress, 0x000000FFu);
		SetupStackArgs(cpu, memory, 0x00500024, 0, 1, 1, paletteEntriesAddress);
		var method = GetPrivateMethod("Palette_SetEntries");
		var result = (uint)method.Invoke(ddrawModule, [cpu, memory, paletteHandle])!;

		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, result);
		// The primary surface should have been refreshed even though its PaletteHandle is 0
		Assert.Equal(1, backend.UpdateCallCount);
		Assert.NotNull(backend.LastFrameData);
		Assert.Equal(255, backend.LastFrameData![0]);
		Assert.Equal(0, backend.LastFrameData[1]);
		Assert.Equal(0, backend.LastFrameData[2]);
		Assert.Equal(255, backend.LastFrameData[3]);
	}

	[Fact]
	public async Task SetDisplayMode_ShouldReinitializeBackend_WhenDimensionsChangeAfterInitialization()
	{
		var memory = new VirtualMemory();
		var cpu = new JitCpu(memory, NullLogger.Instance);
		var backendFactory = new TestBackendFactory();
		var processEnvironment = new ProcessEnvironment(memory, logger: NullLogger.Instance, backendFactory: backendFactory);
		var ddrawModule = new DDrawModule(processEnvironment, 0x00400000, null, NullLogger.Instance);
		const uint ddrawHandle = 0x70000030;
		const uint comObjectAddress = 0x00500030;

		backendFactory.NextBackend = new ResizableTestRenderingBackend(isInitialized: true, width: 640, height: 480);

		var ddrawObject = CreateNestedInstance("DirectDrawObject");
		SetPropertyValue(ddrawObject, "Handle", ddrawHandle);
		SetPropertyValue(ddrawObject, "ComObjectAddress", comObjectAddress);
		SetPropertyValue(ddrawObject, "Width", 640);
		SetPropertyValue(ddrawObject, "Height", 480);
		SetPropertyValue(ddrawObject, "BitsPerPixel", 8);
		SetPropertyValue(ddrawObject, "RenderingBackend", backendFactory.NextBackend);
		GetDictionaryField(ddrawModule, "_ddrawObjects")[ddrawHandle] = ddrawObject;
		GetDictionaryField(ddrawModule, "_comObjectToHandle")[comObjectAddress] = ddrawHandle;

		var method = GetPrivateMethod("DDraw_SetDisplayModeAsync");

		SetupStackArgs(cpu, memory, comObjectAddress, 320, 200, 8);
		var result = (uint)await (Task<uint>)method.Invoke(ddrawModule, [cpu, memory, ddrawHandle])!;

		Assert.Equal((uint)NativeTypes.DDResult.DD_OK, result);
		Assert.Equal(1, backendFactory.CreateRenderingBackendWithHostCallCount);
		Assert.Equal(1, backendFactory.InitialBackend.DisposeCallCount);
		Assert.Equal(1, backendFactory.CreatedBackend.InitializeCallCount);
		Assert.Equal(320, backendFactory.CreatedBackend.Width);
		Assert.Equal(200, backendFactory.CreatedBackend.Height);
		Assert.Same(backendFactory.CreatedBackend, GetPropertyValue(ddrawObject, "RenderingBackend"));
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

	private static object AddSurfaceWithBackbuffer(DDrawModule ddrawModule, uint handle, uint ddrawHandle, uint paletteHandle, uint backBufferHandle)
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

		// Add backbuffer to attached surfaces list
		var attachedSurfaces = (System.Collections.Generic.List<uint>)GetPropertyValue(surface, "AttachedSurfaces")!;
		attachedSurfaces.Add(backBufferHandle);

		GetDictionaryField(ddrawModule, "_surfaces")[handle] = surface;
		return surface;
	}

	private static object AddBackbufferSurface(DDrawModule ddrawModule, uint handle, uint ddrawHandle, uint paletteHandle)
	{
		var surface = CreateNestedInstance("DirectDrawSurface");
		SetPropertyValue(surface, "Handle", handle);
		SetPropertyValue(surface, "Width", 1);
		SetPropertyValue(surface, "Height", 1);
		SetPropertyValue(surface, "Pitch", 1);
		SetPropertyValue(surface, "Bits", new byte[] { 1 });
		SetPropertyValue(surface, "IsPrimary", false); // Backbuffer is not primary
		SetPropertyValue(surface, "DirectDrawHandle", ddrawHandle);
		SetPropertyValue(surface, "PaletteHandle", paletteHandle);

		// Backbuffers can have attached surfaces (for triple buffering)
		// In this test we use a backbuffer with AttachedSurfaces.Count > 0 to simulate flip chain behavior
		GetDictionaryField(ddrawModule, "_surfaces")[handle] = surface;
		return surface;
	}

	private static object AddOffscreenSurface(DDrawModule ddrawModule, uint handle, uint ddrawHandle, uint paletteHandle)
	{
		var surface = CreateNestedInstance("DirectDrawSurface");
		SetPropertyValue(surface, "Handle", handle);
		SetPropertyValue(surface, "Width", 1);
		SetPropertyValue(surface, "Height", 1);
		SetPropertyValue(surface, "Pitch", 1);
		SetPropertyValue(surface, "Bits", new byte[] { 1 });
		SetPropertyValue(surface, "IsPrimary", false);
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
#pragma warning disable CS0067
		public event EventHandler<UIEventArgs>? UIEvent;
#pragma warning restore CS0067

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

	private sealed class TestBackendFactory : IBackendFactory
	{
		public BackendType CurrentBackendType { get; set; } = BackendType.Headless;
		public ResizableTestRenderingBackend? NextBackend { get; set; }
		public ResizableTestRenderingBackend InitialBackend => NextBackend!;
		public ResizableTestRenderingBackend CreatedBackend { get; private set; } = null!;
		public int CreateRenderingBackendWithHostCallCount { get; private set; }

		public IRenderingBackend CreateRenderingBackend(ILogger logger) => throw new NotSupportedException();
		public IAudioBackend CreateAudioBackend(ILogger logger) => throw new NotSupportedException();
		public IInputBackend CreateInputBackend(ILogger logger) => throw new NotSupportedException();

		public IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host)
		{
			CreateRenderingBackendWithHostCallCount++;
			CreatedBackend = new ResizableTestRenderingBackend();
			return CreatedBackend;
		}
	}

	private sealed class ResizableTestRenderingBackend : IRenderingBackend
	{
		public ResizableTestRenderingBackend(bool isInitialized = false, int width = 0, int height = 0)
		{
			IsInitialized = isInitialized;
			Width = width;
			Height = height;
		}

#pragma warning disable CS0067
		public event EventHandler<UIEventArgs>? UIEvent;
#pragma warning restore CS0067

		public bool IsInitialized { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int InitializeCallCount { get; private set; }
		public int DisposeCallCount { get; private set; }

		public Task<bool> InitializeAsync(int width, int height, string title = "Win32Emu Display")
		{
			InitializeCallCount++;
			Width = width;
			Height = height;
			IsInitialized = true;
			return Task.FromResult(true);
		}

		public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch) => throw new NotSupportedException();
		public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch) => throw new NotSupportedException();
		public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch) => throw new NotSupportedException();
		public bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default) => true;
		public void Clear(byte r, byte g, byte b, byte a = 255) { }
		public void ProcessEvents() { }
		public void BeginFrame() { }
		public void EndFrame() { }
		public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices) { }
		public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format) { }
		public void BindTexture(uint textureId) { }
		public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull) { }
		public void DeleteTexture(uint textureId) { }

		public void Dispose()
		{
			DisposeCallCount++;
			IsInitialized = false;
		}
	}
}
