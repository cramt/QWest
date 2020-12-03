import { fetchLogedInUser } from "../whoami"
import $ from "jquery";
import { GET, sendRequest } from "../api";

const appendMorePosts = async () => {

}

$(async () => {
    const user = await fetchLogedInUser()
    const { status, data } = await GET.Post.GetFeed({
        id: user.id,
        amount: 20,
        offset: 0
    })
    console.log(status, data)

    $(window).on("scroll", async () => {
        if (((scrollHeight - 300) >= scrollPos) / scrollHeight == 0) {
            await appendMorePosts()
        }
    })
})