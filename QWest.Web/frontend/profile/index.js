import $ from "jquery";
import "cookie-store"


$(async () => {
    let logoutButton = $("#logout-button")

    logoutButton.on("click", async() => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })
    
})