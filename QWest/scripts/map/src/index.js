import $ from "jquery";
import svgMap from "svgmap";
$(()=>{
    let map = new svgMap({
        targetElementID: 'svgMap',
        data: {
            data: {
                visited: {
                    name: 'percent visited',
                    format: '{0} %',
                    thresholdMax: 100,
                    thresholdMin: 0
                }
            },
            applyData: 'visited',
            values: window.countryData
        }
    });
})