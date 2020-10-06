import $ from "jquery";
import { userPromise } from "../whoami";

async function fetchData() {
    let userDataPromise = (async () => {
        let user = await userPromise
        let progressMapRequest = await fetch("/api/ProgressMap/UserId?id=" + user.id, {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });
        let progressMap = JSON.parse(await progressMapRequest.text())
        return { user, progressMap }
    })()
    let staticDataPromise = (async () => {
        let request = await fetch("/api/Subdivision/Get", {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });
        return JSON.parse(await request.text())
    })()
    let promiseAllResult = await Promise.all([userDataPromise, staticDataPromise])
    let userData = promiseAllResult[0]
    let staticData = promiseAllResult[1]
    return { userData, staticData }
}

function getVisitationPercentage(subdividable) {
    let subdivisions = subdividable.subdivision;
    if (subdivisions.length == 0) {
        return subdividable.visited ? 1 : 0
    }
    return subdivisions.map(getVisitationPercentage).reduce((a, b) => a + b, 0) / subdivisions.length
}

$(async () => {
    let { userData, staticData } = await fetchData();
    userData.progressMap.locations = userData.progressMap.locations.map(x => x.split("-"))
    staticData = staticData.reduce((acc, x) => {
        acc[x.alpha_2] = x
        return acc
    }, {})
    userData.progressMap.locations.forEach(location => {
        let country = staticData[location[0]]
        if (location.length == 1) {
            country.visited = true
        }
        else {
            let curr = country;
            for (let i = 1; i < location.length; i++) {
                curr = curr.subdivision.find(x => x.code == location[i])
            }
            curr.visited = true
        }
    })
    staticData = Object.values(staticData)
    let mapData = staticData.reduce((acc, x) => {
        let visited = getVisitationPercentage(x)
        acc[x.alpha_2] = {
            visited: visited * 100
        }
        return acc
    }, {})
    let map = new svgMap({
        targetElementID: 'svgMap',
        data: {
            data: {
                visited: {
                    name: 'percent visited',
                    format: '{0}%',
                    thresholdMax: 100,
                    thresholdMin: 0
                }
            },
            applyData: 'visited',
            values: mapData
        }
    });
})