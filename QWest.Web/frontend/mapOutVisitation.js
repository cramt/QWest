const mapOutVisitation = (progressMap, staticDataMap) => {
    progressMap.forEach(location => {
        let country = staticDataMap[location[0]]
        if (location.length == 1) {
            country.visited = true
        }
        else {
            let curr = country;
            for (let i = 1; i < location.length; i++) {
                curr = curr.subdivision[location[i]]
            }
            curr.visited = true
        }
    })
}

export { mapOutVisitation }