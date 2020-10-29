import $ from "jquery";
import "cookie-store"
import { fetchLogedInUser } from "../whoami"

const userPromise = fetchLogedInUser();

$(async () => {
    const user = await userPromise;
    const logoutButton = $("#logout-button")
    const usernameField = $("#username-field")
    const descriptionField = $("#description-field");
    const profilePictureContainer = $("#profile-picture-container")
    let logoutButton = $("#logout-button")

    logoutButton.on("click", async() => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })
    
    usernameField.val(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append("<img id=" + "image" + " width=" + "500px" + " src='/api/Image/Get?id=" + user.profilePicture + "' />")
    
})