// Drives an Enyo 1.0 page through a sequence of steps and screenshots it along the way. See shoot-sequence.sh.
const path = require('path');

function loadPlaywright() {
    const tries = [process.env.PLAYWRIGHT_CORE, 'playwright-core', 'playwright'].filter(Boolean);
    for (const t of tries) {
        try {
            return require(t);
        } catch {
        }
    }
    throw new Error('playwright-core not found; set PLAYWRIGHT_CORE to its folder');
}

const [cdp, url, out, ...rest] = process.argv.slice(2);
let probe = null;
const steps = [];
for (let i = 0; i < rest.length; i++) {
    if (rest[i] === '--probe') {
        probe = rest[++i];
    } else {
        steps.push(rest[i]);
    }
}

const point = (s) => s.split(',').map(Number);

(async () => {
    const { chromium } = loadPlaywright();
    const browser = await chromium.connectOverCDP(cdp);
    const page = await browser.contexts()[0].newPage();
    await page.setViewportSize({ width: 1024, height: 600 });
    page.on('pageerror', (e) => console.error('page error:', e.message));
    await page.goto(url);
    await page.waitForTimeout(1500);

    let n = 0;
    const shot = async (label) => {
        const file = path.join(out, `${String(++n).padStart(2, '0')}-${label}.png`);
        await page.screenshot({ path: file });
        const state = probe ? await page.evaluate(probe) : '';
        console.log(file + (state ? ` :: ${state}` : ''));
    };
    const moveTo = async (x, y, count) => {
        await page.mouse.move(x, y, { steps: count || 10 });
    };

    for (const step of steps) {
        const [cmd, ...a] = step.split(/\s+/);
        switch (cmd) {
            case 'shot': await shot(a[0] || 'shot'); break;
            case 'down': { const [x, y] = point(a[0]); await page.mouse.move(x, y); await page.mouse.down(); break; }
            case 'move': { const [x, y] = point(a[0]); await moveTo(x, y, Number(a[1])); break; }
            case 'up': { const [x, y] = point(a[0]); await page.mouse.move(x, y); await page.mouse.up(); break; }
            case 'drag': {
                const [x0, y0] = point(a[0]);
                const [x1, y1] = point(a[1]);
                await page.mouse.move(x0, y0);
                await page.mouse.down();
                await moveTo(x1, y1, Number(a[2]) || 20);
                break;
            }
            case 'click': { const [x, y] = point(a[0]); await page.mouse.click(x, y); break; }
            case 'wait': await page.waitForTimeout(Number(a[0])); break;
            case 'every': {
                const [ms, count, label] = [Number(a[0]), Number(a[1]), a[2] || 'frame'];
                for (let i = 1; i <= count; i++) {
                    await page.waitForTimeout(ms);
                    await shot(`${label}-${i * ms}ms`);
                }
                break;
            }
            case 'eval': await page.evaluate(a.join(' ')); break;
            default: throw new Error(`unknown step: ${step}`);
        }
    }

    await page.close();
    process.exit(0);
})().catch((e) => {
    console.error(e);
    process.exit(1);
});
