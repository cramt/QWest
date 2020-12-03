import $ from "jquery";
import { fetchMeAndUser } from "../whoami"
import { blobToBase64 } from "../blobToBase64";
import autocomplete from "jquery-ui/ui/widgets/autocomplete"
import Cookies from 'js-cookie'
import { POST, sendRequest } from "../api"


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
    const geopoliticalLocationAutocomplete = $("#geopolitical-location-autocomplete")

    if (!new URL(window.location.href).searchParams.get("id")) {
        history.pushState(undefined, undefined, "/profile.html?id=" + user.id)
    }

    logoutButton.on("click", () => {
        Cookies.remove("sessionCookie")
        window.location.href = "/login.html"
    })

    usernameField.text(user.username)

    descriptionField.text(user.description)

    profilePictureContainer.append('<img id="image" width="500px" src="/api/Image/Get?id=' + user.profilePicture + '" />')

    if (!isMe) {
        postContainer.css("display", "none")
        userSettings.text("Add friend")
        userSettings.attr("href", "#")
        userSettings.on("click", async () => {
            const request = await sendRequest("/api/Friendship/AddFriend?id=" + user.id, "POST")
            if (request.status !== 200) {
                alert("error: " + request.status);
                console.log(request.data)
                return
            }
            userSettings.text("Friend added")
            if (!request.data) {
                alert("you are already friends with this person")
            }
        })
    }

    let selectedGeopoliticalLocation = null;

    postButton.on("click", async () => {

        const request = await POST.Post.Upload({
            contents: postContents.val(),
            location: selectedGeopoliticalLocation.id,
            images: await Promise.all(Array.from(postImages[0].files).map(blobToBase64))
        })
        if (request.status === 200) {
            window.location.reload();
            return;
        }
        console.log(request.status)
        console.log(response.data)
    })

    let auto = new autocomplete({
        source: async (request, response) => {
            let searchText = request.term
            let apiResponse = await fetch("api/Geography/NameSearch?searchTerm=" + encodeURI(searchText), {
                method: "GET",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            })
            if (apiResponse.status === 200) {
                response(JSON.parse(await apiResponse.text()).map(x =>
                    ({
                        label: x.alpha_3 ? `The country: ${x.name}` : `The subdivision: ${x.name}`,
                        value: x
                    })
                ))
            }
            else {
                response([])
            }
        },
        select: (ui, e) => {
            console.log(ui, e)
            setTimeout(() => {
                ui.target.value = e.item.label
            }, 0);
            selectedGeopoliticalLocation = e.item.value
        }
    }).element.appendTo(geopoliticalLocationAutocomplete[0])
})