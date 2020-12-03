import { GET } from "./api";

const url = new URL(window.location.href);
const fetchUser = async () => {
    const id = url.searchParams.get("id")
    let response = undefined
    if (id) {
        response = await GET.User.Get({ id })
    }
    else {
        response = await GET.Login.GetMe()
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
    const getMeRequest = GET.Login.GetMe()
    const result = {}
    const id = url.searchParams.get("id")
    let response = undefined;
    if (id) {
        response = await GET.User.Get({ id })
        switch (response.status) {
            case 200:
                result.them = response.data
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
    }
    response = await getMeRequest;
    switch (response.status) {
        case 200:
            result.me = response.data
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
    return result;
}

const fetchLogedInUser = async () => {
    let response = await GET.Login.GetMe()
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

export { fetchUser, fetchLogedInUser, fetchMeAndUser }