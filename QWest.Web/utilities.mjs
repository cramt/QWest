import * as childProcess from "child_process"

export function shellExec(cmd) {
    return new Promise((resolve, reject) => {
        childProcess.exec(cmd, (error, stdout, stderr) => {
            if (error) {
                reject(error)
            }
            else {
                resolve(stdout ? stdout : stderr)
            }
        })
    })
}

const killProcessOnPortNetstatRegex = /\s+\S+\s+\S+\s+\S+\s+\S+\s+(\S+)/

export async function killProcessOnPort(port) {
    let netstatResult = null;
    try {
        netstatResult = await shellExec('netstat -ano | find "LISTENING" | find "8080"')
    }
    catch (e) {
        return;
    }
    let killing = netstatResult.split("\n")
        .filter(x => x !== "")
        .map(x => x.match(killProcessOnPortNetstatRegex)[1])
        .filter(x => !isNaN(parseInt(x)))
        .filter((v, i, a) => a.indexOf(v) === i)
        .map(x => shellExec("taskkill /F /pid " + x))
    await Promise.all(killing);
}