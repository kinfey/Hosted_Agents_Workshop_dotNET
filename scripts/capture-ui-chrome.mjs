import { chromium } from "playwright";
import path from "node:path";
import fs from "node:fs/promises";

const appUrl = process.env.APP_URL ?? "http://localhost:5075";
const outputDir = process.env.SCREENSHOT_DIR ?? "labs/lab-5-ui/images";
const width = Number.parseInt(process.env.SCREENSHOT_WIDTH ?? "1920", 10);
const height = Number.parseInt(process.env.SCREENSHOT_HEIGHT ?? "1080", 10);
const promptText =
  process.env.SCREENSHOT_PROMPT ??
  "Create a launch checklist for an agent named triage-coach in the pilot environment.";

await fs.mkdir(outputDir, { recursive: true });

const browser = await chromium.launch({
  channel: "chrome",
  headless: false,
  args: [`--window-size=${width},${height}`, "--start-maximized"]
});

const context = await browser.newContext({ viewport: { width, height } });
const page = await context.newPage();

try {
  await page.goto(appUrl, { waitUntil: "networkidle" });

  const shot1 = path.join(outputDir, "01-chat-ui-landing.png");
  await page.screenshot({ path: shot1, fullPage: false });

  const promptBox = page.locator("textarea");
  await promptBox.fill(promptText);

  const shot2 = path.join(outputDir, "02-chat-ui-prompt-entered.png");
  await page.screenshot({ path: shot2, fullPage: false });

  await page.getByRole("button", { name: "Send Prompt" }).click();

  await page.waitForFunction(
    (expected) => {
      const body = document.body.innerText || "";
      return body.includes(expected) && !body.includes("Sending...");
    },
    "ASSISTANT",
    { timeout: 120000 }
  );

  const shot3 = path.join(outputDir, "03-chat-ui-response-hd.png");
  await page.screenshot({ path: shot3, fullPage: false });

  console.log(`Saved: ${shot1}`);
  console.log(`Saved: ${shot2}`);
  console.log(`Saved: ${shot3}`);
}
finally {
  await context.close();
  await browser.close();
}
