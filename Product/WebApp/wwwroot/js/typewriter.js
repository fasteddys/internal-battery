var i = 0;
var elementId;
var typeSpeed;

function setTypewriterVars(eId, tSpeed) {
    elementId = eId;
    typeSpeed = tSpeed;
}
function typeWriter() {
    
    var typeText = $('#' + elementId).data("typetext");
    if (i < typeText.length) {
        var character = typeText.charAt(i);
        document.getElementById(elementId).innerHTML += character;
        i++;
        setTimeout(typeWriter, typeSpeed);
    }
}

