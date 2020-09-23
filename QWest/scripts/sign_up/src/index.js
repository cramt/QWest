import $ from "jquery";

$(() => {
    //from http://emailregex.com/
    const emailRegex = /(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/
    let passwordInput = $("#password_input")
    let emailInput = $("#email_input")
    let usernameInput = $("#username_input")
    let errorMessage = $("#error_message")
    let registerButton = $("#register_button")


    const validateRegister = (username, email, password) => {
        if (!emailRegex.test(email)) {
            errorMessage.text("not a valid email address")
            return false
        }
        if (password.length < 8) {
            errorMessage.text("passwords cannot be shorter than 8 characters")
            return false
        }
        return true
    }


    registerButton.on("click", () => {
        let password = passwordInput.val()
        let email = emailInput.val()
        let username = usernameInput.val()
        if (validateRegister(username, email, password)) {
            fetch("api/sign_up/register", {
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
                    x.text().then(text=>{
                        errorMessage.text("error " + status + " happened, " + text)
                    })
                }
                else{
                    errorMessage.text("success")
                }
            })
        }
    })
})