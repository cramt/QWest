import $ from "jquery";
import Cookies from "js-cookie"
import { sendRequest } from "../api";

$(() => {
    //from http://emailregex.com/
    const emailRegex = /(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/
    let passwordInput = $("#password_input")
    let emailInput = $("#email_input")
    let message = $("#message")
    let loginButton = $("#login_button")

    const validateRegister = (email, password) => {
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

    const processClick = async () => {
        let password = passwordInput.val()
        let email = emailInput.val()
        if (validateRegister(email, password)) {
            const { status, data } = await sendRequest("api/Login/Login", "POST", {
                password,
                email,
            })

            if (status === 401) {
                message.text("email and password does not match")
            }
            else if (status < 200 || status > 299) {
                message.text("error " + status + " happened, " + data)
            }
            else {
                message.text("success")
                Cookies.set("sessionCookie", data)
                window.location.href = "/profile.html"
            }

        }
    }

    const processEnter = (e) => {
        if (e.which === 13) {
            processClick()
        }
    }

    loginButton.on("click", processClick)
    passwordInput.on("keypress", processEnter)
    emailInput.on("keypress", processEnter)
})