/**
 * Test script for Win32Emu WASM DirectDraw sample using Playwright
 * 
 * This script:
 * 1. Starts a local web server for the WASM app
 * 2. Launches a browser with Playwright
 * 3. Loads the simple_ddraw.exe sample
 * 4. Monitors for errors and captures screenshots
 * 5. Tests keyboard input (ESC to exit)
 * 6. Captures diagnostic information
 */

const { chromium } = require('playwright');
const http = require('http');
const https = require('https');
const path = require('path');
const fs = require('fs');

// Configuration
const PORT = 8080;
const WWWROOT = path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net9.0/publish/wwwroot');
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots');
const TEST_TIMEOUT = 120000; // 2 minutes

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

/**
 * Simple static file server
 */
function createServer() {
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

        const extname = path.extname(filePath);
        const contentTypeMap = {
            '.html': 'text/html',
            '.js': 'application/javascript',
            '.css': 'text/css',
            '.json': 'application/json',
            '.png': 'image/png',
            '.jpg': 'image/jpg',
            '.gif': 'image/gif',
            '.svg': 'image/svg+xml',
            '.ico': 'image/x-icon',
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
 * Wait for a condition with timeout
 */
async function waitFor(conditionFn, timeout = 30000, interval = 1000) {
    const startTime = Date.now();
    while (Date.now() - startTime < timeout) {
        if (await conditionFn()) {
            return true;
        }
        await new Promise(resolve => setTimeout(resolve, interval));
    }
    return false;
}

/**
 * Main test function
 */
async function runTest() {
    console.log('🧪 Starting DirectDraw WASM Test');
    console.log('================================');

    // Start local web server
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
        // Launch browser
        console.log('🌐 Launching Chromium browser...');
        browser = await chromium.launch({
            headless: true,
            args: [
                '--no-sandbox',
                '--disable-setuid-sandbox',
                '--disable-dev-shm-usage'
            ]
        });

        context = await browser.newContext({
            viewport: { width: 1280, height: 720 }
        });

        page = await context.newPage();

        // Collect console messages
        const consoleMessages = [];
        page.on('console', msg => {
            const text = msg.text();
            consoleMessages.push({ type: msg.type(), text });
            console.log(`[Browser ${msg.type()}]`, text);
        });

        // Collect errors
        const errors = [];
        page.on('pageerror', error => {
            console.error('❌ [Page Error]', error.message);
            errors.push(error.message);
        });

        // Navigate to the page (with GitHub Pages base path)
        console.log(`📄 Navigating to http://localhost:${PORT}/Win32Emu/emulator/`);
        await page.goto(`http://localhost:${PORT}/Win32Emu/emulator/`, { waitUntil: 'networkidle', timeout: 60000 });

        // Wait for Blazor to initialize
        console.log('⏳ Waiting for Blazor to initialize...');
        await page.waitForSelector('#app', { timeout: 30000 });
        await page.waitForFunction(() => {
            const app = document.getElementById('app');
            return app && !app.querySelector('.loading-progress');
        }, { timeout: 60000 });

        console.log('✅ Blazor initialized');

        // Take initial screenshot
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, '01-initial-load.png') });
        console.log('📸 Screenshot saved: 01-initial-load.png');

        // Click on "Simple DirectDraw" sample button
        console.log('🖱️  Clicking "Simple DirectDraw" button...');
        await page.click('button:has-text("Simple DirectDraw")');
        await page.waitForTimeout(2000);

        // Check if executable is loaded
        const isLoaded = await page.evaluate(() => {
            const loadedText = document.body.innerText;
            return loadedText.includes('simple_ddraw.exe');
        });

        if (!isLoaded) {
            throw new Error('Failed to load simple_ddraw.exe');
        }

        console.log('✅ Sample executable loaded');
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, '02-sample-loaded.png') });
        console.log('📸 Screenshot saved: 02-sample-loaded.png');

        // Click "Start" button
        console.log('▶️  Starting emulation...');
        await page.click('button:has-text("Start")');
        await page.waitForTimeout(3000);

        // Take screenshot after starting
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, '03-emulation-started.png') });
        console.log('📸 Screenshot saved: 03-emulation-started.png');

        // Wait for emulator to be running
        const isRunning = await page.evaluate(() => {
            const badge = document.querySelector('.badge.bg-success');
            return badge && badge.textContent.includes('Running');
        });

        if (!isRunning) {
            throw new Error('Emulator did not start');
        }

        console.log('✅ Emulator is running');

        // Monitor canvas updates for 10 seconds
        console.log('🎨 Monitoring canvas updates for 10 seconds...');
        let canvasUpdates = 0;
        const startTime = Date.now();
        
        while (Date.now() - startTime < 10000) {
            const updateCount = await page.evaluate(() => {
                return window.ddrawDiagnostics?.canvasUpdateCount || 0;
            });
            
            if (updateCount > canvasUpdates) {
                console.log(`   Canvas updates: ${updateCount}`);
                canvasUpdates = updateCount;
            }
            
            await page.waitForTimeout(1000);
        }

        // Get DirectDraw diagnostics
        console.log('📊 Reading DirectDraw diagnostics...');
        const diagnostics = await page.evaluate(() => {
            return {
                canvasUpdateCount: window.ddrawDiagnostics?.canvasUpdateCount || 0,
                backendInitialized: window.ddrawDiagnostics?.backendInitialized || false,
                renderingError: window.ddrawDiagnostics?.renderingError || false,
                frameBufferSize: window.ddrawDiagnostics?.frameBufferSize || 0
            };
        });

        console.log('   Canvas Updates:', diagnostics.canvasUpdateCount);
        console.log('   Backend Initialized:', diagnostics.backendInitialized);
        console.log('   Rendering Error:', diagnostics.renderingError);
        console.log('   Frame Buffer Size:', diagnostics.frameBufferSize, 'bytes');

        // Take screenshot of running emulator
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, '04-emulation-running.png') });
        console.log('📸 Screenshot saved: 04-emulation-running.png');

        // Get debug output
        const debugOutput = await page.evaluate(() => {
            const debugPanel = document.querySelector('pre.log-panel');
            return debugPanel ? debugPanel.textContent : '';
        });

        // Save debug output to file
        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'debug-output.txt'),
            debugOutput
        );
        console.log('📝 Debug output saved to debug-output.txt');

        // Get DirectDraw calls log
        const ddrawLog = await page.evaluate(() => {
            const logEl = document.getElementById('ddrawCallsLog');
            return logEl ? logEl.textContent : '';
        });

        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'ddraw-calls.txt'),
            ddrawLog
        );
        console.log('📝 DirectDraw calls log saved to ddraw-calls.txt');

        // Test ESC key press
        console.log('⌨️  Testing ESC key press...');
        const canvas = await page.$('#emulatorCanvas');
        if (canvas) {
            await canvas.focus();
            await page.keyboard.press('Escape');
            await page.waitForTimeout(2000);
        }

        // Take final screenshot
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, '05-after-esc.png') });
        console.log('📸 Screenshot saved: 05-after-esc.png');

        // Check for errors
        if (errors.length > 0) {
            console.error('⚠️  Errors detected during test:');
            errors.forEach((err, i) => console.error(`   ${i + 1}. ${err}`));
        }

        // Summary
        console.log('\n📋 Test Summary');
        console.log('==============');
        console.log('✅ WASM app loaded successfully');
        console.log(`✅ Canvas updates: ${diagnostics.canvasUpdateCount}`);
        console.log(`${diagnostics.renderingError ? '❌' : '✅'} Rendering status: ${diagnostics.renderingError ? 'ERROR' : 'OK'}`);
        console.log(`${errors.length > 0 ? '⚠️' : '✅'} Browser errors: ${errors.length}`);
        console.log(`✅ Screenshots saved to: ${SCREENSHOT_DIR}`);

        // Determine test result
        const testPassed = canvasUpdates > 0 && !diagnostics.renderingError && errors.length === 0;
        
        if (testPassed) {
            console.log('\n✅ TEST PASSED');
        } else {
            console.log('\n❌ TEST FAILED');
            if (canvasUpdates === 0) {
                console.log('   - No canvas updates detected');
            }
            if (diagnostics.renderingError) {
                console.log('   - Rendering error occurred');
            }
            if (errors.length > 0) {
                console.log(`   - ${errors.length} browser error(s) detected`);
            }
        }

        return testPassed ? 0 : 1;

    } catch (error) {
        console.error('❌ Test failed with exception:', error);
        
        // Try to take error screenshot
        if (page) {
            try {
                await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'error-screenshot.png') });
                console.log('📸 Error screenshot saved');
            } catch (e) {
                console.error('Failed to capture error screenshot:', e.message);
            }
        }
        
        return 1;
    } finally {
        // Cleanup
        if (browser) {
            await browser.close();
            console.log('🔒 Browser closed');
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
