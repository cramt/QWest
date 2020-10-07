import $ from "jquery";
import "cookie-store"
import { userPromise } from "../whoami";


$(async () => {
    //userPromise
    let logoutButton = $("#logout-button")

    logoutButton.on("click", async() => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })
    
})