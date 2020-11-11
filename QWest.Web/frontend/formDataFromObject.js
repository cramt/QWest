const formDataFromObject = o => {
    const formData = new FormData();
    Object.keys(o).forEach(key => formData.append(key, o[key]));
    return formData;
}

export { formDataFromObject }