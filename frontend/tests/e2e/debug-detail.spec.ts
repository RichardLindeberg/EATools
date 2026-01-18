/**
 * Simple test to check what's rendering on detail pages
 */

import { test } from '@playwright/test';

test('Check what renders on detail page', async ({ page }) => {
  page.on('console', (msg) => {
    console.log(`[CONSOLE ${msg.type()}]`, msg.text());
  });

  page.on('pageerror', (error) => {
    console.log(`[PAGE ERROR]`, error.message);
  });

  // Test with real ID
  await page.goto('http://localhost:3000/entities/applications/app-c0b2fd2d');
  await page.waitForTimeout(2000); // Wait for rendering

  const html = await page.content();
  console.log('Page title:', await page.title());
  console.log('URL:', page.url());
  
  // Check for various elements
  const elements = {
    'root': await page.locator('#root').isVisible(),
    'entity-detail': await page.locator('.entity-detail').isVisible(),
    'entity-header': await page.locator('.entity-header').isVisible(),
    'entity-detail-loading': await page.locator('.entity-detail-loading').isVisible(),
    'entity-detail-error': await page.locator('.entity-detail-error').isVisible(),
    'entity-tabs': await page.locator('.entity-tabs').isVisible(),
  };

  console.log('Elements found:', JSON.stringify(elements, null, 2));

  // Log first 500 chars of body content
  const bodyText = await page.locator('body').innerText();
  console.log('Body text (first 500 chars):', bodyText.substring(0, 500));
  
  // Take a screenshot
  await page.screenshot({ path: 'test-results/debug-detail-page.png', fullPage: true });
  console.log('Screenshot saved to test-results/debug-detail-page.png');
});
