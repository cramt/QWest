const url = new URL(window.location.href);
const fetchUser = async () => {
    const id = url.searchParams.get("id")
    let response = undefined
    if (id) {
        response = await sendRequest("api/User/Get/" + id)
    }
    else {
        response = await sendRequest("api/User/Get/" + id)
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
            console.log(response.data)
            return null;
            break;
    }
    return response.data
}

const fetchMeAndUser = async () => {
    const getMeRequest = fetch("api/Login/GetMe", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    const result = {}
    const id = url.searchParams.get("id")
    let response = undefined;
    if (id) {
        response = await fetch("api/User/Get/" + id, {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
        })
        switch (response.status) {
            case 200:
                result.them = JSON.parse(await response.text())
                break;
            case 404:
                window.location.href = "/login.html"
                return null;
                break;
            default:
                alert("unexpected " + response.status)
                console.log(await response.text())
                return null;
                break;
        }
    }
    response = await getMeRequest;
    switch (response.status) {
        case 200:
            result.me = JSON.parse(await response.text())
            break;
        case 404:
            window.location.href = "/login.html"
            return null;
            break;
        default:
            alert("unexpected " + response.status)
            console.log(await response.text())
            return null;
            break;
    }
    return result;
}

const fetchLogedInUser = async () => {
    let response = await fetch("api/Login/GetMe", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    switch (response.status) {
        case 200:
            break;
        case 404:
            window.location.href = "/login.html"
            return null;
            break;
        default:
            alert("unexpected " + response.status)
            console.log(await response.text())
            return null;
            break;
    }
    return JSON.parse(await response.text())
}

export { fetchUser, fetchLogedInUser, fetchMeAndUser }