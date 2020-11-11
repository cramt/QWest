import $ from "jquery";
import "cookie-store"
import { fetchMeAndUser } from "../whoami"
import { formDataFromObject } from "../formDataFromObject";
import { blobToBase64 } from "../blobToBase64";

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
    const postContainer = $("#post-container")
    const postContents = $("#post-contents")
    const postImages = $("#post-images")
    const postButton = $("#post-button")

    logoutButton.on("click", async () => {
        await cookieStore.delete("sessionCookie")
        window.location.href = "/login.html"
    })

    usernameField.text(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append('<img id="image" width="500px" src="/api/Image/Get?id=' + user.profilePicture + '" />')

    if (!isMe) {
        postContainer.css("display", "none")
        userSettings.text("Add friend")
        userSettings.removeAttr("href")
        userSettings.href("#")
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


    postButton.on("click", async () => {
        const request = await fetch("api/Post/Upload", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                contents: postContents.val(),
                location: null,
                images: await Promise.all(Array.from(postImages[0].files).map(blobToBase64))
            })
        })
        console.log(request.status)
        console.log(await request.text())
    })
})