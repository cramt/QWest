import $ from "jquery";

const url = new URL(window.location.href);
const userPromise = (async () => {
    const id = url.searchParams.get("id")
    let response = undefined
    if (id) {
        response = await fetch("api/User/Id/" + id, {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
        })
    }
    else {
        response = await fetch("api/Login/GetMe", {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
        })
    }
    switch (response.status) {
        case 200:
            break;
        case 404:
            window.location.href = "/login.html"
            return null;
            break;
        default:
            alert("unexpected " + response.status)
            return null;
            break;
    }
    return JSON.parse(await response.text())
})()

$(() => {

})