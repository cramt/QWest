const mapOutStaticDataReq = (staticData, staticAcc) => {
    if (!staticData || staticData.length === 0) {
        return []
    }
    return staticData.reduce((acc, x) => {
        acc[x.alpha_2] = x
        staticAcc[x.id] = x;
        x.subdivisions = mapOutStaticDataReq(x.subdivisions, staticAcc)
        return acc
    }, {})
}

const mapOutStaticData = (staticData) => {
    const staticAcc = {}
    const result = mapOutStaticDataReq(staticData, staticAcc);
    return {
        ...staticAcc,
        ...result
    }
}

export { mapOutStaticData }