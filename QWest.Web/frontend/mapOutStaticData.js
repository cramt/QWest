const mapOutStaticData = (staticData) => {
    if (staticData.length === 0) {
        return []
    }
    return staticData.reduce((acc, x) => {
        acc[x.alpha_2 || x.code] = x
        x.subdivision = mapOutStaticData(x.subdivision)
        return acc
    }, {})
}

export { mapOutStaticData }