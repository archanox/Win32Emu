/**
 * Test script for running ign_teas game on Win32Emu WASM frontend using Playwright
 * 
 * This script:
 * 1. Starts a local web server for the WASM app
 * 2. Launches a browser with Playwright
 * 3. Loads the ign_teas game (executable + DATA folder)
 * 4. Monitors for errors and canvas rendering
 * 5. Captures screenshots and diagnostic information
 * 6. Identifies issues preventing game display
 */

const { chromium } = require('playwright');
const http = require('http');
const path = require('path');
const fs = require('fs');

// Configuration
const PORT = 8080;
function resolveWwwRoot() {
    const candidates = [
        path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot'),
        path.join(__dirname, 'Win32Emu.Wasm/bin/Debug/net10.0/publish/wwwroot'),
        path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net9.0/publish/wwwroot'),
        path.join(__dirname, 'Win32Emu.Wasm/bin/Debug/net9.0/publish/wwwroot'),
        path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net10.0/wwwroot'),
        path.join(__dirname, 'Win32Emu.Wasm/bin/Debug/net10.0/wwwroot')
    ];

    const isUsableWwwRoot = (candidate) => {
        if (!fs.existsSync(candidate) || !fs.statSync(candidate).isDirectory()) {
            return false;
        }

        const indexPath = path.join(candidate, 'index.html');
        const frameworkPath = path.join(candidate, '_framework');
        if (!fs.existsSync(indexPath) || !fs.existsSync(frameworkPath) || !fs.statSync(frameworkPath).isDirectory()) {
            return false;
        }

        return fs.readdirSync(frameworkPath).some(file => file.startsWith('dotnet.js'));
    };

    for (const candidate of candidates) {
        if (isUsableWwwRoot(candidate)) {
            console.log(`Using WASM wwwroot: ${candidate}`);
            return candidate;
        }
    }

    throw new Error(`Could not find published WASM wwwroot. Checked:\n${candidates.join('\n')}`);
}

const WWWROOT = resolveWwwRoot();
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots');

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
        
        const requestPath = url === '/' ? 'index.html' : url.replace(/^\/+/, '');
        const normalizedPath = path.normalize(requestPath);
        const normalizedForValidation = normalizedPath.replace(/\\/g, '/');
        if (normalizedForValidation.startsWith('..') || normalizedForValidation.includes('/../') || path.isAbsolute(normalizedPath)) {
            res.writeHead(403);
            res.end('Forbidden');
            return;
        }
        
        const resolvedRoot = path.resolve(WWWROOT);
        const filePath = path.resolve(resolvedRoot, normalizedPath);
        const rootWithSeparator = resolvedRoot.endsWith(path.sep) ? resolvedRoot : resolvedRoot + path.sep;
        
        // Prevent directory traversal
        if (filePath !== resolvedRoot && !filePath.startsWith(rootWithSeparator)) {
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
 * Main test function
 */
async function runTest() {
    console.log('🧪 Starting ign_teas WASM Test');
    console.log('==============================');

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
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-01-initial-load.png') });
        console.log('📸 Screenshot saved: ign-01-initial-load.png');

        // Click on "IGN_TEAS Game" button
        console.log('🖱️  Clicking "IGN_TEAS Game" button...');
        await page.click('button:has-text("IGN_TEAS Game")');
        await page.waitForTimeout(5000); // Wait for files to load

        // Check if game is loaded
        const isLoaded = await page.evaluate(() => {
            const loadedText = document.body.innerText;
            return loadedText.includes('IGN_TEAS.EXE');
        });

        if (!isLoaded) {
            console.log('⚠️  Game may not be loaded yet, checking debug output...');
        } else {
            console.log('✅ Game files loaded');
        }

        // Take screenshot after loading
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-02-game-loaded.png') });
        console.log('📸 Screenshot saved: ign-02-game-loaded.png');

        // Click "Start" button if it's enabled
        console.log('▶️  Starting emulation...');
        const startButton = await page.$('button:has-text("Start")');
        if (startButton) {
            const isEnabled = await startButton.isEnabled();
            if (isEnabled) {
                await startButton.click();
                console.log('✅ Start button clicked');
            } else {
                console.log('⚠️  Start button is disabled');
            }
        }
        
        await page.waitForTimeout(5000);

        // Take screenshot after starting
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-03-emulation-started.png') });
        console.log('📸 Screenshot saved: ign-03-emulation-started.png');

        // Wait for emulator to be running
        const isRunning = await page.evaluate(() => {
            const badge = document.querySelector('.badge.bg-success');
            return badge && badge.textContent.includes('Running');
        });

        if (isRunning) {
            console.log('✅ Emulator is running');
        } else {
            console.log('⚠️  Emulator may not have started');
        }

        // Monitor canvas updates for 30 seconds
        console.log('🎨 Monitoring canvas and game state for 120 seconds...');
        let canvasUpdates = 0;
        const startTime = Date.now();
        
        while (Date.now() - startTime < 120000) {
            const updateCount = await page.evaluate(() => {
                return window.ddrawDiagnostics?.canvasUpdateCount || 0;
            });
            
            if (updateCount > canvasUpdates) {
                console.log(`   Canvas updates: ${updateCount}`);
                canvasUpdates = updateCount;
            }
            
            await page.waitForTimeout(1000);
        }

        // Take screenshot of running game
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-04-game-running.png') });
        console.log('📸 Screenshot saved: ign-04-game-running.png');

        // Get debug output to see what's happening
        console.log('📊 Capturing debug output...');
        const debugOutput = await page.evaluate(() => {
            const debugPanel = document.querySelectorAll('pre.log-panel')[1];
            return debugPanel ? debugPanel.textContent : 'No debug panel found';
        });

        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'ign-debug-output.txt'),
            debugOutput
        );
        console.log('📝 Debug output saved to ign-debug-output.txt');

        // Get standard output
        const stdOutput = await page.evaluate(() => {
            const panels = document.querySelectorAll('pre.log-panel');
            return panels.length > 0 ? panels[0].textContent : 'No stdout panel found';
        });

        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'ign-stdout.txt'),
            stdOutput
        );
        console.log('📝 Standard output saved to ign-stdout.txt');

        // Get DirectDraw diagnostics
        console.log('📊 Reading DirectDraw diagnostics...');
        const diagnostics = await page.evaluate(() => {
            const canvasId = window.ddrawDiagnostics?.lastCanvasId || null;
            const canvas = canvasId ? document.getElementById(canvasId) : null;
            const sampleStats = typeof window.sampleCanvasPixels === 'function'
                ? window.sampleCanvasPixels(canvas)
                : { samplePixelCount: 0, sampleNonBlackPixels: 0, firstVisibleColor: null };

            return {
                canvasUpdateCount: window.ddrawDiagnostics?.canvasUpdateCount || 0,
                backendInitialized: window.ddrawDiagnostics?.backendInitialized || false,
                renderingError: window.ddrawDiagnostics?.renderingError || false,
                frameBufferSize: window.ddrawDiagnostics?.frameBufferSize || 0,
                lastCanvasId: canvasId,
                samplePixelCount: sampleStats.samplePixelCount,
                sampleNonBlackPixels: sampleStats.sampleNonBlackPixels,
                firstVisibleColor: sampleStats.firstVisibleColor
            };
        });

        console.log('   Canvas Updates:', diagnostics.canvasUpdateCount);
        console.log('   Backend Initialized:', diagnostics.backendInitialized);
        console.log('   Rendering Error:', diagnostics.renderingError);
        console.log('   Frame Buffer Size:', diagnostics.frameBufferSize, 'bytes');
        console.log('   Last Canvas ID:', diagnostics.lastCanvasId);
        console.log('   Sampled Visible Pixels:', `${diagnostics.sampleNonBlackPixels} / ${diagnostics.samplePixelCount}`);
        console.log('   First Visible Color:', diagnostics.firstVisibleColor ?? 'None');

        // Check for errors
        console.log('\n📋 Test Summary');
        console.log('==============');
        console.log('✅ WASM app loaded successfully');
        console.log(`✅ Canvas updates: ${diagnostics.canvasUpdateCount}`);
        console.log(`${diagnostics.renderingError ? '❌' : '✅'} Rendering status: ${diagnostics.renderingError ? 'ERROR' : 'OK'}`);
        
        if (errors.length > 0) {
            console.log('⚠️  Errors detected during test:');
            errors.forEach((err, i) => console.error(`   ${i + 1}. ${err}`));
        } else {
            console.log('✅ No browser errors detected');
        }

        console.log(`✅ Screenshots saved to: ${SCREENSHOT_DIR}`);

        // Analyze what we've learned
        console.log('\n🔍 Analysis');
        console.log('==========');
        
        if (canvasUpdates === 0) {
            console.log('❌ Issue: No canvas updates detected - game is not rendering');
            console.log('   Possible causes:');
            console.log('   1. DirectDraw initialization failed');
            console.log('   2. Game crashed during startup');
            console.log('   3. Canvas rendering backend not working properly');
            console.log('   4. Game waiting for input or stuck in a loop');
        } else {
            console.log(`✅ Canvas is being updated (${canvasUpdates} updates)`);
        }
        
        if (diagnostics.sampleNonBlackPixels === 0) {
            console.log('❌ Canvas updates are occurring, but sampled pixels are still all black');
        } else {
            console.log(`✅ Canvas contains visible non-black pixels (${diagnostics.sampleNonBlackPixels}/${diagnostics.samplePixelCount})`);
        }
        
        if (diagnostics.renderingError) {
            console.log('❌ Rendering error occurred - check debug output for details');
        }
        
        if (errors.length > 0) {
            console.log('❌ JavaScript errors detected - these may be preventing game from running');
        }
        
        console.log('\nNext steps:');
        console.log('1. Review debug output files for error messages');
        console.log('2. Check if DirectDraw APIs are being called correctly');
        console.log('3. Verify canvas rendering backend is working');
        console.log('4. Test with simpler DirectDraw samples first');

        // Final screenshot
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-05-final-state.png') });
        console.log('📸 Screenshot saved: ign-05-final-state.png');

        // Determine test result
        const renderedToWindowCanvas =
            typeof diagnostics.lastCanvasId === 'string' &&
            diagnostics.lastCanvasId.startsWith('window-canvas-');
        const testPassed =
            canvasUpdates > 0 &&
            renderedToWindowCanvas &&
            diagnostics.sampleNonBlackPixels > 0 &&
            !diagnostics.renderingError &&
            errors.length === 0;
        
        if (testPassed) {
            console.log('\n✅ TEST PASSED - Game is rendering on canvas');
        } else {
            console.log('\n⚠️  TEST INCOMPLETE - Issues need to be addressed');
            if (!renderedToWindowCanvas) {
                const actualCanvas = diagnostics.lastCanvasId ?? 'null (no rendering detected)';
                console.log(`   - Expected rendering on a window canvas, got ${actualCanvas}`);
            }
            if (diagnostics.sampleNonBlackPixels === 0) {
                console.log('   - Canvas updated, but sampled pixels remained black');
            }
        }

        return testPassed ? 0 : 1;

    } catch (error) {
        console.error('❌ Test failed with exception:', error);
        
        // Try to take error screenshot
        if (page) {
            try {
                await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'ign-error-screenshot.png') });
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
