function getGradientColor(startColor, endColor, percent) {
    // strip the leading # if it's there
    percent /= 100;
    startColor = startColor.replace(/^\s*#|\s*$/g, '');
    endColor = endColor.replace(/^\s*#|\s*$/g, '');

    // convert 3 char codes --> 6, e.g. `E0F` --> `EE00FF`
    if(startColor.length == 3) {
        startColor = startColor.replace(/(.)/g, '$1$1');
    }

    if(endColor.length == 3) {
        endColor = endColor.replace(/(.)/g, '$1$1');
    }

    // get colors
    let startRed = parseInt(startColor.substr(0, 2), 16),
        startGreen = parseInt(startColor.substr(2, 2), 16),
        startBlue = parseInt(startColor.substr(4, 2), 16);

    let endRed = parseInt(endColor.substr(0, 2), 16),
        endGreen = parseInt(endColor.substr(2, 2), 16),
        endBlue = parseInt(endColor.substr(4, 2), 16);

    // calculate new color
    let diffRed = endRed - startRed;
    let diffGreen = endGreen - startGreen;
    let diffBlue = endBlue - startBlue;

    diffRed = ( (diffRed * percent) + startRed ).toString(16).split('.')[0];
    diffGreen = ( (diffGreen * percent) + startGreen ).toString(16).split('.')[0];
    diffBlue = ( (diffBlue * percent) + startBlue ).toString(16).split('.')[0];

    // ensure 2 digits by color
    if( diffRed.length == 1 ) diffRed = '0' + diffRed
    if( diffGreen.length == 1 ) diffGreen = '0' + diffGreen
    if( diffBlue.length == 1 ) diffBlue = '0' + diffBlue

    return '#' + diffRed + diffGreen + diffBlue;
}