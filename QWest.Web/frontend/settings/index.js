import $ from "jquery";
import "cookie-store"


$(async () => {
    //from http://emailregex.com/
    const emailRegex = /(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/
    
    let logoutButton = $("#logout-button")
    let updateall = $("#updateall")
    let usernameInput = $("#username-input")
    let emailInput = $("#email-input")
    let bioInput = $("#bio-input")
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

    const validateRegister = (username, email, bio) => {
        if (!emailRegex.test(email)) {
            message.text("not a valid email address")
            return false
        }
        if (password.length < 8) {
            message.text("passwords cannot be shorter than 8 characters")
            return false
        }
        return true
    }

    const processClick = () => {
        let email = emailInput.val()
        let username = usernameInput.val()
        let bio = bioInput.val()
        if (validateRegister(username, email, bio)) {
            fetch("api/User/Update", {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username,
                    email,
                    bio
                })
            }).then(x => {
                let status = x.status
                if (status < 200 || status > 299) {
                    x.text().then(text => {
                        message.text("error " + status + " happened, " + text)
                    })
                }
                else {
                    message.text("success")
                    x.text().then(JSON.parse).then(sessionCookie => {
                        cookieStore.set("sessionCookie", sessionCookie).then(() => {
                            window.location.href = "/profile.html"
                        })
                    })
                }
            })
        }
    }

    const processEnter = (e) => {
        if (e.which === 13) {
            processClick()
        }
    }

    updateall.on("click", processClick)
    usernameInput.on("keypress", processEnter)
    bioInput.on("keypress", processEnter)
    emailInput.on("keypress", processEnter)

})