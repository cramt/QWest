import $ from "jquery"

const friendsResponsePromies = fetch("/api/Friendship/GetRequests", {
    method: "GET",
    headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    },
})

$(async () => {
    const friendsResponse = await friendsResponsePromies;
    const friendsRequests = JSON.parse(await friendsResponse.text());
    const friendRequestContainer = $("#friend-request-container")

    if (friendsRequests.length === 0) {
        friendRequestContainer.text("you have no friend requests")
    }
    else {
        const list = $("<ul>/<ul>")
        friendsRequests.forEach(request => {
            const entry = $("<li></li>")
            const text = $("<span></span>")
            entry.append(text)
            text.text(request.username)
            const button = $("<button>acccept</buttonb>")
            button.on("click", async () => {
                const response = await fetch("api/Friendship/AcceptFriendRequest?id=" + request.id, {
                    method: "POST",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                })
                if(response.status === 200){
                    window.location.reload()
                }
                else{
                    console.log(await response.text())
                    alert("error " + response.status)
                }
            })
            entry.append(button)
            list.append(entry)
        })
        friendRequestContainer.append(list)
    }
})