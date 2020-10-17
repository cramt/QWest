const url = new URL(window.location.href);
const fetchUser = async () => {
    const id = url.searchParams.get("id")
    let response = undefined
    if (id) {
        response = await fetch("api/User/Get/" + id, {
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
            console.log(await response.text())
            return null;
            break;
    }
    return JSON.parse(await response.text())
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

export { fetchUser, fetchLogedInUser }