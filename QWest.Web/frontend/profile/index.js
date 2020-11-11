import $ from "jquery";
import "cookie-store"
import { fetchMeAndUser } from "../whoami"

const userPromise = fetchMeAndUser();

$(async () => {
    const users = await userPromise;
    const isMe = !users.them
    const user = isMe ? users.me : users.them;
    const usernameField = $("#username-field")
    const descriptionField = $("#description-field");
    const profilePictureContainer = $("#profile-picture-container")
    const logoutButton = $("#logout-button")
    const userSettings = $("#user-settings");

    logoutButton.on("click", async () => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })

    usernameField.text(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append('<img id="image" width="500px" src="/api/Image/Get?id=' + user.profilePicture + '" />')

    if (!isMe) {
        var us = document.getElementById("#user-settings")
        userSettings.text("Add friend")
        us.setAttribute("href", "#")
        userSettings.on("click", async () => {
            const request = await fetch("/api/Friendship/AddFriend?id=" + user.id, {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
            })
            if (request.status !== 200) {
                alert("error: " + request.status);
                console.log(await request.text())
                return
            }
            userSettings.text("Friend added")
            if (!JSON.parse(await request.text())) {
                alert("you are already friends with this person")
            }
        })
    }

})