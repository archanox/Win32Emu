/**
 * Quick test to measure log output volume in WASM frontend
 * This test runs for 10 seconds and counts debug log messages
 */

const { chromium } = require('playwright');
const http = require('http');
const path = require('path');
const fs = require('fs');

// Configuration
const PORT = 8080;
const WWWROOT = path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net9.0/publish/wwwroot');
const TEST_DURATION_MS = 10000; // 10 seconds

/**
 * Simple static file server
 */
function createServer() {
    const server = http.createServer((req, res) => {
        let url = req.url;
        if (url.startsWith('/Win32Emu/emulator/')) {
            url = url.substring('/Win32Emu/emulator'.length);
        }
        
        let filePath = path.join(WWWROOT, url === '/' ? 'index.html' : url);
        
        if (!filePath.startsWith(WWWROOT)) {
            res.writeHead(403);
            res.end('Forbidden');
            return;
        }

        const extname = path.extname(filePath);
        const contentTypeMap = {
            '.html': 'text/html',
            '.js': 'application/javascript',
            '.css': 'text/css',
            '.json': 'application/json',
            '.png': 'image/png',
            '.wasm': 'application/wasm',
            '.dll': 'application/octet-stream',
            '.exe': 'application/octet-stream',
            '.br': 'application/octet-stream',
            '.gz': 'application/gzip'
        };

        const contentType = contentTypeMap[extname] || 'application/octet-stream';

        fs.readFile(filePath, (error, content) => {
            if (error) {
                if (error.code === 'ENOENT') {
                    res.writeHead(404);
                    res.end('File not found: ' + req.url);
                } else {
                    res.writeHead(500);
                    res.end('Server error: ' + error.code);
                }
            } else {
                res.writeHead(200, { 
                    'Content-Type': contentType,
                    'Cross-Origin-Embedder-Policy': 'require-corp',
                    'Cross-Origin-Opener-Policy': 'same-origin'
                });
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
    console.log('🧪 Testing WASM Logging Performance');
    console.log('===================================');

    const server = createServer();
    await new Promise((resolve) => {
        server.listen(PORT, () => {
            console.log(`✅ Web server started on http://localhost:${PORT}`);
            resolve();
        });
    });

    let browser;
    let context;
    let page;
    
    try {
        browser = await chromium.launch({
            headless: true,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });

        context = await browser.newContext({
            viewport: { width: 1280, height: 720 }
        });

        page = await context.newPage();

        // Collect console messages and categorize them
        const consoleMessages = [];
        const wasmRenderingLogs = [];
        
        page.on('console', msg => {
            const text = msg.text();
            consoleMessages.push({ type: msg.type(), text, timestamp: Date.now() });
            
            // Track WasmRenderingBackend logs specifically
            if (text.includes('[WASM]') && text.includes('WasmRenderingBackend')) {
                wasmRenderingLogs.push(text);
            }
        });

        console.log(`📄 Navigating to http://localhost:${PORT}/Win32Emu/emulator/`);
        await page.goto(`http://localhost:${PORT}/Win32Emu/emulator/`, { waitUntil: 'networkidle', timeout: 60000 });

        console.log('⏳ Waiting for Blazor to initialize...');
        await page.waitForSelector('#app', { timeout: 30000 });
        await page.waitForFunction(() => {
            const app = document.getElementById('app');
            return app && !app.querySelector('.loading-progress');
        }, { timeout: 60000 });

        console.log('✅ Blazor initialized');

        // Click on "IGN_TEAS Game" button
        console.log('🖱️  Loading IGN_TEAS Game...');
        await page.click('button:has-text("IGN_TEAS Game")');
        await page.waitForTimeout(3000);

        // Start emulation
        console.log('▶️  Starting emulation...');
        const startButton = await page.$('button:has-text("Start")');
        if (startButton && await startButton.isEnabled()) {
            await startButton.click();
            console.log('✅ Emulation started');
        }

        // Clear message arrays and start monitoring
        const startTime = Date.now();
        consoleMessages.length = 0;
        wasmRenderingLogs.length = 0;
        
        console.log(`⏱️  Monitoring logs for ${TEST_DURATION_MS / 1000} seconds...`);
        
        // Wait for test duration
        await page.waitForTimeout(TEST_DURATION_MS);
        
        const duration = (Date.now() - startTime) / 1000;
        
        // Analyze results
        console.log('\n📊 Results');
        console.log('=========');
        console.log(`Duration: ${duration.toFixed(2)} seconds`);
        console.log(`Total console messages: ${consoleMessages.length}`);
        console.log(`Messages per second: ${(consoleMessages.length / duration).toFixed(2)}`);
        console.log(`\nWasmRenderingBackend logs: ${wasmRenderingLogs.length}`);
        console.log(`WasmRenderingBackend logs per second: ${(wasmRenderingLogs.length / duration).toFixed(2)}`);
        
        // Break down by log content
        const updateFrameBufferLogs = wasmRenderingLogs.filter(log => log.includes('UpdateFrameBuffer called'));
        const updateCanvasLogs = wasmRenderingLogs.filter(log => log.includes('updateCanvasWithErrorHandling'));
        const canvasCompleteLogs = wasmRenderingLogs.filter(log => log.includes('Canvas update completed successfully'));
        
        console.log(`\nDetailed breakdown:`);
        console.log(`  "UpdateFrameBuffer called" logs: ${updateFrameBufferLogs.length}`);
        console.log(`  "updateCanvasWithErrorHandling" logs: ${updateCanvasLogs.length}`);
        console.log(`  "Canvas update completed successfully" logs: ${canvasCompleteLogs.length}`);
        
        // Expected: With LogTrace, these should be 0 unless trace logging is enabled
        const expectedPerFrameLogs = 3;
        const totalFrameLogs = updateFrameBufferLogs.length + updateCanvasLogs.length + canvasCompleteLogs.length;
        const estimatedFrameCount = totalFrameLogs / expectedPerFrameLogs;
        
        if (totalFrameLogs > 0) {
            console.log(`\n⚠️  Found ${totalFrameLogs} per-frame logs (should be 0 with LogTrace)`);
            console.log(`    This suggests ~${estimatedFrameCount.toFixed(0)} frames were logged`);
            console.log(`    At 3 logs per frame, this is ${(totalFrameLogs / duration).toFixed(2)} logs/sec`);
        } else {
            console.log(`\n✅ No per-frame logs detected - logging optimization successful!`);
            console.log(`    (LogTrace messages are not shown at default Debug log level)`);
        }

        return 0;

    } catch (error) {
        console.error('❌ Test failed:', error);
        return 1;
    } finally {
        if (browser) {
            await browser.close();
            console.log('\n🔒 Browser closed');
        }
        
        server.close();
        console.log('🔒 Web server stopped');
    }
}

// Run the test
runTest()
    .then(exitCode => process.exit(exitCode))
    .catch(error => {
        console.error('Fatal error:', error);
        process.exit(1);
    });
