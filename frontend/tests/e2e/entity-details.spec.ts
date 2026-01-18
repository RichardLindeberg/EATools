/**
 * Entity Detail Pages E2E Tests
 * Tests all 9 entity detail pages
 */

import { test, expect } from '@playwright/test';

test.describe('Entity Detail Pages', () => {
  test.beforeEach(async ({ page }) => {
    // Capture console messages
    page.on('console', (msg) => {
      const type = msg.type();
      if (type === 'error') {
        console.log(`[CONSOLE ERROR] ${msg.text()}`);
      }
    });
  });

  test('Application detail page loads and displays data', async ({ page }) => {
    // Navigate to applications list to get the first application
    await page.goto('http://localhost:3000/entities/applications');
    await page.waitForLoadState('networkidle');

    // Click on the first application in the list (if available)
    const firstRow = page.locator('table tbody tr').first();
    if (await firstRow.isVisible()) {
      await firstRow.click();
      
      // Wait for detail page to load
      await page.waitForLoadState('networkidle');
      
      // Check that we're on a detail page
      await expect(page).toHaveURL(/\/entities\/applications\/[a-zA-Z0-9-]+$/);
      
      // Check for EntityHeader elements
      await expect(page.locator('.entity-header')).toBeVisible();
      await expect(page.locator('.entity-breadcrumbs')).toBeVisible();
      await expect(page.locator('.entity-title')).toBeVisible();
      
      // Check for tabs
      await expect(page.locator('.entity-tabs')).toBeVisible();
      await expect(page.locator('.tab-button')).toHaveCount(3); // Overview, Relationships, Audit
      
      // Check that Overview tab is active by default
      await expect(page.locator('.tab-button.active')).toContainText('Overview');
      
      // Click on Relationships tab
      await page.locator('.tab-button:has-text("Relationships")').click();
      await expect(page.locator('.tab-button.active')).toContainText('Relationships');
      
      // Click on Audit tab
      await page.locator('.tab-button:has-text("Audit")').click();
      await expect(page.locator('.tab-button.active')).toContainText('Audit');
      
      // Check for action buttons
      await expect(page.locator('.entity-action:has-text("Edit")')).toBeVisible();
      await expect(page.locator('.entity-action:has-text("Delete")')).toBeVisible();
      await expect(page.locator('.entity-action:has-text("Back to List")')).toBeVisible();
      
      console.log('✅ Application detail page test passed');
    } else {
      console.log('⚠️ No applications found to test detail page');
    }
  });

  test('Server detail page is accessible', async ({ page }) => {
    await page.goto('http://localhost:3000/entities/servers');
    await page.waitForLoadState('networkidle');
    
    // If there are servers, click the first one
    const firstRow = page.locator('table tbody tr').first();
    if (await firstRow.isVisible()) {
      await firstRow.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/\/entities\/servers\/[a-zA-Z0-9-]+$/);
      await expect(page.locator('.entity-header')).toBeVisible();
      console.log('✅ Server detail page accessible');
    } else {
      console.log('⚠️ No servers found');
    }
  });

  test('Integration detail page is accessible', async ({ page }) => {
    await page.goto('http://localhost:3000/entities/integrations');
    await page.waitForLoadState('networkidle');
    
    const firstRow = page.locator('table tbody tr').first();
    if (await firstRow.isVisible()) {
      await firstRow.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/\/entities\/integrations\/[a-zA-Z0-9-]+$/);
      await expect(page.locator('.entity-header')).toBeVisible();
      console.log('✅ Integration detail page accessible');
    } else {
      console.log('⚠️ No integrations found');
    }
  });

  test('Direct navigation to Application detail page', async ({ page }) => {
    // Try to navigate to a known ID (if it exists) or handle 404
    await page.goto('http://localhost:3000/entities/applications/test-app-1');
    await page.waitForLoadState('networkidle');
    
    // Check for either the detail page or a not found message
    const isDetailPage = await page.locator('.entity-header').isVisible();
    const isErrorPage = await page.locator('.entity-detail-error').isVisible();
    
    expect(isDetailPage || isErrorPage).toBe(true);
    
    if (isDetailPage) {
      console.log('✅ Direct navigation shows detail page');
      await expect(page.locator('.entity-tabs')).toBeVisible();
    } else if (isErrorPage) {
      console.log('✅ Direct navigation shows appropriate error page');
    }
  });

  test('All entity types have detail page routes configured', async ({ page }) => {
    const entityTypes = [
      'applications',
      'servers',
      'integrations',
      'data-entities',
      'business-capabilities',
      'organizations',
      'relations',
      'application-services',
      'application-interfaces',
    ];

    for (const entityType of entityTypes) {
      await page.goto(`http://localhost:3000/entities/${entityType}/test-id-123`);
      await page.waitForLoadState('networkidle');
      
      // Check for either the detail page elements or error page (both are valid)
      const hasDetailElements = await page.locator('.entity-header').isVisible() || 
                                 await page.locator('.entity-detail-loading').isVisible() ||
                                 await page.locator('.entity-detail-error').isVisible();
      
      expect(hasDetailElements).toBe(true);
      console.log(`✅ ${entityType} detail route configured`);
    }
  });
});
