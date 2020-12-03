import $ from "jquery";
import { fetchUser } from "../whoami";
import { mapOutStaticData } from "../mapOutStaticData"
import { mapOutVisitation } from "../mapOutVisitation";
import Cookies from 'js-cookie'

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
        let request = await fetch("/api/Geography/Get", {
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
    let subdivisions = Object.entries(subdividable.subdivisions).filter(x => typeof x[0] === "string").map(x => x[1]);
    if (subdividable.visited) {
        return 1;
    }
    if (subdivisions.length == 0) {
        return 0;
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
    let staticDataMap = mapOutStaticData(staticData)
    let logoutButton = $("#logout-button")

    mapOutVisitation(userData.progressMap.locations, staticDataMap)
    renderMap(staticDataMap)

    let customMenu = $(".custom-menu")
    $(".svgMap-country").on("contextmenu", e => {
        e.preventDefault();
        let alpha2 = e.currentTarget.getAttribute("data-id")
        customMenu.empty()
        const edit = $("<li>Edit " + staticDataMap[alpha2].name + "</li>")
        customMenu.append(edit)
        const chechEntry = $("<li></li>")
        const chech = $("<input type=\"checkbox\"/>")
        const country = staticDataMap[alpha2]
        const originalPossition = country.visited
        if (originalPossition) {
            chech.prop("checked", true)
        }
        chech.on("click", async () => {
            let additions = []
            let subtractions = []
            let value = !originalPossition
            if (value) {
                additions.push(country.id)
            }
            else {
                subtractions.push(country.id)
            }
            let request = await fetch("/api/ProgressMap/Change", {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    id: userData.progressMap.id,
                    additions,
                    subtractions
                })
            });
            if (request.status === 200) {
                window.location.reload()
                return
            }
            console.log(await request.text())
            alert("error " + request.status)
        })
        chechEntry.append(chech)
        customMenu.append(chechEntry)
        edit.on("click", () => {
            window.location.href = "/edit_map.html?alpha_2=" + alpha2
        })
        customMenu.finish().fadeIn(500).css({
            top: e.pageY + "px",
            left: e.pageX + "px"
        })

    })

    logoutButton.on("click", () => {
        Cookies.remove("sessionCookie")
        window.location.href = "/login.html"
    })
})