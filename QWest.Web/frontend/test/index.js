import $ from "jquery";
import "cookie-store"

$(async () => {
    $('.mdb-select').select2({
        placeholder: "Select friends", 
        tags: true,
        tokenSeperators: ['/',',',','," "]
    });
});

$(async () => {
    let logoutButton = $("#logout-button")
    let userSettings = $("#user-settings")
    let requestPasswordResetButton = $("#request-password-reset-button")

    logoutButton.on("click", async () => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })

    requestPasswordResetButton.on("click", async () => {
        const response = await fetch("api/User/RequestPasswordReset", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
        })

        if (response.status === 200) {
            alert("you should receive an email with instructions on password reset")
        }
        else {
            alert("error: " + response.status)
            console.log(await response.text())
        }
    })


})