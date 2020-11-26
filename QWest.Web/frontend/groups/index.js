import $ from "jquery";
import "cookie-store"
import { fetchUser } from "../whoami"

const userPromise = fetchUser();

const renderGroups = async (user) => {
    let response = await fetch("api/Group/FetchUsersGroups?id=" + user.id, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    const groupList = $("#group-list")
    if (response.status !== 200) {
        alert("error: " + response.status);
        console.log(await response.text())
        return;
    }
    const groups = JSON.parse(await response.text())
    groups.forEach(x => {
        const entry = $('<li id="single-group"></li>')
        const span = $('<span id="groupname-span"></span>')
        span.text(x.name)
        // Currently no group images
        //const image = $('<img id="image" src="/api/Image/Get?id=' + x.profilePicture + '" />')
        const br = $("<br></br>")
        entry.append(span)
        entry.append(br)
        //entry.append(image)
        friendList.append(entry)
    })
}

$(async () => {
    const user = await userPromise
    const response = await fetch("/api/Friendship/GetFriends", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    if (response.status !== 200) {
        console.log(await response.text())
        alert("error " + response.status)
    }
    const friends = JSON.parse(await response.text())
    const friendsSelect = $("#friends-select")
    const saveChangesButton = $("#save-changes-button")
    const groupName = $("#group-name")
    const descriptionText = $("#description-text")
    const membersSelected = []
    friends.forEach(friend => {
        const option = $(`<option value="${friend.id}"></option>`)
        option.text(friend.username + " (" + friend.email + ")")
        option.on("dblclick", () => {
            const index = membersSelected.indexOf(friend);
            if (index === -1) {
                membersSelected.push(friend)
                option.text(option.text() + " ✓")
            }
            else {
                membersSelected.splice(index, 1)
                const t = option.text()
                option.text(t.substring(0, t.length - 2))
            }
        })
        friendsSelect.append(option)
    })
    saveChangesButton.on("click", async () => {
        const response = await fetch("/api/Group/Add", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name: groupName.val(),
                description: descriptionText.val(),
                members: membersSelected
            })
        })
        if (response.status !== 200) {
            console.log(await response.text())
            alert("error " + response.status)
        }
        window.location = "group.html?id=" + (await response.text())
    })
})

/* TODO: Implement Group requests */
/*
const renderGroupRequests = async (user) => {
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
        const entry = $('<li id="single-user"></li>')
        const span = $('<span id="username-span"></span>')
        span.text(x.username)
        const image = $('<img id="image" src="/api/Image/Get?id=' + x.profilePicture + '" />')
        const acceptButton = $('<a id="accept-button" class="btn btn-lg btn-secondary">Accept</a>')
        const br = $("<br></br>")
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
            if (!JSON.parse(await response.text())) {
                alert("you dont have a friend request from this person")
            }
        })
        entry.append(span)
        entry.append(br)
        entry.append(image)
        entry.append(br)
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
*/