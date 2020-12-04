import $ from "jquery";
import { fetchLogedInUser } from "../whoami"
import Cookies from 'js-cookie'
import autocomplete from "jquery-ui/ui/widgets/autocomplete"
import { blobToBase64 } from "../blobToBase64";

const fetchThisGroup = async () => {
    const url = new URL(window.location.href);
    const id = url.searchParams.get("id")
    if (!id) {
        location.href = "/login.html"
    }
    const response = await fetch("api/Group/Get?id=" + id, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    })
    if (response.status === 404) {
        alert("group doesnt exists")
        return
    }
    else if (response.status !== 200) {
        console.log(await response.text())
        alert("error " + response.status)
        return
    }
    return JSON.parse(await response.text())
}

const fetchFriends = async () => {
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
    return friends
}

const groupPromise = fetchThisGroup()

const userPromise = fetchLogedInUser()

const friendsPromise = fetchFriends();

$(async () => {
    const group = await groupPromise
    console.log(group)
    const user = await userPromise
    const friends = await friendsPromise
    const isOwned = group.members.findIndex(x => x.id === user.id) !== -1
    const logoutButton = $("#logout-button")
    const groupName = $("#group-name")
    const editButton = $("#edit-button")
    const saveChangesButton = $("#save-changes-button")
    const groupNameModal = $("#group-name-modal")
    const groupDescription = $("#group-description")
    const membersList = $("#members-list")
    const progressMap = $("#progress-map")
    const postContainer = $("#post-container")
    const postContents = $("#post-contents")
    const postImages = $("#post-images")
    const postButton = $("#post-button")
    const geopoliticalLocationAutocomplete = $("#geopolitical-location-autocomplete")

    logoutButton.on("click", () => {
        Cookies.remove("sessionCookie")
        window.location.href = "/login.html"
    })

    editButton.on("click", () => {
        groupNameModal.text(group.name)
        groupDescription.text(group.description)
        //TODO: Fix the friend-select menu for inviting friends. it's broken currently because select has a hard time inside modal windows
    })

    groupName.text(group.name)
    group.members.forEach(x => membersList.append(
        $('<li id="member-container"></li>')
            .append(
                $('<a id="member"></a>')
                    .text(x.username + " (" + x.email + ")")
                    .attr("href", "/profile.html?id=" + x.id)
            )
            .append(
                isOwned ?
                    $('<button type="button" id="remove-button" class="btn btn-danger"></button>')
                        .text("remove")
                        .on("click", async () => {
                            const response = await fetch("api/Group/UpdateMembers", {
                                method: "POST",
                                headers: {
                                    'Accept': 'application/json',
                                    'Content-Type': 'application/json'
                                },
                                body: JSON.stringify({
                                    id: group.id,
                                    additions: [],
                                    subtractions: [x.id]
                                })
                            })
                            if (response.status !== 200) {
                                alert("error " + response.status)
                                console.log(await response.text())
                            }
                            else {
                                window.location.reload()
                            }
                        })
                    : undefined)
    ))
    /* What is the purpose of this multi-select thing?
    If you can explain, I'll add it back in. */
    /*
    const membersSelected = []
    if (isOwned) {
        membersList.append(
            $("<li></li>")
                .append(
                    (() => {
                        const select = $("<select multiple='true'></select>")
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
                            select.append(option)
                        })
                        return select
                    })()
                )
        )
    }
    else{
        postContainer.css("display", "none")
    }
    */

    const contentUpdate = async () => {
        let groupname = groupNameModal.val()
        let description = groupDescription.val()
        if (groupname === group.name) {
            groupname = null
        }
        if (description === group.description) {
            description = null
        }
        if (description === null && groupname === null) {
            return;
        }
        const request = await fetch("/api/Group/Update", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                id:group.id,
                name:groupname,
                description
            })
        })
        if (request.status === 200) {
            window.location.reload();
            return;
        }
        window.r2 = request
        alert("error: " + request.status)
        console.log(await request.text())
    }

    saveChangesButton.on("click", async () => {
        await Promise.all([contentUpdate()])

    })

    //TODO: this doesnt work, map.html always renders current user's map
    progressMap.attr("href", "map.html?id=" + group.progressMap.id)

    let selectedGeopoliticalLocation = null;

    postButton.on("click", async () => {
        const request = await fetch("api/Post/Upload", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                contents: postContents.val(),
                location: selectedGeopoliticalLocation ? selectedGeopoliticalLocation.id : null,
                images: await Promise.all(Array.from(postImages[0].files).map(blobToBase64)),
                groupAuthor: group.id
            })
        })
        if (request.status === 200) {
            window.location.reload();
            return;
        }
        console.log(request.status)
        console.log(await request.text())
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