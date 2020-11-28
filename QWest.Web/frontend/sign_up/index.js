import $ from "jquery";
import Cookies from 'js-cookie'

$(() => {
    //from http://emailregex.com/
    const emailRegex = /(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/
    let passwordInput = $("#password_input")
    let passwordInputConfirm = $("#password_input_confirm")
    let emailInput = $("#email_input")
    let usernameInput = $("#username_input")
    let message = $("#message")
    let registerButton = $("#register_button")


    const validateRegister = (username, email, password, passwordConfirm) => {
        if (passwordConfirm != password) {
            message.text("passwords must match")
            return false
        }
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
        let password = passwordInput.val()
        let email = emailInput.val()
        let username = usernameInput.val()
        let passwordConfirm = passwordInputConfirm.val()
        if (validateRegister(username, email, password, passwordConfirm)) {
            fetch("api/SignUp/Register", {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    password,
                    email,
                    username
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
                        Cookies.set("sessionCookie", sessionCookie)
                        window.location.href = "/profile.html"
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

    registerButton.on("click", processClick)
    passwordInput.on("keypress", processEnter)
    passwordInputConfirm.on("keypress", processEnter)
    emailInput.on("keypress", processEnter)
    usernameInput.on("keypress", processEnter)
})