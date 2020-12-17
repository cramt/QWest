import $ from "jquery";
import autocomplete from "jquery-ui/ui/widgets/autocomplete";
import Cookies from 'js-cookie'
import { POST, sendRequest } from "../api"
import { blobToBase64 } from "../blobToBase64";
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
    if (!post.images) {
        post.images = []
    }
    console.log(post)
    if (post.userAuthor) {
        if (post.userAuthor.id !== user.id) {
            window.location.href = "/login.html"
            return
        }
    }
    else if (post.groupAuthor) {
        if (!post.groupAuthor.map(x => x.id).includes(user.id)) {
            window.location.href = "/login.html"
            return
        }
    }
    
    //THIS CODE BELOW IS NOT EXECUTED WHEN IT'S A GROUP POST???
    const logoutButton = $("#logout-button")
    const postContents = $("#post-contents")
    const updateButton = $('#update-button')
    postContents.text(post.contents)

    logoutButton.on("click", () => {
        Cookies.remove("sessionCookie")
        window.location.href = "/login.html"
    })

    const geopoliticalLocationAutocomplete = $("#geopolitical-location-autocomplete")

    updateButton.on("click", async () => {
        post.contents = postContents.val();
        console.log(post)
        const request = await POST.Post.Update(post)
        if (request.status === 200) {
            window.location.reload();
            return;
        }
        console.log(request.status)
        console.log(response.data)
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