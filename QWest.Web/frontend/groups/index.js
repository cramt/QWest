import $ from "jquery";
import { fetchUser } from "../whoami"
import "select2"

const userPromise = fetchUser();

const fetchGroups = async () => {
    const response = await fetch("api/Group/FetchUsersGroups?userId=null", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    if (response.status !== 200) {
        console.log(await response.text())
        alert("error " + response.status)
        return
    }
    return JSON.parse(await response.text())
}

const groupsPromise = fetchGroups()

$(".mul-select").select2({
    placeholder: "select friends",
    tags: true,
    tokenSeparators: ['/', ',', ';', " "]
});

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



    const groupList = $("#group-list")
    const groups = await groupsPromise
    groups.forEach(x => {
        groupList.append(
            $("<li></li>")
            .append(
                $("<a></a>")
                .text(x.name)
                .attr("href", "/group.html?id=" + x.id)
            )
            
        )
    })
})
