const { spawnSync, spawn } = require("child_process");
const fs = require("fs");

const isWin = process.platform === "win32";
const log = fs.openSync("gitnexus-bootstrap.log", "a");

function run(args) {
  const r = spawnSync("npx", ["gitnexus", ...args], {
    stdio: ["ignore", log, log],
    shell: isWin,
  });
  if (r.error) process.exit(1);
  if (r.status !== 0) process.exit(r.status ?? 1);
}

run(["clean", "--force"]);
run(["analyze"]);

const child = spawn("npx", ["gitnexus", "mcp"], {
  stdio: "inherit",
  shell: isWin,
});

child.on("exit", code => process.exit(code ?? 0));