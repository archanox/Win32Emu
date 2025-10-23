# Playwright Testing Guide

This document provides examples for testing Win32Emu.Browser with Playwright MCP.

## Prerequisites

- Win32Emu.Browser deployed to GitHub Pages or running locally
- Playwright installed and configured

## Basic Tests

### 1. Check Page Load

```javascript
// Navigate to the page
await page.goto('https://archanox.github.io/Win32Emu/');

// Verify page title
const title = await page.title();
expect(title).toBe('Win32Emu - Windows 32-bit Emulator');

// Check for main heading
const heading = await page.locator('h1').textContent();
expect(heading).toContain('Win32Emu Web Demo');
```

### 2. Verify WebGPU Support Detection

```javascript
// Navigate to the page
await page.goto('https://archanox.github.io/Win32Emu/');

// Wait for WebGPU detection to complete
await page.waitForTimeout(2000);

// Check WebGPU badge
const webgpuBadge = await page.locator('dt:has-text("WebGPU:") + dd .badge').textContent();
console.log('WebGPU Support:', webgpuBadge);

// WebGPU should either be "Supported" or "Not Supported"
expect(['Supported', 'Not Supported']).toContain(webgpuBadge);
```

### 3. Test Initialize Button

```javascript
// Navigate and wait for load
await page.goto('https://archanox.github.io/Win32Emu/');
await page.waitForTimeout(2000);

// Click Initialize button
await page.click('button:has-text("Initialize")');

// Wait for initialization
await page.waitForTimeout(1000);

// Check status
const status = await page.locator('dt:has-text("Status:") + dd .badge').textContent();
expect(['Initialized', 'WebGPU not supported']).toContain(status);
```

### 4. Verify Console Output

```javascript
// Navigate to the page
await page.goto('https://archanox.github.io/Win32Emu/');
await page.waitForTimeout(2000);

// Click Initialize
await page.click('button:has-text("Initialize")');
await page.waitForTimeout(1000);

// Check console output
const consoleText = await page.locator('#console-output').textContent();
expect(consoleText).toContain('Win32Emu web interface loaded');
```

### 5. Test Rendering Test

```javascript
// Navigate and initialize
await page.goto('https://archanox.github.io/Win32Emu/');
await page.waitForTimeout(2000);
await page.click('button:has-text("Initialize")');
await page.waitForTimeout(1000);

// Click Test Rendering button
await page.click('button:has-text("Test Rendering")');
await page.waitForTimeout(1000);

// Verify console output
const consoleText = await page.locator('#console-output').textContent();
expect(consoleText).toContain('Rendering test completed successfully');
```

### 6. Mobile Viewport Test

```javascript
// Set mobile viewport
await page.setViewportSize({ width: 375, height: 667 });

// Navigate to the page
await page.goto('https://archanox.github.io/Win32Emu/');
await page.waitForTimeout(2000);

// Verify platform detection
const platform = await page.locator('dt:has-text("Platform:") + dd').textContent();
console.log('Detected Platform:', platform);

// Check that UI is responsive
const canvas = await page.locator('#emulator-canvas');
expect(await canvas.isVisible()).toBe(true);
```

### 7. Canvas Element Test

```javascript
// Navigate to the page
await page.goto('https://archanox.github.io/Win32Emu/');

// Check for canvas element
const canvas = await page.locator('#emulator-canvas');
expect(await canvas.isVisible()).toBe(true);

// Verify canvas dimensions
const canvasWidth = await canvas.getAttribute('width');
const canvasHeight = await canvas.getAttribute('height');
expect(canvasWidth).toBe('640');
expect(canvasHeight).toBe('480');
```

## Automated Test Suite Example

```javascript
const { test, expect } = require('@playwright/test');

test.describe('Win32Emu Browser Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('https://archanox.github.io/Win32Emu/');
    await page.waitForTimeout(2000);
  });

  test('page loads correctly', async ({ page }) => {
    const title = await page.title();
    expect(title).toBe('Win32Emu - Windows 32-bit Emulator');
  });

  test('webgpu detection works', async ({ page }) => {
    const webgpuBadge = await page.locator('dt:has-text("WebGPU:") + dd .badge').textContent();
    expect(['Supported', 'Not Supported']).toContain(webgpuBadge);
  });

  test('initialize button works', async ({ page }) => {
    await page.click('button:has-text("Initialize")');
    await page.waitForTimeout(1000);
    
    const status = await page.locator('dt:has-text("Status:") + dd .badge').textContent();
    expect(['Initialized', 'WebGPU not supported']).toContain(status);
  });

  test('console output appears', async ({ page }) => {
    const consoleText = await page.locator('#console-output').textContent();
    expect(consoleText).toContain('Win32Emu web interface loaded');
  });

  test('canvas is present', async ({ page }) => {
    const canvas = await page.locator('#emulator-canvas');
    expect(await canvas.isVisible()).toBe(true);
  });
});
```

## Running Tests

### With Playwright CLI

```bash
npx playwright test
```

### With Playwright Codegen (Recording)

```bash
npx playwright codegen https://archanox.github.io/Win32Emu/
```

### Mobile Testing

```bash
npx playwright test --device="iPhone 12"
npx playwright test --device="Pixel 5"
npx playwright test --device="iPad Pro"
```

## Notes

- The page uses WebGPU which may not be available in all browsers/environments
- Some tests may need adjustment based on WebGPU availability
- Mobile tests verify responsive design and platform detection
- Console output provides detailed test feedback
