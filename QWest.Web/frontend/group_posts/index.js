import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { GET, sendRequest } from "../api";

let currOffset = 0
const fetchAmount = 5;
let fetchingLock = false;
const url = new URL(window.location.href);
const groupId = url.searchParams.get("id")

$(async () => {
    const user = await fetchLogedInUser()

    const appendPost = (post) => {
        const profileHtml = $("<p></p>")
        profileHtml
            .text(post.groupAuthor.name)
        
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
                    .text(post.location.alpha_3 ? `The country: ${post.location.name}` : `The subdivision ${post.location.name}`)
            )
        }

        $("body").append(
            $("<div></div>")
                .append(profileHtml)
                .append(
                    $("<p></p>")
                        .text(post.contents)
                )
                .append(images)
                .append(locationHtml)
        )
    }

    const appendMorePosts = async () => {
        if (fetchingLock) {
            return
        }
        fetchingLock = true
        const { status, data } = await GET.Post.GetGroupPosts({
            id: groupId,
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