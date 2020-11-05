import $ from "jquery";
import "cookie-store"
import { fetchMeAndUser } from "../whoami"

const userPromise = fetchMeAndUser();

const renderFriends = async (user) => {
    let response = await fetch("api/Friendship/GetFriends?id=" + user.id, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    const friendList = $("#friend-list")
    if (response.status !== 200) {
        alert("error: " + response.status);
        console.log(await response.text())
        return;
    }
    const users = JSON.parse(await response.text())
    users.forEach(x => {
        const entry = $("<li></li>")
        const span = $("<span></span>")
        span.text(x.username)
        const image = $('<img id="image" width="500px" src="/api/Image/Get?id=' + x.profilePicture + '" />')

        entry.append(span)
        entry.append(image)
        friendList.append(entry)
    })
}

const renderFriendRequests = async (user) => {
    let response = await fetch("api/Friendship/GetRequests", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    const friendRequestContainer = $("#friend-request-container")
    const friendRequestList = $("#friend-request-list")
    friendRequestContainer.css("display", "block")
    if (response.status !== 200) {
        alert("error: " + response.status);
        console.log(await response.text())
        return;
    }
    const users = JSON.parse(await response.text())
    users.forEach(x => {
        const entry = $("<li></li>")
        const span = $("<span></span>")
        span.text(x.username)
        const image = $('<img id="image" width="500px" src="/api/Image/Get?id=' + x.profilePicture + '" />')
        const acceptButton = $("<button>accept</button>")
        acceptButton.on("click", async () => {
            let response = await fetch("api/Friendship/AcceptFriendRequest?id=" + x.id, {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
            })
            if (response.status !== 200) {
                alert("error: " + response.status);
                console.log(await response.text())
                return;
            }
        })
        entry.append(span)
        entry.append(image)
        entry.append(acceptButton)
        friendRequestList.append(entry)
    })
}

$(async () => {
    const users = await userPromise;
    const isMe = !users.them
    const user = isMe ? users.me : users.them;
    if (isMe) {
        renderFriendRequests(user)
    }
    renderFriends(user)
})