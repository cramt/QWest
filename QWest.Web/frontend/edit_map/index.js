import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { mapOutStaticData } from "../mapOutStaticData";
import { mapOutVisitation } from "../mapOutVisitation";

const url = new URL(window.location.href);
const alpha2 = url.searchParams.get("alpha2")
if (alpha2 === undefined) {
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
    let request = await fetch("/api/Subdivision/Get?alpha2=" + alpha2, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    return JSON.parse(await request.text())
})()

$(async () => {
    const [progressMap, staticData] = await Promise.all([progressMapPromise, staticDataPromise])
    progressMap.locations = progressMap.locations.map(x => x.substring(alpha2.length + 1).split("-"))
    const staticDataMap = mapOutStaticData(staticData.subdivision)
    mapOutVisitation(progressMap.locations, staticDataMap)
    const title = $("#title")
    const subdivisionList = $("#subdivision_list")
    title.text("Edit " + staticData.name)
    staticData.subdivision.forEach(subdivision => {
        const entry = $("<li></li>")
        const label = $("<label></label>")
        label.text(subdivision.name)
        entry.append(label)
        if (subdivision.subdivision.length === 0) {
            const check = $("<input type=\"checkbox\"/>")
            if (subdivision.visited) {
                check.prop("checked", true)
            }
            entry.append(check)
        }
        else {
            const next = $("<a href='" + window.location.href + "-" + subdivision.code + "'>-&gt;</a>")
            entry.append(next)
        }
        subdivisionList.append(entry)
    })
})