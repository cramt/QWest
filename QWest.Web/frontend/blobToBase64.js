const blobToBase64 = blob => new Promise((resolve, reject) => {
    var reader = new FileReader();
    reader.readAsDataURL(blob);
    reader.onloadend = () => {
        resolve(reader.result)
    }
})

export {blobToBase64}