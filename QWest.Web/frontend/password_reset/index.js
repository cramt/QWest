import $ from "jquery";
const url = new URL(window.location.href);


$(async () => {
    const token = url.searchParams.get("token");
    if (token === null) {
        window.location.href = "/home.html"
    }
    const response = await fetch("/api/User/GetByPasswordResetToken?token=" + encodeURIComponent(token), {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    if (response.status !== 200) {
        console.log(await response.text())
        alert(response.status)
        //window.location.href = "/home.html"
    }
    const user = JSON.parse(await response.text())
    const header = $("#header-with-username");
    const password = $("#password")
    const confirmPassword = $("#confirm-password")
    const saveButton = $("#save-button")
    const message = $("#message")
    header.text("change password of user: " + user.username)


    const processClick = async () => {
        const passwordVal = password.val()
        const confirmPasswordVal = confirmPassword.val();
        if (passwordVal.length < 8) {
            message.text("length of the password needs to be larger than 7")
            return
        }
        if (passwordVal !== confirmPasswordVal) {
            message.text("passwords dont match")
            return
        }
        const response = await fetch("api/User/ConfirmPasswordReset", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                token,
                password: passwordVal
            })
        })
        if (response.status === 200) {
            window.location.href = "/login.html"
        }
        else {
            alert("error: " + response.status)
            console.log(await response.text())
        }
    }

    const processEnter = (e) => {
        if (e.which === 13) {
            processClick()
        }
    }

    password.on("keypress", processEnter)
    confirmPassword.on("keypress", processEnter)
    saveButton.on("click", processClick)
})