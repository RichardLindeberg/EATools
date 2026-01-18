import { test, expect } from '@playwright/test';

// Capture console messages for debugging
test.beforeEach(async ({ page }) => {
  page.on('console', (msg) => {
    const type = msg.type();
    const text = msg.text();
    
    // Log all console messages with appropriate prefix
    if (type === 'error') {
      console.error(`[BROWSER ERROR] ${text}`);
    } else if (type === 'warning') {
      console.warn(`[BROWSER WARNING] ${text}`);
    } else if (type === 'log') {
      console.log(`[BROWSER LOG] ${text}`);
    }
  });
  
  // Capture page errors
  page.on('pageerror', (error) => {
    console.error(`[PAGE ERROR] ${error.message}`);
  });
  
  // Capture failed requests
  page.on('requestfailed', (request) => {
    console.error(`[REQUEST FAILED] ${request.url()}: ${request.failure()?.errorText}`);
  });
});

test('homepage loads', async ({ page }) => {
  await page.goto('/');
  
  // Take screenshot
  await page.screenshot({ path: 'test-results/homepage.png', fullPage: true });
  
  // Check if the page has content
  const body = page.locator('body');
  await expect(body).not.toBeEmpty();
  
  console.log('Page title:', await page.title());
});

test('applications page loads', async ({ page }) => {
  await page.goto('/entities/applications');
  
  // Take screenshot
  await page.screenshot({ path: 'test-results/applications.png', fullPage: true });
  
  // Wait for content to load
  await page.waitForLoadState('networkidle');
  
  console.log('Page URL:', page.url());
});

test('navigation works', async ({ page }) => {
  await page.goto('/');
  
  // Try clicking on different navigation items
  await page.click('text=Applications');
  await expect(page).toHaveURL(/.*applications/);
  await page.screenshot({ path: 'test-results/nav-applications.png' });
  
  console.log('Navigated to:', page.url());
});
