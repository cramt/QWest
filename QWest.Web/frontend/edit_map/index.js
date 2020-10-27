import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { mapOutStaticData } from "../mapOutStaticData";
import { mapOutVisitation } from "../mapOutVisitation";

const url = new URL(window.location.href);
const alpha2 = url.searchParams.get("alpha_2")
if (alpha2 === null) {
    window.location.href = "/map.html"
}

const progressMapPromise = (async () => {
    let progressMapRequest = await fetch("/api/ProgressMap/UserId", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    if (progressMapRequest.status === 401) {
        window.location.href = "/login.html"
    }
    let progressMap = JSON.parse(await progressMapRequest.text())
    return progressMap
})()
const staticDataPromise = (async () => {
    let request = await fetch("/api/Geography/Get?alpha2=" + alpha2, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    return JSON.parse(await request.text())
})()

const additions = []
const subtractions = []

$(async () => {
    const [progressMap, staticData] = await Promise.all([progressMapPromise, staticDataPromise])
    progressMap.locations = progressMap.locations.filter(x => x.startsWith(alpha2)).map(x => x.substring(alpha2.length + 1).split("-"))
    const staticDataMap = mapOutStaticData(staticData.subdivisions)
    mapOutVisitation(progressMap.locations, staticDataMap)
    const title = $("#title")
    const subdivisionList = $("#subdivision_list")
    const saveButton = $("#save")
    title.text("Edit " + staticData.name)
    console.log(staticData)
    staticData.subdivisions.forEach(subdivision => {
        const entry = $("<li></li>")
        const label = $("<label></label>")
        label.text(subdivision.name)
        entry.append(label)
        if (subdivision.subdivisions.length === 0) {
            const check = $("<input type=\"checkbox\"/>")
            const originalPossition = subdivision.visited
            if (originalPossition) {
                check.prop("checked", true)
            }
            const array = originalPossition ? subtractions : additions
            check.on("click", e => {
                const checked = e.currentTarget.checked
                if (checked === originalPossition) {
                    let index = array.indexOf(subdivision.alpha_2)
                    if (index !== -1) {
                        array.splice(index, 1)
                    }
                }
                else {
                    array.push(subdivision.alpha_2)
                }
            })
            entry.append(check)
        }
        else {
            const next = $("<a href='" + window.location.href + "-" + subdivision.alpha_2 + "'>-&gt;</a>")
            entry.append(next)
        }
        subdivisionList.append(entry)
    })
    saveButton.on("click", async () => {
        let add = additions.map(x => alpha2 + "-" + x)
        let sub = subtractions.map(x => alpha2 + "-" + x)
        let request = await fetch("/api/ProgressMap/Change", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                id: progressMap.id,
                additions: add,
                subtractions: sub
            })
        });
        if (request.status !== 200) {
            console.log("error " + request.status)
            console.log(await request.text())
        }
        window.location.href = "/map.html"
    })
})