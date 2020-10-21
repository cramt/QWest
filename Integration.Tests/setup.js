const path = require('path');
const fs = require('fs');
const os = require('os');
const mkdirp = require('mkdirp');
const puppeteer = require('puppeteer');
const childProcess = require("child_process")

const DIR = path.join(os.tmpdir(), 'jest_puppeteer_global_setup');

module.exports = async function () {

    const [browser, projectProcess] = await Promise.all([puppeteer.launch(), initializeProject()])
    // store the browser instance so we can teardown it later
    // this global is only available in the teardown but not in TestEnvironments
    global.__BROWSER_GLOBAL__ = browser;
    global.__PROJECT_PROCESS__ = projectProcess;

    // use the file system to expose the wsEndpoint for TestEnvironments
    mkdirp.sync(DIR);
    fs.writeFileSync(path.join(DIR, 'wsEndpoint'), browser.wsEndpoint());
};

async function initializeProject() {
    const isRunning = await new Promise((resolve, reject) => {
        childProcess.exec("tasklist", (err, stdout, stderr) => {
            resolve(stdout.toString().split("\n").filter(x => x.startsWith("QWest.Services.Run.exe")).length === 1)
        })
    })
    if (isRunning) {
        return null;
    }
    else {
        return await runProject();
    }
}

function runProject() {
    return new Promise(async (resolve, reject) => {
        await buildProject();
        const dir = path.resolve(__dirname, "../QWest.Services.Run/bin/Debug")
        const process = childProcess.execFile(path.resolve(dir, "QWest.Services.Run.exe"), {
            cwd: dir
        })
        setTimeout(() => resolve(process), 100);
    })
}

function buildProject() {
    return new Promise((resolve, reject) => {
        let a = `
        $msbuild = $env:MSBuild
        if($msbuild -eq $null){(Resolve-Path ([io.path]::combine(\${env:ProgramFiles(x86)}, 'Microsoft Visual Studio', '*', '*', 'MSBuild', '*' , 'bin' , 'msbuild.exe'))).Path}
        $collectionOfArgs = @("${path.resolve(__dirname, "../QWest.sln")}", "/target:Clean", "/target:Build")
        & $msbuild $collectionOfArgs
               `;
        console.log(a)
        childProcess.exec(a, {
            shell: "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
            cwd: path.resolve(__dirname, "../")
        }, (err, stdout, stderr) => {
            if (err) {
                reject(err)
            }
            else {
                resolve()
            }
        })
    })
}