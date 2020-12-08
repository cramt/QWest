const { GET } = require("../api");
const { fetchLogedInUser } = require("../whoami");

const userPromise = fetchLogedInUser()

const fetchPost = async () => {
    const id = new URL(window.location.href).searchParams.get("id");
    if (!id) {
        window.location.href = "/login.html"
        return
    }
    let { status, data } = await GET.Post.Get({
        id
    })
    if (status === 404) {
        alert("post doesnt exists")
        window.location.href = "/login.html"
        return
    }
    if (status !== 200) {
        console.log(data)
        alert("error " + status)
        return
    }
    return data
}

const postPromise = fetchPost();

$(async () => {
    const user = await userPromise;
    const post = await postPromise;
    if (!(post.userAuthor.id === user.id || post.groupAuthor.map(x => x.id).includes(user.id))) {
        window.location.href = "/login.html"
        return
    }

})