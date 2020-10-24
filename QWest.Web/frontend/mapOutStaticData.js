const mapOutStaticData = (staticData) => {
    if (!staticData || staticData.length === 0) {
        return []
    }
    return staticData.reduce((acc, x) => {
        acc[x.alpha_2] = x
        x.subdivisions = mapOutStaticData(x.subdivisions)
        return acc
    }, {})
}

export { mapOutStaticData }