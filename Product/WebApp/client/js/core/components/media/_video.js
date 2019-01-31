$('.video-play-button').on('click', function () {
    $(this).hide();
    $('.video-thumbnail').hide();
    $('#VideoComponent').prop('controls', true);
    document.getElementById("VideoComponent").play();
});