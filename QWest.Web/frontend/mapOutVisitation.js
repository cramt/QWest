const mapOutVisitation = (progressMap, staticDataMap) => {
    progressMap.forEach(locationid => {
        staticDataMap[locationid].visited = true
    })
}

export { mapOutVisitation }