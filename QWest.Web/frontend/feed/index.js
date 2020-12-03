import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { GET, sendRequest } from "../api";

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


        $("body").append(
            $("<div></div>")
                .append(profileHtml)
                .append(
                    $
                )
        )
    }

    const appendMorePosts = async () => {
        const { status, data } = await GET.Post.GetFeed({
            id: user.id,
            amount: 20,
            offset: 0
        })
        if (status !== 200) {
            alert("error " + status)
            console.log(data)
            return
        }
        data.forEach(appendPost)
    }

    await appendMorePosts()

    return;
    $(window).on("scroll", async () => {
        let scrollHeight = $(document).height();
        let scrollPos = $(window).height() + $(window).scrollTop();
        if (((scrollHeight - 300) >= scrollPos) / scrollHeight == 0) {
            await appendMorePosts()
        }
    })
})