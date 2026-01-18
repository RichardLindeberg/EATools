import { test, expect } from '@playwright/test';

test.describe('Console Messages Check', () => {
  const consoleMessages: { type: string; text: string; url: string }[] = [];
  const pageErrors: { message: string; url: string }[] = [];
  const failedRequests: { url: string; error: string }[] = [];

  test.beforeEach(async ({ page }) => {
    // Clear arrays
    consoleMessages.length = 0;
    pageErrors.length = 0;
    failedRequests.length = 0;

    // Capture console messages
    page.on('console', (msg) => {
      const type = msg.type();
      if (['error', 'warning'].includes(type)) {
        consoleMessages.push({
          type,
          text: msg.text(),
          url: page.url(),
        });
      }
    });

    // Capture page errors
    page.on('pageerror', (error) => {
      pageErrors.push({
        message: error.message,
        url: page.url(),
      });
    });

    // Capture failed requests
    page.on('requestfailed', (request) => {
      failedRequests.push({
        url: request.url(),
        error: request.failure()?.errorText || 'Unknown error',
      });
    });
  });

  test.afterEach(async () => {
    // Report all captured issues
    if (consoleMessages.length > 0) {
      console.log('\n=== Console Errors/Warnings ===');
      consoleMessages.forEach((msg) => {
        console.log(`[${msg.type.toUpperCase()}] ${msg.url}`);
        console.log(`  ${msg.text}`);
      });
    }

    if (pageErrors.length > 0) {
      console.log('\n=== Page Errors ===');
      pageErrors.forEach((error) => {
        console.log(`[ERROR] ${error.url}`);
        console.log(`  ${error.message}`);
      });
    }

    if (failedRequests.length > 0) {
      console.log('\n=== Failed Requests ===');
      failedRequests.forEach((req) => {
        console.log(`[FAILED] ${req.url}`);
        console.log(`  ${req.error}`);
      });
    }
  });

  test('homepage - check for console issues', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'test-results/console-homepage.png', fullPage: true });

    // Check for critical errors
    expect(pageErrors.length, 'Should have no page errors').toBe(0);
  });

  test('applications page - check for console issues', async ({ page }) => {
    await page.goto('/entities/applications');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'test-results/console-applications.png', fullPage: true });

    expect(pageErrors.length, 'Should have no page errors').toBe(0);
  });

  test('all entity pages - check for console issues', async ({ page }) => {
    const pages = [
      '/entities/applications',
      '/entities/servers',
      '/entities/integrations',
      '/entities/data-entities',
      '/entities/business-capabilities',
      '/entities/organizations',
      '/entities/relations',
      '/entities/application-services',
      '/entities/application-interfaces',
    ];

    for (const path of pages) {
      const pageName = path.split('/').pop() || 'unknown';
      console.log(`\n=== Testing ${pageName} ===`);
      
      await page.goto(path);
      await page.waitForLoadState('networkidle');
      await page.screenshot({ 
        path: `test-results/console-${pageName}.png`,
        fullPage: true 
      });

      // Don't fail immediately, collect all issues
      if (pageErrors.length > 0 || consoleMessages.filter(m => m.type === 'error').length > 0) {
        console.log(`Issues found on ${pageName}`);
      }
    }
  });
});
