import $ from "jquery";
import "cookie-store"
import { fetchLogedInUser } from "../whoami"

const userPromise = fetchLogedInUser();

$(async () => {
    const user = await userPromise;
    const logoutButton = $("#logout-button")
    const usernameField = $("#username-field")
    const requestPasswordResetButton = $("#request-password-reset-button")
    const descriptionField = $("#description-field");
    const profilePictureContainer = $("#profile-picture-container")
    const profilePictureField = $("#profile-picture-field")
    const saveButton = $("#save-button")

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

    usernameField.val(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append("<img id=" + "image" + " width=" + "500px" + " src='/api/Image/Get?id=" + user.profilePicture + "' />")

    const profilePictureUpdate = async () => {
        const files = profilePictureField[0].files;
        if (files.length === 0) {
            return;
        }
        const file = files[0];
        const formData = new FormData();
        formData.append("file", file)
        const request = await fetch("/api/User/UpdateProfilePicture", {
            method: "POST",
            body: formData
        })
        if (request.status === 200) {
            return;
        }
        window.r1 = request
        alert("error: " + request.status)
        console.log(await request.text())
    }

    const contentUpdate = async () => {
        let username = usernameField.val()
        let description = descriptionField.val()
        if (username === user.username) {
            username = null
        }
        if (description === user.description) {
            description = null
        }
        if (description === null && username === null) {
            return;
        }
        const request = await fetch("/api/User/Update", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username,
                description
            })
        })
        if (request.status === 200) {
            return;
        }
        window.r2 = request
        alert("error: " + request.status)
        console.log(await request.text())
    }

    saveButton.on("click", async () => {
        await Promise.all([contentUpdate(), profilePictureUpdate()])

    })
})