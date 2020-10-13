import $ from "jquery";
import { fetchUser } from "../whoami";
import { mapOutStaticData } from "../mapOutStaticData"
import { mapOutVisitation } from "../mapOutVisitation";

async function fetchData() {
    let userDataPromise = (async () => {
        let user = await fetchUser()
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

const getVisitationPercentage = (subdividable) => {
    let subdivisions = Object.values(subdividable.subdivision);
    if (subdivisions.length == 0) {
        return subdividable.visited ? 1 : 0
    }
    return subdivisions.map(getVisitationPercentage).reduce((a, b) => a + b, 0) / subdivisions.length
}

const renderMap = (staticData) => {
    let mapData = Object.values(staticData).reduce((acc, x) => {
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
    return map
}

$(async () => {
    let { userData, staticData } = await fetchData();
    userData.progressMap.locations = userData.progressMap.locations.map(x => x.split("-"))
    let staticDataMap = mapOutStaticData(staticData)
    mapOutVisitation(userData.progressMap.locations, staticDataMap)

    renderMap(staticDataMap)

    let customMenu = $(".custom-menu")
    $(".svgMap-country").on("contextmenu", e => {
        e.preventDefault();
        let alpha2 = e.currentTarget.getAttribute("data-id")
        customMenu.empty()
        const edit = $("<li>Edit " + staticDataMap[alpha2].name + "</li>")
        customMenu.append(edit)
        edit.on("click", () => {
            window.location.href = "/edit_map.html?alpha_2=" + alpha2
        })
        customMenu.finish().toggle(100).css({
            top: e.pageY + "px",
            left: e.pageX + "px"
        })
    })
})