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

    logoutButton.on("click", async () => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })

    usernameField.text(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append('<img id="image" width="500px" src="/api/Image/Get?id=' + user.profilePicture + '" />')

})