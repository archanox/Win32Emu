/**
 * Test script for dd_image.exe WASM rendering using Playwright
 * 
 * This script:
 * 1. Starts a local web server for the WASM app
 * 2. Launches a browser with Playwright
 * 3. Loads dd_image.exe sample
 * 4. Monitors for errors and captures screenshots
 * 5. Captures diagnostic information
 */

const { chromium } = require('playwright');
const http = require('http');
const path = require('path');
const fs = require('fs');

// Configuration
const PORT = 8080;
const WWWROOT = path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot');
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots');
const TEST_TIMEOUT = 60000; // 1 minute

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

/**
 * Simple static file server
 */
function createServer() {
    const mimeTypes = {
        '.html': 'text/html',
        '.js': 'application/javascript',
        '.css': 'text/css',
        '.json': 'application/json',
        '.png': 'image/png',
        '.jpg': 'image/jpeg',
        '.gif': 'image/gif',
        '.svg': 'image/svg+xml',
        '.wav': 'audio/wav',
        '.mp4': 'video/mp4',
        '.woff': 'application/font-woff',
        '.ttf': 'application/font-ttf',
        '.eot': 'application/vnd.ms-fontobject',
        '.otf': 'application/font-otf',
        '.wasm': 'application/wasm',
        '.dll': 'application/octet-stream',
        '.exe': 'application/octet-stream',
        '.pdb': 'application/octet-stream',
        '.dat': 'application/octet-stream',
        '.blat': 'application/octet-stream'
    };

    const server = http.createServer((req, res) => {
        // Remove base path prefix if present (GitHub Pages uses /Win32Emu/emulator/)
        let url = req.url;
        if (url.startsWith('/Win32Emu/emulator/')) {
            url = url.substring('/Win32Emu/emulator'.length);
        }
        
        // Sanitize the URL to prevent directory traversal
        let filePath = path.join(WWWROOT, url === '/' ? 'index.html' : url);
        
        // Prevent directory traversal
        if (!filePath.startsWith(WWWROOT)) {
            res.writeHead(403);
            res.end('Forbidden');
            return;
        }
        
        const extname = String(path.extname(filePath)).toLowerCase();
        const mimeType = mimeTypes[extname] || 'application/octet-stream';
        
        fs.readFile(filePath, (error, content) => {
            if (error) {
                if (error.code == 'ENOENT') {
                    console.log(`404: ${url}`);
                    res.writeHead(404);
                    res.end('File not found');
                } else {
                    console.error(`Server error: ${error.code}`);
                    res.writeHead(500);
                    res.end(`Server error: ${error.code}`);
                }
            } else {
                res.writeHead(200, { 'Content-Type': mimeType });
                res.end(content, 'utf-8');
            }
        });
    });
    
    return server;
}

/**
 * Main test function
 */
async function runTest() {
    let server = null;
    let browser = null;
    let exitCode = 0;
    
    try {
        // Start server
        server = createServer();
        await new Promise((resolve, reject) => {
            server.listen(PORT, () => {
                console.log(`🌐 Server started at http://localhost:${PORT}`);
                resolve();
            });
            server.on('error', reject);
        });
        
        // Launch browser
        console.log('🚀 Launching browser...');
        browser = await chromium.launch({
            headless: true,
            args: [
                '--no-sandbox',
                '--disable-setuid-sandbox',
                '--disable-dev-shm-usage',
                '--disable-web-security'  // For WASM
            ]
        });
        
        const context = await browser.newContext({
            viewport: { width: 1280, height: 720 }
        });
        
        const page = await context.newPage();
        
        // Setup console logging
        page.on('console', msg => {
            const type = msg.type();
            const text = msg.text();
            console.log(`[Browser ${type}] ${text}`);
        });
        
        // Setup error handling
        page.on('pageerror', error => {
            console.error('[Browser Error]', error.message);
        });
        
        // Navigate to page
        console.log(`📄 Loading page: http://localhost:${PORT}`);
        await page.goto(`http://localhost:${PORT}`, { waitUntil: 'networkidle', timeout: 30000 });
        
        // Wait for Blazor to load
        console.log('⏳ Waiting for Blazor to initialize...');
        await page.waitForFunction(() => {
            return document.querySelector('.loading-progress') === null ||
                   window.getComputedStyle(document.querySelector('.loading-progress')).display === 'none';
        }, { timeout: 30000 });
        
        console.log('✅ Blazor initialized');
        
        // Take initial screenshot
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'dd_image_01_initial.png'), fullPage: true });
        console.log('📸 Initial screenshot taken');
        
        // Load dd_image.exe sample
        console.log('🎮 Loading dd_image.exe...');
        await page.evaluate(async () => {
            // Call the sample loading function
            const response = await fetch('samples/dd_image.exe');
            const bytes = new Uint8Array(await response.arrayBuffer());
            
            // Simulate file selection by calling internal functions
            // This mimics what LoadSampleExecutable does
            window._executableData = bytes;
            window._loadedFileName = 'dd_image.exe';
            window._loadedFileSize = bytes.length;
            window._executableLoaded = true;
            window._folderMode = false;
            
            console.log(`dd_image.exe loaded: ${bytes.length} bytes`);
        });
        
        // Click the load button (or trigger loading programmatically)
        await page.click('button.btn-outline-primary:has-text("Simple DirectDraw")').catch(() => {
            console.log('Could not find sample button, trying alternative method...');
        });
        
        await page.waitForTimeout(2000);
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'dd_image_02_after_load.png'), fullPage: true });
        console.log('📸 Screenshot after loading sample taken');
        
        // Click start button
        console.log('▶️  Starting emulation...');
        await page.click('button.btn-success:has-text("Start")');
        
        // Wait for emulation to start and render frames
        console.log('⏳ Waiting for rendering (10 seconds)...');
        await page.waitForTimeout(10000);
        
        // Take screenshot after emulation starts
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'dd_image_03_running.png'), fullPage: true });
        console.log('📸 Screenshot while running taken');
        
        // Get DirectDraw diagnostics
        console.log('📊 Getting DirectDraw diagnostics...');
        const diagnostics = await page.evaluate(() => {
            if (window.ddrawDiagnostics) {
                return {
                    canvasUpdateCount: window.ddrawDiagnostics.canvasUpdateCount,
                    lastUpdateTime: window.ddrawDiagnostics.lastUpdateTime,
                    backendInitialized: window.ddrawDiagnostics.backendInitialized,
                    renderingError: window.ddrawDiagnostics.renderingError,
                    frameBufferSize: window.ddrawDiagnostics.frameBufferSize
                };
            }
            return null;
        });
        
        console.log('DirectDraw Diagnostics:', JSON.stringify(diagnostics, null, 2));
        
        // Check canvas content
        const canvasInfo = await page.evaluate(() => {
            const canvas = document.getElementById('emulatorCanvas');
            if (!canvas) return { exists: false };
            
            const ctx = canvas.getContext('2d');
            if (!ctx) return { exists: true, has2dContext: false };
            
            const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            const data = imageData.data;
            
            let nonBlackPixels = 0;
            let firstNonBlackPixel = null;
            
            // Check if any pixel is non-black
            for (let i = 0; i < data.length; i += 4) {
                if (data[i] !== 0 || data[i+1] !== 0 || data[i+2] !== 0) {
                    nonBlackPixels++;
                    if (!firstNonBlackPixel) {
                        firstNonBlackPixel = {
                            r: data[i],
                            g: data[i+1],
                            b: data[i+2],
                            a: data[i+3]
                        };
                    }
                }
            }
            
            return {
                exists: true,
                has2dContext: true,
                width: canvas.width,
                height: canvas.height,
                totalPixels: data.length / 4,
                nonBlackPixels: nonBlackPixels,
                firstNonBlackPixel: firstNonBlackPixel,
                hasContent: nonBlackPixels > 0
            };
        });
        
        console.log(`Canvas Info:`, JSON.stringify(canvasInfo, null, 2));
        console.log(`Canvas has content: ${canvasInfo.hasContent ? 'YES ✅' : 'NO ❌'}`);
        
        if (!canvasInfo.hasContent) {
            console.error('❌ TEST FAILED: Canvas is empty - no rendering occurred');
            exitCode = 1;
        } else {
            console.log('✅ TEST PASSED: Canvas has content');
        }
        
        // Get console logs from the page console
        const consoleLogs = await page.evaluate(() => {
            if (window.pageConsole && window.pageConsole.getLogs) {
                return window.pageConsole.getLogs();
            }
            return [];
        });
        
        console.log(`\nPage Console Logs (last 20):`);
        consoleLogs.slice(-20).forEach(log => {
            console.log(`  [${log.timestamp}] [${log.level}] ${log.message.substring(0, 200)}`);
        });
        
    } catch (error) {
        console.error('❌ Test error:', error);
        exitCode = 1;
    } finally {
        // Cleanup
        if (browser) {
            await browser.close();
            console.log('🔒 Browser closed');
        }
        if (server) {
            server.close();
            console.log('🔒 Server closed');
        }
    }
    
    process.exit(exitCode);
}

// Run test
runTest();
