import $ from "jquery";
import "cookie-store"


$(async () => {
    let logoutButton = $("#logout-button")
    let userSettings = $("#user-settings")

    logoutButton.on("click", async() => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })
})