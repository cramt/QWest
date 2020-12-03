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

const uriEncode = (obj) => {
    let str = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return str.join("&");
}

const constructProxyChain = (get, chain) => {
    return new Proxy(() => { }, {
        get: (_, value) => {
            let c = chain.slice()
            c.push(value)
            return constructProxyChain(get, c)
        },
        apply: (_b, _a, args) => {
            let argument = args[0] || {}
            let path = chain.join("/")
            let body = undefined
            let method = undefined
            if (get) {
                let uriString = uriEncode(argument);
                if (uriString.length !== 0) {
                    path += "?" + uriString
                }
                method = "GET"
            }
            else {
                body = argument
                method = "POST"
            }
            return sendRequest(path, method, body)
        }
    })
}

const [POST, GET] = [false, true].map(x => constructProxyChain(x, ["api"]))

window.POST = POST
window.GET = GET

export { sendRequest, POST, GET }