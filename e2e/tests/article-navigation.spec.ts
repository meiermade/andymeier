import { expect, test, type Page } from '@playwright/test'

const siteBaseUrl = process.env.SITE_E2E_BASE_URL ?? (process.env.E2E_SCOPE === 'deployed' ? 'https://andymeier.dev' : 'http://127.0.0.1:5051')

test.beforeEach(async ({ page }) => {
  await page.context().addCookies([{
    name: 'analytics-consent',
    value: 'v1.declined.2026-08-16.0',
    url: siteBaseUrl,
    sameSite: 'Lax',
  }])
})

async function expectHeadingBelowHeader(page: Page, id: string) {
  await expect.poll(() => page.locator(`#${id}`).evaluate(element => {
    const top = element.getBoundingClientRect().top
    const headerBottom = document.getElementById('top-nav')!.getBoundingClientRect().bottom
    return top >= headerBottom && top < headerBottom + 100
  })).toBe(true)
}

for (const [slug, count] of [['personal-infrastructure', 10], ['dev-env', 11], ['fsharp-semantic-kernel', 3]] as const) {
  test(`${slug} renders matching section navigation from the article headings`, async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 1000 })
    await page.goto(`/articles/${slug}`)
    const nav = page.getByRole('navigation', { name: 'On this page', exact: true })
    await expect(nav).toBeVisible()
    await expect(nav.getByRole('link')).toHaveCount(count)
    await expect(page.getByRole('heading', { name: 'Contents', exact: true })).toHaveCount(0)
    const headings = await page.locator('[data-article-section]').evaluateAll(elements =>
      elements.map(element => ({ label: element.textContent, href: `#${element.id}` })))
    const links = await nav.getByRole('link').evaluateAll(elements =>
      elements.map(element => ({ label: element.textContent, href: element.getAttribute('href') })))
    expect(links).toEqual(headings)
    expect(new Set(headings.map(heading => heading.href)).size).toBe(count)
    expect(await page.locator('article').evaluate(el => el.clientWidth)).toBeGreaterThanOrEqual(992)
  })
}

test('section links, scroll tracking, and history work after enhanced navigation', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1000 })
  const errors: string[] = []
  const requests: string[] = []
  page.on('pageerror', error => errors.push(error.message))
  page.on('request', request => {
    if (request.headers()['datastar-request'] === 'true') requests.push(new URL(request.url()).pathname)
  })
  await page.goto('/articles')
  await page.evaluate(() => document.documentElement.dataset.testDocument = 'preserved')
  await page.getByRole('link', { name: 'Personal Infrastructure', exact: true }).click()
  await expect(page.locator('[data-article-page] svg[id^="mermaid-"]')).toHaveCount(5)
  const nav = page.getByRole('navigation', { name: 'On this page', exact: true })
  const cloudflare = nav.getByRole('link', { name: 'Cloudflare for networking and access', exact: true })
  const observability = nav.getByRole('link', { name: 'OpenTelemetry and ClickStack', exact: true })
  const requestCount = requests.length
  await cloudflare.click()
  await expectHeadingBelowHeader(page, 'cloudflare')
  await expect(cloudflare).toHaveAttribute('aria-current', 'location')
  await expect(page.locator('#cloudflare')).toBeFocused()
  await observability.click()
  await expectHeadingBelowHeader(page, 'observability')
  await page.goBack()
  await expect(page).toHaveURL(/#cloudflare$/)
  await expectHeadingBelowHeader(page, 'cloudflare')
  await expect(cloudflare).toHaveAttribute('aria-current', 'location')
  await page.goForward()
  await expectHeadingBelowHeader(page, 'observability')
  await expect(observability).toHaveAttribute('aria-current', 'location')
  expect(requests).toHaveLength(requestCount)

  await page.locator('#organization').evaluate(el => el.scrollIntoView())
  await expect(nav.getByRole('link', { name: 'How it is organized' })).toHaveAttribute('aria-current', 'location')
  await expect(page).toHaveURL(/#observability$/) // Reading does not create fragment history entries.
  await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight))
  await expect(nav.getByRole('link', { name: 'Intentional tradeoffs' })).toHaveAttribute('aria-current', 'location')
  await expect(page.getByRole('progressbar', { name: 'Article reading progress' })).toHaveAttribute('aria-valuenow', '100')

  await page.locator('#nav-articles').click()
  await page.getByRole('link', { name: 'Development Environment', exact: true }).click()
  await page.getByRole('navigation', { name: 'On this page', exact: true }).getByRole('link', { name: 'Pi coding agent', exact: true }).click()
  await expectHeadingBelowHeader(page, 'pi')
  await expect(page.locator('[data-article-toc] a[aria-current="location"]')).toHaveCount(2)
  await page.goBack() // The unfragmented Development Environment entry.
  await expect(page).toHaveURL('/articles/dev-env')
  await page.goBack() // The article index.
  await expect(page.getByRole('heading', { name: 'Articles', exact: true })).toBeVisible()
  await page.goBack() // Restore the previous article at its last reading position.
  await expect(page).toHaveURL(/personal-infrastructure#observability$/)
  await expect(page.locator('[data-article-page] svg[id^="mermaid-"]')).toHaveCount(5)
  await expect(page.getByRole('navigation', { name: 'On this page', exact: true }).getByRole('link', { name: 'Intentional tradeoffs' })).toHaveAttribute('aria-current', 'location')
  await expect(page.locator('html')).toHaveAttribute('data-test-document', 'preserved')
  expect(errors).toEqual([])
})

test('mobile disclosure supports keyboard navigation and closes after selecting a section', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/articles/dev-env')
  const disclosure = page.locator('[data-article-mobile-toc]')
  const summary = disclosure.locator('summary')
  await expect(disclosure).not.toHaveAttribute('open', '')
  await summary.focus()
  await page.keyboard.press('Enter')
  await expect(disclosure).toHaveAttribute('open', '')
  const link = disclosure.getByRole('link', { name: 'Terminal environment', exact: true })
  await link.focus()
  await page.keyboard.press('Enter')
  await expect(page).toHaveURL(/#terminal$/)
  await expect(disclosure).not.toHaveAttribute('open', '')
  await expectHeadingBelowHeader(page, 'terminal')
  await expect(page.locator('#terminal')).toBeFocused()
  await page.goBack()
  await expect(page).toHaveURL('/articles/dev-env')
})

test('direct section URLs survive diagram layout and refresh', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1000 })
  await page.goto('/articles/personal-infrastructure#cloudflare')
  await expect(page.locator('[data-article-page] svg[id^="mermaid-"]')).toHaveCount(5)
  await expectHeadingBelowHeader(page, 'cloudflare')
  await page.reload()
  await expect(page.locator('[data-article-page] svg[id^="mermaid-"]')).toHaveCount(5)
  await expectHeadingBelowHeader(page, 'cloudflare')
})

test('section links work without JavaScript', async ({ browser }) => {
  const context = await browser.newContext({ javaScriptEnabled: false, viewport: { width: 390, height: 844 } })
  const page = await context.newPage()
  await page.goto(`${siteBaseUrl}/articles/dev-env`)
  await page.locator('[data-article-mobile-toc] summary').click()
  await page.getByRole('link', { name: 'Language runtimes', exact: true }).click()
  await expect(page).toHaveURL(/#runtimes$/)
  await expectHeadingBelowHeader(page, 'runtimes')
  await context.close()
})

test('narrow and zoom-equivalent layouts keep navigation and code within the page', async ({ page }) => {
  for (const width of [320, 720, 1280]) {
    await page.setViewportSize({ width, height: 900 })
    await page.goto('/articles/personal-infrastructure')
    await expect(page.locator('[data-article-mobile-toc]')).toBeVisible()
    await expect(page.locator('[data-article-page] aside')).toBeHidden()
    await expect(page.locator('[data-article-page] svg[id^="mermaid-"]')).toHaveCount(5)
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
  }
})
