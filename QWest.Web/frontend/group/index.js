import $ from "jquery";
import { fetchLogedInUser } from "../whoami"

const fetchThisGroup = async () => {
    const url = new URL(window.location.href);
    const id = url.searchParams.get("id")
    if (!id) {
        //location.href = "/login.html"
    }
    const response = await fetch("api/Group/Get?id=" + id, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    if (response.status === 404) {
        alert("group doesnt exists")
        return
    }
    else if (response.status !== 200) {
        console.log(await response.text())
        alert("error " + response.status)
        return
    }
    return JSON.parse(await response.text())
}

const groupPromise = fetchThisGroup()

const userPromise = fetchLogedInUser()

$(async () => {
    const group = await groupPromise
    const user = await userPromise
    const isOwned = group.members.findIndex(x => x.id === user.id) !== -1
    console.log(group)
})