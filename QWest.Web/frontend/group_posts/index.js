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
    const postsContainer = $('#posts-container')

    const appendPost = (post) => {
        const singlePost = $('<div id="single-post" class="row mb-3 w3-container w3-pale-blue w3-leftbar w3-border-blue"></div>')
        const postElementAuthor = $('<div id="post-element-author" class="col-lg-4"></div>')
        const postElementContents = $('<div id="post-element-contents" class="col-lg-4">')
        const postElementImages = $('<div id="post-element-images" class="col-lg-4"></div>')
        const postName = $('<h4 id="post-name"></h4>')
        const postLocation = $('<p id="post-location"></p>')
        const postContents = $('<p id="post-contents"></p>')
        const postImages = $('<img id="post-images" height="300px">')
        const viewGroup = $('#view-group')

        viewGroup.attr("href", "group.html?id=" + groupId)

        //Add author, contents and location
        if (post.groupAuthor) {
            postName
                .text(post.groupAuthor.name)
            if (post.location) {
                postLocation
                    .text(post.location.alpha_3 ? `The country: ${post.location.name}` : `The subdivision: ${post.location.name}`,)
            }
            postContents
                .text(post.contents)
            postElementAuthor
                .append(postName)
                .append(postLocation)
            postElementContents
                .append(postContents)
        }

        else {
            throw new Error("aaaaaaaaa this shouldnt happenF")
        }

        //Add post image(s)
        post.images.forEach(image => {
            postElementImages.append(
                postImages
                    .attr("src", "/api/Image/Get?id=" + image)
            )
        })

        // Adding the line of code below breaks all posts :^)
        
        //const canEdit = post.groupAuthor.map(x => x.id === groupId)
        const editButton = $('<button id="edit-button" type="button" class="btn btn-info">Edit post</button>')
        const editButtonWrapper = $('<a id="edit-button-wrapper"></a>')
        
        //Add edit button if needed
        if(true) {
            editButtonWrapper.attr("href", "/edit_post?id=" + groupId)
            editButtonWrapper.append(editButton)
        }
        

        // Merge it all into a single post
        postsContainer
            .append(singlePost
                .append(editButtonWrapper)
                .append(postElementAuthor)
                .append(postElementContents)
                .append(postElementImages)
                .append("<br>")
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
        window.requestAnimationFrame(() => {
            fetchingLock = false
        })
    }

    await appendMorePosts()

    $("#scroller").on("scroll", async () => {
        let scrollHeight = $("#posts-container").height()
        let scrollPos = $("#scroller").height() + $("#scroller").scrollTop();
        if (((scrollHeight - 300) >= scrollPos) / scrollHeight == 0) {
            await appendMorePosts()
        }
    })
})