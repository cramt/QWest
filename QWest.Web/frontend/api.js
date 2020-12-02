import Cookies from "js-cookie"

const sendRequest = async (url, method = "GET", body = undefined) => {
    if (typeof body === "object") {
        body = JSON.stringify(body)
    }
    let request = await fetch(url, {
        method: method,
        headers: {
            'Authorization': Cookies.get("sessionCookie"),
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: body
    });
    let data = await request.text();
    try {
        let a = JSON.parse(data)
        data = a
    }
    catch (_) { }
    return {
        status: request.status,
        data: data
    }
}

export { sendRequest }