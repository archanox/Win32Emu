// WebGPU Helper Functions for Win32Emu Browser

window.webgpuHelper = {
    // Check if WebGPU is supported
    isSupported: async function () {
        if (!navigator.gpu) {
            console.warn('WebGPU is not supported in this browser');
            return false;
        }
        
        try {
            const adapter = await navigator.gpu.requestAdapter();
            if (!adapter) {
                console.warn('No WebGPU adapter available');
                return false;
            }
            return true;
        } catch (error) {
            console.error('Error checking WebGPU support:', error);
            return false;
        }
    },

    // Initialize WebGPU context
    initialize: async function (canvasId) {
        if (!navigator.gpu) {
            throw new Error('WebGPU is not supported in this browser');
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            throw new Error(`Canvas with id '${canvasId}' not found`);
        }

        const adapter = await navigator.gpu.requestAdapter();
        if (!adapter) {
            throw new Error('Failed to get WebGPU adapter');
        }

        const device = await adapter.requestDevice();
        const context = canvas.getContext('webgpu');
        
        const canvasFormat = navigator.gpu.getPreferredCanvasFormat();
        context.configure({
            device: device,
            format: canvasFormat,
        });

        console.log('WebGPU initialized successfully');
        return {
            adapter: adapter,
            device: device,
            context: context,
            format: canvasFormat
        };
    },

    // Get canvas context for rendering
    getContext: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            throw new Error(`Canvas with id '${canvasId}' not found`);
        }
        return canvas.getContext('webgpu');
    },

    // Set canvas size
    setCanvasSize: function (canvasId, width, height) {
        const canvas = document.getElementById(canvasId);
        if (canvas) {
            canvas.width = width;
            canvas.height = height;
            canvas.style.display = 'block';
        }
    },

    // Show/hide canvas
    setCanvasVisibility: function (canvasId, visible) {
        const canvas = document.getElementById(canvasId);
        if (canvas) {
            canvas.style.display = visible ? 'block' : 'none';
        }
    }
};

// Feature detection for Playwright MCP
window.win32EmuFeatures = {
    webgpu: 'gpu' in navigator,
    webassembly: typeof WebAssembly !== 'undefined',
    sharedArrayBuffer: typeof SharedArrayBuffer !== 'undefined',
    crossOriginIsolated: window.crossOriginIsolated
};

console.log('Win32Emu WebGPU helper loaded');
console.log('Features:', window.win32EmuFeatures);
