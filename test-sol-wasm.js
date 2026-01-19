/**
 * Test script for running sol.exe (Windows Solitaire) on Win32Emu WASM using Playwright
 * 
 * This script:
 * 1. Starts a local web server for the WASM app
 * 2. Launches a browser with Playwright
 * 3. Loads sol.exe from EXEs/WinME/
 * 4. Monitors for errors during Win16 module registration
 * 5. Captures screenshots and diagnostic information
 * 6. Verifies the fix for Win16 module registration crash
 */

const { chromium } = require('playwright');
const http = require('http');
const path = require('path');
const fs = require('fs');

// Configuration
const PORT = 8080;
const WWWROOT = path.join(__dirname, 'Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot');
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots');
const SOL_EXE_PATH = path.join(__dirname, 'EXEs/WinME/sol.exe');

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

// Check if sol.exe exists
if (!fs.existsSync(SOL_EXE_PATH)) {
    console.error('❌ sol.exe not found at:', SOL_EXE_PATH);
    console.error('   Please ensure EXEs/WinME/sol.exe exists');
    process.exit(1);
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
 * Main test function
 */
async function runTest() {
    console.log('🧪 Starting sol.exe WASM Test');
    console.log('==============================');
    console.log('Testing Win16 module registration fix for sol.exe crash');
    console.log('');

    // Check if WASM build exists
    if (!fs.existsSync(WWWROOT)) {
        console.error('❌ WASM build not found at:', WWWROOT);
        console.error('   Please run: dotnet publish Win32Emu.Wasm/Win32Emu.Wasm.csproj -c Release');
        process.exit(1);
    }

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
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'sol-01-initial-load.png') });
        console.log('📸 Screenshot saved: sol-01-initial-load.png');

        // Load sol.exe file
        console.log('📁 Loading sol.exe...');
        const solExeContent = fs.readFileSync(SOL_EXE_PATH);
        const solExeBase64 = solExeContent.toString('base64');

        // Upload sol.exe via file input (simulate file selection)
        await page.evaluate((base64Content) => {
            // Create a File object from base64
            const byteCharacters = atob(base64Content);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const file = new File([byteArray], 'sol.exe', { type: 'application/octet-stream' });
            
            // Store in window for later use
            window.testSolExeFile = file;
        }, solExeBase64);

        // Find and trigger file input
        const fileInput = await page.$('input[type="file"]');
        if (fileInput) {
            // Use setInputFiles to load sol.exe
            await page.setInputFiles('input[type="file"]', SOL_EXE_PATH);
            console.log('✅ sol.exe uploaded via file input');
        } else {
            console.log('⚠️  File input not found, trying alternative method...');
            
            // Try clicking "Choose EXE" button and manually triggering
            await page.click('button:has-text("Choose EXE")');
            await page.waitForTimeout(1000);
            
            const fileInputAfter = await page.$('input[type="file"]');
            if (fileInputAfter) {
                await page.setInputFiles('input[type="file"]', SOL_EXE_PATH);
                console.log('✅ sol.exe uploaded after clicking button');
            }
        }
        
        await page.waitForTimeout(2000);

        // Take screenshot after loading file
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'sol-02-file-loaded.png') });
        console.log('📸 Screenshot saved: sol-02-file-loaded.png');

        // Check if file was loaded
        const isFileLoaded = await page.evaluate(() => {
            const bodyText = document.body.innerText;
            return bodyText.includes('sol.exe') || bodyText.includes('SOL.EXE');
        });

        if (!isFileLoaded) {
            console.log('⚠️  sol.exe may not be loaded, checking for alternative UI...');
        } else {
            console.log('✅ sol.exe file detected in UI');
        }

        // Click "Start" button to begin emulation
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
        } else {
            console.log('⚠️  Start button not found');
        }
        
        // Wait for emulator to start
        await page.waitForTimeout(10000);

        // Take screenshot after starting
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'sol-03-emulation-started.png') });
        console.log('📸 Screenshot saved: sol-03-emulation-started.png');

        // Check for Win16 module registration logs
        console.log('');
        console.log('🔍 Checking for Win16 module registration logs...');
        console.log('================================================');
        
        const win16Logs = consoleMessages.filter(msg => 
            msg.text.includes('Registering Win16') ||
            msg.text.includes('Looking up KERNEL32') ||
            msg.text.includes('Looking up USER32') ||
            msg.text.includes('Looking up GDI32') ||
            msg.text.includes('Looking up WINMM') ||
            msg.text.includes('Win16 thunking modules registered') ||
            msg.text.includes('Creating Win16')
        );

        if (win16Logs.length > 0) {
            console.log('✅ Found Win16 module registration logs:');
            win16Logs.forEach(log => {
                console.log(`   ${log.text}`);
            });
        } else {
            console.log('⚠️  No Win16 module registration logs found');
        }

        // Check for errors during module registration
        console.log('');
        console.log('🔍 Checking for errors...');
        console.log('========================');
        
        const errorLogs = consoleMessages.filter(msg => 
            msg.type === 'error' || 
            msg.text.toLowerCase().includes('error') ||
            msg.text.toLowerCase().includes('exception') ||
            msg.text.toLowerCase().includes('fail')
        );

        if (errorLogs.length > 0) {
            console.log('❌ Found errors:');
            errorLogs.forEach(log => {
                console.log(`   [${log.type}] ${log.text}`);
            });
        } else {
            console.log('✅ No errors detected');
        }

        // Get debug output
        console.log('');
        console.log('📊 Capturing debug output...');
        const debugOutput = await page.evaluate(() => {
            // Try to find debug/log panels
            const logPanels = document.querySelectorAll('pre.log-panel, .log-output, textarea');
            let output = '';
            
            for (let panel of logPanels) {
                if (panel.textContent || panel.value) {
                    output += (panel.textContent || panel.value) + '\n\n';
                }
            }
            
            return output || 'No debug panel found';
        });

        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'sol-debug-output.txt'),
            debugOutput
        );
        console.log('📝 Debug output saved to sol-debug-output.txt');

        // Save console messages
        fs.writeFileSync(
            path.join(SCREENSHOT_DIR, 'sol-console-messages.json'),
            JSON.stringify(consoleMessages, null, 2)
        );
        console.log('📝 Console messages saved to sol-console-messages.json');

        // Final screenshot
        await page.waitForTimeout(5000);
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'sol-04-final-state.png') });
        console.log('📸 Screenshot saved: sol-04-final-state.png');

        // Summary
        console.log('');
        console.log('📋 Test Summary');
        console.log('===============');
        console.log(`Total console messages: ${consoleMessages.length}`);
        console.log(`Win16 registration logs: ${win16Logs.length}`);
        console.log(`Errors: ${errorLogs.length}`);
        console.log(`Page errors: ${errors.length}`);
        
        if (errorLogs.length === 0 && errors.length === 0 && win16Logs.length > 0) {
            console.log('');
            console.log('✅ TEST PASSED: sol.exe loaded successfully with Win16 module registration!');
            console.log('   The fix appears to be working correctly in WASM.');
        } else if (errorLogs.length > 0 || errors.length > 0) {
            console.log('');
            console.log('❌ TEST FAILED: Errors detected during sol.exe loading');
            console.log('   Check debug output and console messages for details.');
        } else {
            console.log('');
            console.log('⚠️  TEST INCONCLUSIVE: No clear success or failure indicators');
            console.log('   Check screenshots and debug output for more information.');
        }

    } catch (error) {
        console.error('');
        console.error('❌ Test failed with error:');
        console.error(error);
        
        // Try to take error screenshot
        if (page) {
            try {
                await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'sol-error.png') });
                console.log('📸 Error screenshot saved: sol-error.png');
            } catch (screenshotError) {
                // Ignore screenshot errors
            }
        }
    } finally {
        // Cleanup
        if (browser) {
            await browser.close();
            console.log('🌐 Browser closed');
        }
        
        server.close(() => {
            console.log('🛑 Web server stopped');
        });
    }
}

// Run the test
runTest().catch(error => {
    console.error('Fatal error:', error);
    process.exit(1);
});
