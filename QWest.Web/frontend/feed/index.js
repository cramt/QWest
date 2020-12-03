import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { GET, sendRequest } from "../api";

let currOffset = 0
const fetchAmount = 5;
let fetchingLock = false

$(async () => {
    const user = await fetchLogedInUser()

    const appendPost = (post) => {
        console.log(post)
        const profileHtml = $("<p></p>")
        if (post.groupAuthor) {
            console.log("doing group")
            profileHtml
                .text(post.groupAuthor.name)
        }
        else if (post.userAuthor) {
            console.log("doing user")
            profileHtml
                .text(post.userAuthor.username)
                .append(
                    $("<img/>")
                        .attr("src", "/api/Image/Get?id=" + post.userAuthor.profilePicture)
                )
        }
        else {
            throw new Error("aaaaaaaaa this shouldnt happenF")
        }

        const images = $("<div></div>")

        post.images.forEach(image => {
            images.append(
                $("<img/>")
                    .attr("src", "/api/Image/Get?id=" + image)
            )
        })

        const locationHtml = $("<div></div>")

        if (post.location) {
            locationHtml.append(
                $("<p></p>")
                    .text(post.location.alpha_3 ? `The country: ${post.location.name}` : `The subdivision: ${post.location.name}`,)
            )
        }


        $("body").append(
            $("<div></div>")
                .append(profileHtml)
                .append(
                    $("<p></p>")
                        .text(post.content)
                )
                .append(images)
                .append(locationHtml)
        )
    }

    const appendMorePosts = async () => {
        if (fetchingLock) {
            return;
        }
        fetchingLock = true
        const { status, data } = await GET.Post.GetFeed({
            id: user.id,
            amount: fetchAmount,
            offset: currOffset
        })
        currOffset += data.length
        if (status !== 200) {
            alert("error " + status)
            console.log(data)
            fetchingLock = false
            return
        }
        data.forEach(appendPost)
        fetchingLock = false
    }

    await appendMorePosts()

    $(window).on("scroll", async () => {
        let scrollHeight = $(document).height();
        let scrollPos = $(window).height() + $(window).scrollTop();
        if (((scrollHeight - 300) >= scrollPos) / scrollHeight == 0) {
            await appendMorePosts()
        }
    })
})