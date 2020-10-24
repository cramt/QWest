const mapOutVisitation = (progressMap, staticDataMap) => {
    progressMap.forEach(location => {
        let country = staticDataMap[location[0]]
        country.visited = true

        let curr = country;
        for (let i = 1; i < location.length; i++) {
            curr = curr.subdivisions[location[i]]
        }
        curr.visited = true

    })
}

export { mapOutVisitation }